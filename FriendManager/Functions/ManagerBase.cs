using CustomSpectreConsole;
using CustomSpectreConsole.Settings;
using FriendManager.BAL.FriendTech;
using FriendManager.BAL.FriendTechTracker.BAL;
using FriendManager.Services;
using Newtonsoft.Json;
using OfficeOpenXml;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TL;

namespace FriendManager.Functions
{
    public abstract class ManagerBase : ConsoleFunction
    {
        #region Properties

        public HttpClient WebClient { get; set; }
        public abstract override string DisplayName { get; }

        #endregion
    }

    public abstract class ManagerBase<T> : ManagerBase
    where T : class, new()
    {
        #region Properties

        private static readonly Lazy<T> _instance = new Lazy<T>(() => new T());
        public static T Instance => _instance.Value;

        #endregion

        #region Private API

        protected void ShowHolders()
        {
            FriendTechHolderDetails details = FriendTechService.GetHolderDetails();

            if (details.Users != null)
            {
                details.Users.ForEach(x =>
                {
                    AnsiConsole.MarkupLine("[pink1]{0}[/]", x.TwitterUsername);

                    if (typeof(T) == typeof(DiscordManager))
                        AnsiConsole.MarkupLine("Discord User Name: [yellow]{0}[/]", !string.IsNullOrEmpty(x.UserMapping.DiscordUserName) ? x.UserMapping.DiscordUserName : "N/A");
                    else if (typeof(T) == typeof(TelegramManager))
                        AnsiConsole.MarkupLine("Telegram User Name: [yellow]{0}[/]", !string.IsNullOrEmpty(x.UserMapping.TelegramUserName) ? x.UserMapping.TelegramUserName : "N/A");

                    AnsiConsole.MarkupLine("Wallet Address: {0}", x.Address);
                    AnsiConsole.MarkupLine("Keys Held: {0}", x.Balance);
                    AnsiConsole.MarkupLine("Last Online: {0}", x.LastOnline);
                    AnsiConsole.WriteLine();
                });
            }
        }

        protected void ManageExcludedUsers()
        {
            Setting setting = typeof(T) == typeof(DiscordManager) ? Setting.DiscordExcludedUsers : Setting.TelegramExcludedUsers;

            string currentExcludedUsers = XMLSettings.GetValue(setting);
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
                string excluded = CustomSpectreConsole.Utilities.GetInput(setting.GetPrompt(), x => !string.IsNullOrEmpty(x));

                excluded.Split(',').ToList().ForEach(x =>
                {
                    string value = x.Trim();
                    if (!excludedUsers.Contains(value))
                        excludedUsers.Add(value);
                });

                excluded = string.Join(",", excludedUsers);
                XMLSettings.Update(setting, excluded);

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
                XMLSettings.Update(setting, excluded);

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
