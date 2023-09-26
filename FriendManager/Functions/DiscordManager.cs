using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Discord.Rest;
using FriendManager.DiscordClients;
using FriendManager.BAL.Discord;
using TL;
using CustomSpectreConsole;
using CustomSpectreConsole.Settings;
using Spectre.Console;
using FriendManager.DAL.Discord;
using Microsoft.EntityFrameworkCore;

namespace FriendManager.Functions
{
    public class DiscordManager : ManagerBase<DiscordManager>
    {
        #region Properties

        public override string DisplayName => "Discord Manager";

        private FriendBotClient Client;
        private DataExtractionClient ExtractionClient;
        private bool RunningSyncRoutine;
        private bool ScheduledSyncEnabled;
        private bool RunningPurgeRoutine;
        private bool ScheduledPurgeEnabled;
        private bool RunningChannelPermissionsRoutine;

        #endregion

        #region Public API

        public async override void Run()
        {
            await Configure();
            await RunProgramLoop();
        }

        #endregion

        #region Private API: User Inferface

        protected override List<ListOption> GetListOptions()
        {
            List<ListOption> listOptions = new List<ListOption>();

            listOptions.Add(new ListOption("Show Holders", ShowHolders));
            listOptions.Add(new ListOption("Purge Invalid Users", PurgeInvalidUsers));
            listOptions.Add(new ListOption("Run Scheduled User Purge", RunScheduledPurge));
            listOptions.Add(new ListOption("Synchronize Data", SynchronizeData));
            listOptions.Add(new ListOption("Run Scheduled Synchronization", RunScheduledSync));
            listOptions.Add(new ListOption("Stop Sync Routines", StopSyncRoutines));
            listOptions.Add(new ListOption("Edit Channel Of The Day", EditChannelOfTheDay));
            listOptions.Add(new ListOption("Edit Configuration", EditConfiguration));
            listOptions.Add(new ListOption("Ensure Channel Permissions", EnsureChannelPermissions));
            listOptions.Add(new ListOption("Manage Excluded Channels", EditChannelConfig));
            listOptions.Add(new ListOption("Manage Excluded Users", ManageExcludedUsers));
            listOptions.Add(new ListOption("Manage Target Servers", EditTargetServerConfig));
            listOptions.Add(new ListOption("Manage Roles", EditRoleConfig));
            listOptions.Add(new ListOption("Rebuild Channels", RebuildChannels));
            listOptions.Add(new ListOption("Delete Channels", DeleteChannels));
            listOptions.AddRange(base.GetListOptions());
            listOptions.Add(GetHelpOption());

            return listOptions;
        }

        #endregion

        #region Private API: Configuration

        private async Task Configure()
        {
            XMLSettings.GetSetting(Setting.WalletAddress);
            XMLSettings.GetSetting(Setting.GoogleSheetId);
            XMLSettings.GetSetting(ConnectionStrings.Discord);

            if (Client == null)
            {
                Client = new FriendBotClient();
                Client.DiscordServerId = ulong.Parse(XMLSettings.GetSetting(Setting.DiscordServerId, ValidateSetting));
                Client.BotToken = XMLSettings.GetSetting(Setting.DiscordBotToken, null, new PromptSettings() { IsSecret = true });
            }

            if (ExtractionClient == null)
            {
                ExtractionClient = new DataExtractionClient();
                ExtractionClient.DiscordUserToken = XMLSettings.GetSetting(Setting.DiscordUserToken, null, new PromptSettings() { IsSecret = true });
                ExtractionClient.BotClient = Client;
            }

            if (!Client.Initialized)
            {
                try { await Client.Initialize(); }
                catch (Exception e) { e.LogException(); }
            }

            if (!ExtractionClient.Initialized)
            {
                try { await ExtractionClient.Initialize(); }
                catch (Exception e) { e.LogException(); }
            }

            while (!Client.Initialized || !ExtractionClient.Initialized)
            { // wait
            }

            Thread.Sleep(1000);

            using (var context = new DiscordContext())
            {
                AnsiConsole.MarkupLine("Connected to database: [blue]{0}[/]\n", context.Database.GetDbConnection().Database);
            }

            

            string extractionServerIds = XMLSettings.GetValue(Setting.DiscordServerExtractionSettings);

            if (string.IsNullOrEmpty(extractionServerIds) || !ValidateSetting(Setting.DiscordServerExtractionSettings, extractionServerIds))
                EditTargetServerConfig();

            ulong.TryParse(XMLSettings.GetSetting(Setting.DiscordLogChannelId, ValidateSetting), out ulong logChannelId);
            Client.LogChannelId = logChannelId;

            await Task.CompletedTask;
        }

        private void EditConfiguration()
        {
            bool proceed = Utilities.GetConfirmation("Are you sure you want to edit the main configuration?  Existing configuration settings will be lost, and all bot execution will be stopped until configuration is re-applied.");

            if (!proceed)
                return;

            StopSyncRoutines();

            Client.Shutdown();
            ExtractionClient.Shutdown();

            Client = null;
            ExtractionClient = null;

            XMLSettings.Update(Setting.DiscordServerExtractionSettings, null);
            XMLSettings.Update(Setting.DiscordUserToken, null);
            XMLSettings.Update(Setting.DiscordBotToken, null);
            XMLSettings.Update(Setting.DiscordServerId, null);
            XMLSettings.Update(Setting.DiscordLogChannelId, null);

            Configure();
        }

        private void EditTargetServerConfig()
        {
            List<DiscordGuildExtractionConfig> extractionSettings = DiscordManager.GetExtractionSettings();
            List<DiscordGuildDTO> userGuilds = ExtractionClient.GetAvailableGuilds();

            MultiSelectionPrompt<DiscordGuildDTO> prompt = new MultiSelectionPrompt<DiscordGuildDTO>();
            prompt.Title = Setting.DiscordServerExtractionSettings.GetPrompt();
            prompt.InstructionsText = "[grey](Press [blue]<space>[/] to toggle an option, [green]<enter>[/] to continue)[/]\n";
            prompt.Required = false;
            prompt.PageSize = 20;
            prompt.UseConverter(x => string.Format("{0} ({1})", x.GuildName, x.GuildId));

            userGuilds.ForEach(x =>
            {
                IMultiSelectionItem<DiscordGuildDTO> item = prompt.AddChoice(x);

                if (extractionSettings.Any(y => y.GuildId == x.GuildId))
                    item.Select();
            });

            List<DiscordGuildDTO> choices = AnsiConsole.Prompt(prompt);

            choices.ForEach(x =>
            {
                if (!extractionSettings.Any(y => y.GuildId == x.GuildId))
                    extractionSettings.Add(new DiscordGuildExtractionConfig() { Name = x.GuildName, GuildId = x.GuildId });
            });

            extractionSettings = extractionSettings.Where(x => choices.Any(y => y.GuildId == x.GuildId)).ToList();

            XMLSettings.Update(Setting.DiscordServerExtractionSettings, JsonConvert.SerializeObject(extractionSettings));

            AnsiConsole.WriteLine("Target servers updated successfully!");
            AnsiConsole.WriteLine();
        }

        private void EditChannelConfig()
        {
            List<DiscordGuildExtractionConfig> extractionSettings = DiscordManager.GetExtractionSettings();

            SelectionPrompt<DiscordGuildExtractionConfig> extractionPrompt = new SelectionPrompt<DiscordGuildExtractionConfig>();
            extractionPrompt.Title = "Select the server whose channels you want to manage";
            extractionPrompt.PageSize = 20;
            extractionPrompt.UseConverter(x => string.Format("{0} ({1})", x.Name, x.GuildId));
            extractionPrompt.AddChoices(extractionSettings);

            DiscordGuildExtractionConfig setting = AnsiConsole.Prompt(extractionPrompt);   
            extractionSettings.Remove(setting);

            List<DiscordChannelDTO> availableChannels = ExtractionClient.GetAvailableChannels()
                                                        .Where(x => x.GuildId == setting.GuildId)
                                                        .ToList();

            MultiSelectionPrompt<DiscordChannelDTO> prompt = new MultiSelectionPrompt<DiscordChannelDTO>();
            prompt.Title = "Select the channels you want to exclude from data extraction";
            prompt.InstructionsText = "[grey](Press [blue]<space>[/] to toggle an option, [green]<enter>[/] to continue)[/]\n";
            prompt.Required = false;
            prompt.PageSize = 20;
            prompt.UseConverter(x => string.Format("{0} ({1})", x.ChannelName, x.ChannelId));


            availableChannels.Where(x => !x.ParentChannelId.HasValue)
            .ToList()
            .ForEach(x =>
            {
                List<DiscordChannelDTO> children = availableChannels.Where(y => y.ParentChannelId == x.ChannelId).ToList();
                prompt.AddChoiceGroup(x, children);

                if (setting.ExcludedChannelIds != null) 
                {
                    if(setting.ExcludedChannelIds.Any(y => y == x.ChannelId))
                        prompt.Select(x);

                    children.ForEach(child =>
                    {
                        if (setting.ExcludedChannelIds.Any(y => y == child.ChannelId))
                            prompt.Select(child);
                    });
                } 
            });

            List<DiscordChannelDTO> choices = AnsiConsole.Prompt(prompt);
            List<ulong> excludedChannelIds = choices.Select(y => y.ChannelId).ToList();

            availableChannels.Where(x => x.ParentChannelId.HasValue)
            .GroupBy(x => x.ParentChannelId.Value)
            .ToList()
            .ForEach(group =>
            {
                if (group.All(x => excludedChannelIds.Contains(x.ChannelId)))
                    excludedChannelIds.Add(group.Key);
            });

            setting.ExcludedChannelIds = excludedChannelIds;
            extractionSettings.Add(setting);

            XMLSettings.Update(Setting.DiscordServerExtractionSettings, JsonConvert.SerializeObject(extractionSettings));

            AnsiConsole.WriteLine("Target server channels updated successfully!");
            AnsiConsole.WriteLine();
        }

        private void EditRoleConfig()
        {
            List<DiscordRoleConfig> currentConfigs = GetRoleConfigurations();

            List<SocketRole> roles = Client.GetRoles();

            SelectionPrompt<SocketRole> prompt = new SelectionPrompt<SocketRole>();
            prompt.Title = Setting.DiscordRoleConfiguration.GetPrompt();
            prompt.PageSize = 20;
            prompt.UseConverter(x => string.Format("{0} ({1})", x.Name, x.Id));
            prompt.AddChoices(roles);

            SocketRole choice = AnsiConsole.Prompt(prompt);

            string keys = Utilities.GetInput("How many keys should a user hold for this role?", x => x.All(y => char.IsDigit(y)));
            int.TryParse(keys, out int numKeys);

            DiscordRoleConfig config = currentConfigs.FirstOrDefault(x => x.RoleId == choice.Id);

            if (config == null)
            {
                config = new DiscordRoleConfig();
                config.RoleId = choice.Id;
                config.RoleName = choice.Name;
                currentConfigs.Add(config);
            }

            config.NumKeys = numKeys;
            XMLSettings.Update(Setting.DiscordRoleConfiguration, JsonConvert.SerializeObject(currentConfigs));

            AnsiConsole.MarkupLine("[green]The role '{0}' has been updated successfully![/]", choice.Name);
        }

        private bool ValidateSetting(Setting node, string value)
        {
            bool validated = true;

            if (node.Name == Setting.DiscordServerId.Name || node.Name == Setting.DiscordLogChannelId.Name)
            {
                validated = value.All(x => char.IsDigit(x));

                if (!validated)
                    Console.WriteLine("Invalid server Id.");
            }
            else if (node.Name == Setting.DiscordServerExtractionSettings.Name)
            {
                try
                {
                    JsonConvert.DeserializeObject<List<DiscordGuildExtractionConfig>>(value);
                }
                catch (Exception)
                {
                    validated = false;
                    Console.WriteLine("The current list of target server ids is invalid, please re-configure.");
                }
            }
            else if (node.Name == Setting.DiscordSyncInterval.Name || node.Name == Setting.DiscordPurgeInterval.Name)
            {
                int minInterval = node.Name == Setting.DiscordSyncInterval.Name ? Setting.MinSyncInterval : Setting.MinPurgeInterval;

                validated = value.All(x => char.IsDigit(x)) && int.Parse(value) >= minInterval;

                if (!validated)
                    Console.WriteLine("Invalid interval.");
            }

            return validated;
        }

        #endregion

        #region Private API: User Management

        private async void PurgeInvalidUsers()
        {
            await RunPurgeRoutine();
        }

        private async void RunScheduledPurge()
        {
            ScheduledPurgeEnabled = true;

            while (ScheduledPurgeEnabled && Client != null && ExtractionClient != null && Client.Initialized && ExtractionClient.Initialized)
            {
                int.TryParse(XMLSettings.GetSetting(Setting.DiscordPurgeInterval, ValidateSetting), out int interval);

                if (interval == 0 || interval < Setting.MinPurgeInterval)
                {
                    AnsiConsole.WriteLine("Invalid purge interval found in configuration settings, using default value of {0} minutes", Setting.DefaultPurgeInterval);
                    interval = Setting.DefaultPurgeInterval;
                }

                interval = (int)TimeSpan.FromMinutes(interval).TotalMilliseconds;

                await RunPurgeRoutine();
                await Task.Delay(interval);
            }
        }

        private async Task RunPurgeRoutine()
        {
            try
            {
                RunningPurgeRoutine = true;

                AnsiConsole.MarkupLine("\n[yellow]Invalid User Purge In Progress - {0}[/]", DateTime.Now.ToShortTimeString());

                await Client.PurgeInvalidUsers();

                AnsiConsole.MarkupLine("[green]Invalid User Purge Complete - {0}[/]\n", DateTime.Now.ToShortTimeString());

                RunningPurgeRoutine = false;

            }
            catch (Exception e)
            {
                AnsiConsole.MarkupLine("[red]Invalid User Purge Failed - {0}[/]\n", DateTime.Now.ToShortTimeString());
                e.LogException();
                RunningPurgeRoutine = false;
            }

            await Task.CompletedTask;
        }

        private async void EnsureChannelPermissions()
        {
            if (RunningSyncRoutine || RunningPurgeRoutine)
            {
                AnsiConsole.MarkupLine("\n[orange1]Ensuring Channel Permissions - Waiting for other routines to complete... {0}[/]", DateTime.Now.ToShortTimeString());

                while (RunningSyncRoutine || RunningPurgeRoutine)
                    await Task.Delay(2000);
            }

            try
            {
                AnsiConsole.MarkupLine("\n[yellow]Ensuring Channel Permissions - {0}[/]", DateTime.Now.ToShortTimeString());

                RunningChannelPermissionsRoutine = true;
                List<DiscordChannelModel> channels = await GetChannels();

                await Client.EnsureChannelPermissions(channels);

                AnsiConsole.MarkupLine("[green]Ensuring Channel Permissions Complete - {0}[/]\n", DateTime.Now.ToShortTimeString());

                RunningChannelPermissionsRoutine = false;

            }
            catch (Exception e)
            {
                AnsiConsole.MarkupLine("[red]Ensuring Channel Permissions Failed - {0}[/]\n", DateTime.Now.ToShortTimeString());
                e.LogException();
                RunningChannelPermissionsRoutine = false;
            }

            await Task.CompletedTask;
        }

        private void EditChannelOfTheDay()
        {
            if (RunningSyncRoutine || RunningPurgeRoutine)
            {
                AnsiConsole.MarkupLine("\nWaiting for other routines to complete... {0}[/]", DateTime.Now.ToShortTimeString());

                while (RunningSyncRoutine || RunningPurgeRoutine)
                    Thread.Sleep(2000);
            }

            List<DiscordChannelModel> channels = Task.Run(() => GetChannels()).Result;

            if (channels == null || !channels.Any())
            {
                AnsiConsole.MarkupLine("[red]No channels found![/]");
                return;
            }

            SelectionPrompt<DiscordChannelModel> prompt = new SelectionPrompt<DiscordChannelModel>();
            prompt.Title = "Select the channel you want to add as channel of the day";
            prompt.PageSize = 20;
            prompt.UseConverter(x => string.Format("{0} ({1})", x.Name, x.Id));
            prompt.AddChoices(channels);
            prompt.AddChoice(new DiscordChannelModel { Name = "None", Id = 0 });

            DiscordChannelModel channel = AnsiConsole.Prompt(prompt);

            Client.ChannelOfTheDayId = channel.Id;

            EnsureChannelPermissions();
        }

        #endregion

        #region Private API: Synchronization

        private async void SynchronizeData()
        {
            await RunSyncRoutine();
        }

        private async void RunScheduledSync()
        {
            ScheduledSyncEnabled = true;

            while (ScheduledSyncEnabled && Client != null && ExtractionClient != null && Client.Initialized && ExtractionClient.Initialized)
            {
                int.TryParse(XMLSettings.GetSetting(Setting.DiscordSyncInterval, ValidateSetting), out int syncInterval);

                if (syncInterval == 0 || syncInterval < Setting.MinSyncInterval)
                {
                    AnsiConsole.WriteLine("Invalid sync interval found in configuration settings, using default value of {0} minutes", Setting.DefaultSyncInterval);
                    syncInterval = Setting.DefaultSyncInterval;
                }

                syncInterval = (int)TimeSpan.FromMinutes(syncInterval).TotalMilliseconds;

                await RunSyncRoutine();
                await Task.Delay(syncInterval);
            }
        }

        private async Task RunSyncRoutine()
        {
            try
            {
                RunningSyncRoutine = true;

                AnsiConsole.MarkupLine("\n[yellow]Synchronization In Progress - {0}[/]", DateTime.Now.ToShortTimeString());

                List<DiscordChannelModel> channels = await GetChannels();

                List<DiscordMessageDTO> messages = await ExtractionClient.DownloadGuildData(channels);
                await Client.SynchronizeChannels(channels, messages);

                RunningSyncRoutine = false;

            }
            catch (Exception e)
            {
                e.LogException();

                List<DiscordChannelSyncLogModel> logs = await DiscordChannelSyncLogModel.GetAll(null, new() { x => x.Channel });
                DiscordChannelSyncLogModel latestLog = logs.OrderByDescending(x => x.SynchedDate).FirstOrDefault();

                if(latestLog != null)
                {
                    AnsiConsole.MarkupLine("[orange1]The last synchronized message was[/]:\n\nChannel: [blue]{0}[/]\nMessageId: [blue]{1}[/]\nSentAt: [blue]{2}[/]\n\n", latestLog.Channel.Name, latestLog.LastSynchedMessageId, latestLog.SynchedDate.Value.ToLongTimeString());
                    Client.LogMessage(string.Format("An error occured while running the sync routine.\n\nThe last synchronized message was:\n\nChannel: {0}\nMessageId: {1}\nSentAt: {2}\n", latestLog.Channel.Name, latestLog.LastSynchedMessageId, latestLog.SynchedDate.Value.ToLongTimeString()));
                }

                AnsiConsole.MarkupLine("[red]Synchronization Failed - {0}[/]\n", DateTime.Now.ToShortTimeString());
                RunningSyncRoutine = false;
            }

            await Task.CompletedTask;
        }

        private async void RebuildChannels()
        {
            StopSyncRoutines();

            List<DiscordChannelModel> availableChannels = GetChannels().Result;

            MultiSelectionPrompt<DiscordChannelModel> prompt = new MultiSelectionPrompt<DiscordChannelModel>();
            prompt.Title = "Select the channels you want to rebuild";
            prompt.InstructionsText = "[grey](Press [blue]<space>[/] to toggle an option, [green]<enter>[/] to continue)[/]\n";
            prompt.Required = false;
            prompt.PageSize = 20;
            prompt.UseConverter(x => string.Format("{0} ({1})", x.Name, x.Id));

            prompt.AddChoices(availableChannels);

            List<DiscordChannelModel> choices = AnsiConsole.Prompt(prompt);
            List<ulong> channelIds = choices.Select(x => x.Id).ToList();

            List<DiscordChannelSyncLogModel> logs = DiscordChannelSyncLogModel.GetAll(new() { x => channelIds.Contains(x.ChannelId) }).Result;

            AnsiConsole.MarkupLine("\n[yellow]Channel Rebuilds In Progress - {0}[/]", DateTime.Now.ToShortTimeString());

            logs.ForEach(x => x.Delete());
            Client.RebuildChannels(choices);

            AnsiConsole.MarkupLine("\n[green]Channel Rebuilds Complete - {0}[/]", DateTime.Now.ToShortTimeString());
        }

        private async void DeleteChannels()
        {
            StopSyncRoutines();

            List<DiscordChannelModel> availableChannels = GetChannels().Result;

            MultiSelectionPrompt<DiscordChannelModel> prompt = new MultiSelectionPrompt<DiscordChannelModel>();
            prompt.Title = "Select the channels you want to delete";
            prompt.InstructionsText = "[grey](Press [blue]<space>[/] to toggle an option, [green]<enter>[/] to continue)[/]\n";
            prompt.Required = false;
            prompt.PageSize = 20;
            prompt.UseConverter(x => string.Format("{0} ({1})", x.Name, x.Id));

            prompt.AddChoices(availableChannels);

            List<DiscordChannelModel> choices = AnsiConsole.Prompt(prompt);
            List<ulong> channelIds = choices.Select(x => x.Id).ToList();

            List<DiscordChannelSyncLogModel> logs = DiscordChannelSyncLogModel.GetAll(new() { x => channelIds.Contains(x.ChannelId) }).Result;

            AnsiConsole.MarkupLine("\n[yellow]Delete Channels In Progress - {0}[/]", DateTime.Now.ToShortTimeString());

            logs.ForEach(x => x.Delete());
            Client.DeleteChannels(choices);
            choices.ForEach(x => x.Delete());

            AnsiConsole.MarkupLine("\n[green]Delete Channels Complete - {0}[/]", DateTime.Now.ToShortTimeString());
        }

        #endregion

        #region Private API

        public static List<DiscordGuildExtractionConfig> GetExtractionSettings()
        {
            string val = XMLSettings.GetValue(Setting.DiscordServerExtractionSettings);
            List<DiscordGuildExtractionConfig> settings = !string.IsNullOrEmpty(val) ? JsonConvert.DeserializeObject<List<DiscordGuildExtractionConfig>>(val) : new List<DiscordGuildExtractionConfig>();
            return settings;
        }

        public static List<DiscordRoleConfig> GetRoleConfigurations()
        {

            string currentConfig = XMLSettings.GetValue(Setting.DiscordRoleConfiguration);
            List<DiscordRoleConfig> currentConfigs = new List<DiscordRoleConfig>();

            if (!string.IsNullOrEmpty(currentConfig))
                currentConfigs = JsonConvert.DeserializeObject<List<DiscordRoleConfig>>(currentConfig);

            return currentConfigs;
        }

        public static async Task<List<DiscordChannelModel>> GetChannels()
        {
            List<ulong> targetServerIds = DiscordManager.GetExtractionSettings().Select(x => x.GuildId).ToList();

            List<DiscordChannelModel> channels = await DiscordChannelModel.GetAll(new() { x => targetServerIds.Contains(x.SourceGuildId) }, new() { x => x.Guild, x => x.Logs });

            return channels;
        }

        private void StopSyncRoutines()
        {
            Status status = AnsiConsole.Status()
                            .AutoRefresh(false)
                            .Spinner(Spinner.Known.Star)
                            .SpinnerStyle(Style.Parse("green bold"));

            if (RunningSyncRoutine || RunningPurgeRoutine)
            {
                AnsiConsole.Status()
                .Start("A scheduled routine is currently running, waiting for execution to complete...", ctx =>
                {
                    // Simulate some work
                    while (RunningSyncRoutine || RunningPurgeRoutine)
                    {
                        Client.StopRoutine = true;
                        ExtractionClient.StopRoutine = true;
                        Thread.Sleep(1000);
                    }
                });
            }

            AnsiConsole.MarkupLine("[green]All synchronization routines have been stopped successfully[/]");

            ScheduledSyncEnabled = false;
            ScheduledPurgeEnabled = false;
        }


        #endregion
    }
}
