using CustomSpectreConsole;
using Newtonsoft.Json;
using OfficeOpenXml;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FriendManager.BAL.FriendTech;
using FriendManager.BAL.FriendTechTracker.BAL;
using TL;
using WTelegram;
using CustomSpectreConsole.Settings;
using FriendManager.Services;

namespace FriendManager.Functions
{
    public class TelegramManager : ManagerBase<TelegramManager>
    {
        #region Properties

        public WTelegram.Client TelegramClient { get; set; }
        public User CurrentUser { get; set; }
        public override string DisplayName => "Telegram Manager";

        #endregion

        public override async void Run()
        {
            if (TelegramClient == null)
                TelegramClient = new WTelegram.Client(Config);

            if (CurrentUser == null)
                CurrentUser = Task.Run<User>(async () => await TelegramClient.LoginUserIfNeeded()).Result;

            AnsiConsole.MarkupLine("You are now logged in as as [blue]" + CurrentUser + "[/] (id " + CurrentUser.id + ")\n");

            await RunProgramLoop();
        }

        #region Private API

        private string Config(string what)
        {
            switch (what)
            {
                case "api_id": return XMLSettings.GetSetting(Setting.TelegramAPIKey, Validate);
                case "api_hash": return XMLSettings.GetSetting(Setting.TelegramAPIHash, Validate);
                case "phone_number": return XMLSettings.GetSetting(Setting.TelegramPhoneNumber, Validate);
                case "verification_code": Console.Write("Enter the verification code sent to your Telegram app: "); return Console.ReadLine();
                default: return null; // let WTelegramClient decide the default config
            }
        }

        private bool Validate(Setting node, string value)
        {
            bool validated = true;

            if(node.Name == Setting.TelegramPhoneNumber.Name)
            {
                validated = value.All(x => char.IsDigit(x));

                if (!validated)
                    Console.WriteLine("Invalid phone number.");
            }

            return validated;
        }

        protected override List<ListOption> GetListOptions()
        {
            List<ListOption> listOptions = new List<ListOption>();

            listOptions.Add(new ListOption("Show Holders", ShowHolders));
            listOptions.Add(new ListOption("Remove Invalid Users", RemoveUsersPrompt));
            listOptions.Add(new ListOption("Remove Invalid Users On Schedule", RemoveUsersSchedule));
            listOptions.Add(new ListOption("Manage Excluded Users", ManageExcludedUsers));
            listOptions.AddRange(base.GetListOptions());
            listOptions.Add(GetHelpOption());

            return listOptions;
        }

        private Channel GetChannel()
        {
            Messages_Chats chatList = Task.Run<Messages_Chats>(async () => await TelegramClient.Messages_GetAllChats()).Result;
            string chatId = XMLSettings.GetValue(Setting.TelegramChatId);

            while (string.IsNullOrEmpty(chatId))
            {
                List<ListOption> listOptions = new List<ListOption>();
                listOptions.Add(new ListOption("View Chats", () =>
                {
                    foreach (KeyValuePair<long, ChatBase> pair in chatList.chats.OrderBy(x => x.Value.Title))
                    {
                        if (!pair.Value.IsActive)
                            continue;

                        AnsiConsole.MarkupLine("[pink1]" + pair.Value.Title + "[/]");
                        AnsiConsole.MarkupLine(pair.Value.ID.ToString());
                        AnsiConsole.WriteLine();
                    }
                }));
                listOptions.Add(new ListOption("Enter Chat Id", () =>
                {
                    chatId = XMLSettings.GetSetting(Setting.TelegramChatId, (node, value) => chatList.chats.Any(x => x.Value.ID == long.Parse(value)));
                }));
                listOptions.Add(new ListOption(GlobalConstants.SelectionOptions.ReturnToMenu, () => throw new Exception(GlobalConstants.Commands.CANCEL)));

                SelectionPrompt<ListOption> prompt = new SelectionPrompt<ListOption>();
                prompt.Title = string.Format("Enter in the chat ID of the chat you wish to manage.  To view available chats, select 'View Chats'");
                prompt.AddChoices(listOptions);

                ListOption choice = AnsiConsole.Prompt(prompt);
                choice.Function();
            }

            Channel channel = (Channel)chatList.chats[long.Parse(chatId)];

            return channel;
        }

        private void RemoveUsersSchedule()
        {
            string removalInterval = XMLSettings.GetSetting(Setting.TelegramPurgeInterval, (node, value) => value.All(x => char.IsDigit(x)) && int.Parse(value) > 0);
            int interval = int.Parse(removalInterval);

            while (true)
            {
                RemoveUsers(true);
                Thread.Sleep((int)TimeSpan.FromMinutes(interval).TotalMilliseconds);
            }
        }

        private void RemoveUsersPrompt()
        {
            RemoveUsers(false);
        }

        private void RemoveUsers(bool executeImmediate)
        {
            Channel channel = GetChannel();

            List<Holder> validUsers = FriendTechService.GetHolderDetails().Users ?? new List<Holder>();
            List<string> invalidUserNames = new List<string>();

            Rule rule = new Rule(string.Format("\n[blue]Beginning User Removal Process[/]\n")).DoubleBorder<Rule>();
            AnsiConsole.Write(rule);

            List<User> invalidUsers = new List<User>();
            string currentExcludedUsers = XMLSettings.GetValue(Setting.TelegramExcludedUsers);
            List<string> excludedUsers = currentExcludedUsers != null ? currentExcludedUsers.Split(',').ToList() : new List<string>();

            for (int offset = 0; ;)
            {
                var userList = Task.Run<Channels_ChannelParticipants>(async () => await TelegramClient.Channels_GetParticipants(channel, null, offset)).Result;

                foreach (var (id, user) in userList.users)
                {
                    if (!user.IsBot && !(user.MainUsername == CurrentUser.MainUsername))
                    {
                        bool isValid = false;

                        isValid = excludedUsers.Contains(user.MainUsername) || validUsers.Any(x => x.UserMapping.TelegramUserName != null && string.Equals(x.UserMapping.TelegramUserName.ToLower(), user.MainUsername.ToLower()));
                       
                        if (!isValid)
                            invalidUsers.Add(user);
                    }
                }

                offset += userList.participants.Length;
                if (offset >= userList.count || userList.participants.Length == 0) break;
            }

            if (!invalidUsers.Any())
            {
                AnsiConsole.MarkupLine("\n[green]All Users Valid![/]\n\n");
                return;
            }
            else
            {
                Action review = () =>
                {
                    AnsiConsole.WriteLine();
                    invalidUsers.ForEach(x => AnsiConsole.MarkupLine("Invalid User: [red]{0}[/]", x.MainUsername));
                    AnsiConsole.WriteLine(); AnsiConsole.WriteLine();
                };

                Action execute = () =>
                {
                    executeImmediate = true;
                    invalidUsers.ForEach(user => Task.Run(async () => await DeleteChatUser(channel, user)));
                };

                if (executeImmediate)
                    execute();

                while (!executeImmediate)
                {
                    List<ListOption> listOptions = new List<ListOption>();
                    listOptions.Add(new ListOption("Review", review));
                    listOptions.Add(new ListOption("Perform Deletion", execute));
                    listOptions.Add(new ListOption(GlobalConstants.SelectionOptions.ReturnToMenu, () => throw new Exception(GlobalConstants.Commands.CANCEL)));

                    SelectionPrompt<ListOption> prompt = new SelectionPrompt<ListOption>();
                    prompt.Title = string.Format("A total of [red]{0}[/] users were found to be invalid.  How would you like to proceed?", invalidUsers.Count());
                    prompt.AddChoices(listOptions);

                    ListOption choice = AnsiConsole.Prompt(prompt);
                    choice.Function();
                }
            }

            AnsiConsole.MarkupLine("\n[green]{0}[/] users have been removed from [blue]{1}[/] successfully!\n\n", invalidUsers.Count(), channel.Title);
        }

        private Task<UpdatesBase> DeleteChatUser(InputPeer peer, InputUser user)
        {
            InputPeerChat inputPeerChat = peer as InputPeerChat;

            if (inputPeerChat == null)
            {
                InputPeerChannel inputPeerChannel = peer as InputPeerChannel;
                if (inputPeerChannel != null)
                {
                    return TelegramClient.Channels_EditBanned((InputChannel)inputPeerChannel, (InputPeerUser)user, new ChatBannedRights
                    {
                        flags = ChatBannedRights.Flags.view_messages,
                        until_date = DateTime.Now.AddMinutes(1),
                    });
                }
            }

            return TelegramClient.Messages_DeleteChatUser(inputPeerChat.chat_id, user, revoke_history: true);
        }

        #endregion
    }
}
