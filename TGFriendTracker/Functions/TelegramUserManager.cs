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
using TGFriendTracker.BAL;
using TGFriendTracker.BAL.FriendTechTracker.BAL;
using TL;
using WTelegram;

namespace TGFriendTracker.Functions
{
    public class TelegramUserManager : ManagerBase<TelegramUserManager>
    {
        #region Properties

        public WTelegram.Client TelegramClient { get; set; }
        public User CurrentUser { get; set; }
        public override string DisplayName => "Telegram User Manager";

        #endregion

        public override void Run()
        {
            if (TelegramClient == null)
                TelegramClient = new WTelegram.Client(Config);

            if (CurrentUser == null)
                CurrentUser = Task.Run<User>(async () => await TelegramClient.LoginUserIfNeeded()).Result;

            AnsiConsole.MarkupLine("You are now logged in as as [blue]" + CurrentUser + "[/] (id " + CurrentUser.id + ")\n");

            RunProgramLoop();
        }

        #region Private API

        private string Config(string what)
        {
            switch (what)
            {
                case "api_id": return Settings.GetSetting(Settings.Node.TelegramAPIKey, Validate);
                case "api_hash": return Settings.GetSetting(Settings.Node.TelegramAPIHash, Validate);
                case "phone_number": return Settings.GetSetting(Settings.Node.TelegramPhoneNumber, Validate);
                case "verification_code": Console.Write("Enter the verification code sent to your Telegram app: "); return Console.ReadLine();
                default: return null; // let WTelegramClient decide the default config
            }
        }

        private bool Validate(Settings.Node node, string value)
        {
            bool validated = false;

            switch (node)
            {
                case Settings.Node.TelegramPhoneNumber:

                    validated = value.All(x => char.IsDigit(x));

                    if (!validated)
                        Console.WriteLine("Invalid phone number.");
                    break;

                default:
                    validated = true;
                    break;
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

        private void ShowHolders()
        {
            HolderDetails details = GetHolderDetails();

            if (details.Users != null)
            {
                details.Users.ForEach(x =>
                {
                    AnsiConsole.MarkupLine("[pink1]{0}[/]", x.TwitterUsername);
                    AnsiConsole.MarkupLine("Telegram User Name: [yellow]{0}[/]", !string.IsNullOrEmpty(x.UserMapping.TelegramUserName) ? x.UserMapping.TelegramUserName : "N/A");
                    AnsiConsole.MarkupLine("Wallet Address: {0}", x.Address);
                    AnsiConsole.MarkupLine("Keys Held: {0}", x.Balance);
                    AnsiConsole.MarkupLine("Last Online: {0}", x.LastOnline);
                    AnsiConsole.WriteLine();
                });
            }
        }

        private HolderDetails GetHolderDetails()
        {
            string address = Settings.GetSetting(Settings.Node.WalletAddress);

            var request = new HttpRequestMessage(HttpMethod.Get, string.Format(HolderDetailsEndpoint, address));

            HttpResponseMessage response = WebClient.Send(request);
            HolderDetails details = new HolderDetails();

            using (var reader = new StreamReader(response.Content.ReadAsStream()))
            {
                string body = reader.ReadToEnd();
                details = JsonConvert.DeserializeObject<HolderDetails>(body);
            }

            if (details.Users == null)
                throw new Exception("You currently have no holders!");

            List<HolderUserMapping> mappings = new List<HolderUserMapping>();

            string googleSheetId = Settings.GetSetting(Settings.Node.GoogleSheetId);

            Action<ExcelWorksheet> processSheet = (sheet) =>
            {
                List<HolderUserMapping> entries = ExcelUtil.ImportData<HolderUserMapping>(sheet);

                details.Users.ForEach(user =>
                {
                    user.UserMapping = entries.FirstOrDefault(x => x.TwitterUserName != null && x.TwitterUserName.ToLower() == user.TwitterUsername.ToLower()) ?? new HolderUserMapping();
                });
            };

            GoogleSheetsUtil.ProcessSheet(googleSheetId, processSheet);

            return details;
        }

        private Channel GetChannel()
        {
            Messages_Chats chatList = Task.Run<Messages_Chats>(async () => await TelegramClient.Messages_GetAllChats()).Result;
            string chatId = Settings.GetValue(Settings.Node.TelegramChatId);

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
                    chatId = Settings.GetSetting(Settings.Node.TelegramChatId, (node, value) => chatList.chats.Any(x => x.Value.ID == long.Parse(value)));
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
            string removalInterval = Settings.GetSetting(Settings.Node.TelegramRemovalInterval, (node, value) => value.All(x => char.IsDigit(x)) && int.Parse(value) > 0);
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

            List<Holder> validUsers = GetHolderDetails().Users;
            List<string> invalidUserNames = new List<string>();

            Rule rule = new Rule(string.Format("\n[blue]Beginning User Removal Process[/]\n")).DoubleBorder<Rule>();
            AnsiConsole.Write(rule);

            List<User> invalidUsers = new List<User>();
            string currentExcludedUsers = Settings.GetValue(Settings.Node.TelegramExcludedUsers);
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

        private void ManageExcludedUsers()
        {
            string currentExcludedUsers = Settings.GetValue(Settings.Node.TelegramExcludedUsers);
            List<string> excludedUsers = !string.IsNullOrEmpty(currentExcludedUsers) ? currentExcludedUsers.Split(',').ToList() : new List<string>();

            Action viewExcludedUsers = () =>
            {
                if (excludedUsers.Any())
                    excludedUsers.OrderBy(x => x).ToList().ForEach(user => AnsiConsole.MarkupLine("[yellow]{0}[/]", user));
                else
                    AnsiConsole.WriteLine("No exluded users found.");

                AnsiConsole.WriteLine();
            };

            Action addExcludedUsers = () =>
            {
                string excluded = Utilities.GetInput("Enter a comma delimited list of users (Telegram usernames) you wish to exclude", x => !string.IsNullOrEmpty(x));

                excluded.Split(',').ToList().ForEach(x =>
                {
                    string value = x.Trim();
                    if (!excludedUsers.Contains(value))
                        excludedUsers.Add(value);
                });

                excluded = string.Join(",", excludedUsers);
                Settings.Update(Settings.Node.TelegramExcludedUsers, excluded);

                AnsiConsole.WriteLine("Excluded users updated successfully!");
                AnsiConsole.WriteLine();
            };

            Action removeExcludedUsers = () =>
            {
                MultiSelectionPrompt<string> prompt = new MultiSelectionPrompt<string>();
                prompt.Title = "Select the users you wish to remove from the excluded users list";
                prompt.InstructionsText = "[grey](Press [blue]<space>[/] to toggle an option, [green]<enter>[/] to continue)[/]\n";
                prompt.Required = false;
                prompt.PageSize = 20;
                prompt.AddChoices(excludedUsers);

                List<string> choices = AnsiConsole.Prompt(prompt);

                choices.ForEach(x =>
                {
                    if (excludedUsers.Contains(x))
                        excludedUsers.Remove(x);
                });

                string excluded = string.Join(",", excludedUsers.OrderBy(x => x).ToList());
                Settings.Update(Settings.Node.TelegramExcludedUsers, excluded);

                AnsiConsole.WriteLine("Excluded users updated successfully!");
                AnsiConsole.WriteLine();
            };

            while (true)
            {
                List<ListOption> listOptions = new List<ListOption>();
                listOptions.Add(new ListOption("View Excluded Users", viewExcludedUsers));
                listOptions.Add(new ListOption("Add Excluded Users", addExcludedUsers));
                listOptions.Add(new ListOption("Remove Excluded Users", removeExcludedUsers));
                listOptions.Add(new ListOption(GlobalConstants.SelectionOptions.ReturnToMenu, () => throw new Exception(GlobalConstants.Commands.CANCEL)));

                SelectionPrompt<ListOption> prompt = new SelectionPrompt<ListOption>();
                prompt.AddChoices(listOptions);

                ListOption choice = AnsiConsole.Prompt(prompt);
                choice.Function();
            }
        }

        #endregion
    }
}
