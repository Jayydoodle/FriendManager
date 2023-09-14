using CustomSpectreConsole.Settings;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using FriendManager.BAL.Discord;
using FriendManager.BAL.FriendTechTracker.BAL;
using FriendManager.Services;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TL;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FriendManager.Discord
{
    public class FriendBotClient : DiscordClientBase
    {
        #region Properties

        public ulong DiscordServerId;
        public string BotToken;

        private bool _channelChecked;
        private SocketTextChannel _logChannel;
        public SocketTextChannel LogChannel
        {
            get
            {
                if (_logChannel == null && !_channelChecked && LogChannelId > 0)
                {
                    _logChannel = Client.GetChannel(LogChannelId) as SocketTextChannel;
                    _channelChecked = true;
                }

                return _logChannel;
            }
            set
            {
                _logChannel = value;
            }
        }

        private DiscordSocketClient Client;
        private CommandService Commands;
        private IServiceProvider Services;

        private SocketGuild Guild;

        #endregion

        #region Configuration

        public async override Task Initialize()
        {
            if (string.IsNullOrEmpty(BotToken))
                throw new Exception(string.Format("Invalid setting '{0}', please double check your App.config", Setting.DiscordBotToken.Name));

            if (DiscordServerId == 0)
                throw new Exception(string.Format("Invalid setting '{0}', please double check your App.config", Setting.DiscordServerId.Name));

            Client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.All
            });

            Commands = new CommandService();

            Services = new ServiceCollection()
                .AddSingleton(Client)
                .AddSingleton(Commands)
                .BuildServiceProvider();

            Client.Log += ClientLog;

            await RegisterCommandsAsync();

            await Client.LoginAsync(TokenType.Bot, BotToken);

            await Client.StartAsync();

            //now the bot is online
            await base.Initialize();
        }

        private async Task RegisterCommandsAsync()
        {
            Client.UserJoined += HandleUserJoined;

            await Commands.AddModulesAsync(Assembly.GetEntryAssembly(), Services);
        }

        public async override Task Shutdown()
        {
            await base.Shutdown();
            await Client.LogoutAsync();
            await Client.StopAsync();
        }

        #endregion

        #region Synchronization

        public async Task SynchronizeChannels(List<DiscordChannelModel> peristantChannels, List<DiscordMessageDTO> messages)
        {
            if (!Initialized || messages == null) return;

            messages = messages.OrderBy(x => x.GuildName)
                               .ThenBy(x => x.ChannelName)
                               .ThenBy(x => x.SentAt)
                               .ToList();

            if (peristantChannels == null)
                peristantChannels = new List<DiscordChannelModel>();

            SocketGuild targetGuild = null;
            List<SocketGuildChannel> channels = null;
            DiscordGuildModel guildModel = null;

            if (messages.Any())
            {
                targetGuild = Client.GetGuild(DiscordServerId);
                channels = targetGuild.Channels.ToList();

                guildModel = new DiscordGuildModel(targetGuild.Id);
                await guildModel.Loading;

                if (guildModel.LoadComplete)
                {
                    guildModel.Id = targetGuild.Id;
                    guildModel.Name = targetGuild.Name;
                    await guildModel.Save();
                }
            }

            var groups = messages.GroupBy(x => new
            {
                x.GuildId,
                x.GuildName,
                x.ParentChannelId,
                x.ParentChannelName
            });

            foreach (var guildGroup in groups)
            {
                SocketCategoryChannel category = channels.FirstOrDefault(x => x.Name == guildGroup.Key.ParentChannelName) as SocketCategoryChannel;

                if (category == null && !string.IsNullOrEmpty(guildGroup.Key.ParentChannelName) && guildGroup.Key.ParentChannelId.HasValue)
                {
                    RestCategoryChannel newCategory = await targetGuild.CreateCategoryChannelAsync(guildGroup.Key.ParentChannelName);
                    category = targetGuild.GetChannel(newCategory.Id) as SocketCategoryChannel;

                    if (category != null)
                        channels.Add(category);
                }

                DiscordChannelModel categoryModel = null;

                if (category != null)
                {
                    categoryModel = peristantChannels.FirstOrDefault(x => x.Id == category.Id);

                    if (categoryModel == null)
                    {
                        categoryModel = new DiscordChannelModel();
                        categoryModel.Id = category.Id;
                        categoryModel.Name = category.Name;
                        categoryModel.GuildId = targetGuild.Id;
                        categoryModel.ParentChannelId = category != null ? category.Id : (ulong?)null;
                        categoryModel.SourceGuildId = guildGroup.Key.GuildId;
                        categoryModel.SourceGuildName = guildGroup.Key.GuildName;
                        categoryModel.SourceChannelId = guildGroup.Key.ParentChannelId.Value;
                        categoryModel.SourceChannelName = guildGroup.Key.ParentChannelName;
                        await categoryModel.Save();
                    }
                }

                var channelGroups = guildGroup.GroupBy(x => new
                {
                    x.ChannelId, 
                    x.ChannelName, 
                    x.GuildId, 
                    x.GuildName, 
                    x.ParentChannelId, 
                    x.ParentChannelName
                });

                foreach(var channelGroup in channelGroups) 
                { 
                    SocketTextChannel textChannel = channels.FirstOrDefault(x => x.Name == channelGroup.Key.ChannelName) as SocketTextChannel;

                    if (textChannel == null)
                    {
                        RestTextChannel newChannel = category != null ? await targetGuild.CreateTextChannelAsync(channelGroup.Key.ChannelName, x => x.CategoryId = category.Id)
                                                                      : await targetGuild.CreateTextChannelAsync(channelGroup.Key.ChannelName);

                        textChannel = targetGuild.GetTextChannel(newChannel.Id);

                        if (textChannel != null)
                            channels.Add(textChannel);
                    }

                    DiscordChannelModel channelModel = peristantChannels.FirstOrDefault(x => x.Id == textChannel.Id);

                    if (channelModel == null)
                    {
                        channelModel = new DiscordChannelModel();
                        channelModel.Id = textChannel.Id;
                        channelModel.Name = textChannel.Name;
                        channelModel.GuildId = targetGuild.Id;
                        channelModel.ParentChannelId = categoryModel != null ? categoryModel.Id : (ulong?)null;
                        channelModel.SourceGuildId = channelGroup.Key.GuildId;
                        channelModel.SourceGuildName = channelGroup.Key.GuildName;
                        channelModel.SourceChannelId = channelGroup.Key.ChannelId;
                        channelModel.SourceChannelName = channelGroup.Key.ChannelName;
                        channelModel.SourceParentChannelId = channelGroup.Key.ParentChannelId;
                        channelModel.SourceChannelName = channelGroup.Key.ChannelName;
                        await channelModel.Save();
                    }

                    List<DiscordMessageDTO> channelGroupMessages = channelGroup.ToList();
                    int count = channelGroupMessages.Count();
                    ulong lastSynchedMessageId = 0;

                    DiscordChannelSyncLogModel channelSyncLog = new DiscordChannelSyncLogModel();
                    channelSyncLog.ChannelId = channelModel.Id;

                    for (int i = 0; i < count; i++)
                    {
                        DiscordMessageDTO message = channelGroupMessages[i];

                        if (message.Attachments.Any())
                        {
                            if (!string.IsNullOrEmpty(message.MessageContent))
                                await textChannel.SendMessageAsync(message.MessageContent);

                            foreach (DiscordAttachmentDTO attachment in message.Attachments)
                                if(!string.IsNullOrEmpty(attachment.DownloadUrl))
                                    await textChannel.SendMessageAsync(attachment.DownloadUrl);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(message.MessageContent))
                                await textChannel.SendMessageAsync(message.MessageContent);
                        }

                        channelSyncLog.LastSynchedMessageId = message.MessageId;
                        channelSyncLog.SynchedDate = DateTime.UtcNow;
                        await channelSyncLog.Save();
                    }
                }
            }


            await Task.CompletedTask;
        }

        #endregion

        #region User Management

        public async Task PurgeInvalidUsers()
        {
            if (!Initialized) return;

            if (Guild == null)
                Guild = Client.GetGuild(DiscordServerId);

            string currentExcludedUsers = AppSettings.GetValue(Setting.DiscordExcludedUsers);
            List<string> excludedUsers = !string.IsNullOrEmpty(currentExcludedUsers) ? currentExcludedUsers.Split(',').ToList() : new List<string>();

            List<Holder> holders = FriendTechService.GetHolderDetails().Users ?? new List<Holder>();

            await Guild.DownloadUsersAsync();

            foreach(var user in Guild.Users)
                await ValidateUser(user, holders, excludedUsers);

            await Task.CompletedTask;
        }

        private async Task ValidateUser(SocketGuildUser user, List<Holder> holders = null, List<string> excludedUsers = null)
        {
            if (!Initialized) return;

            if (Guild == null)
                Guild = Client.GetGuild(DiscordServerId);

            if (user.IsBot || user.Id == Guild.OwnerId)
                return;

            if (excludedUsers == null)
            {
                string currentExcludedUsers = AppSettings.GetValue(Setting.DiscordExcludedUsers);
                excludedUsers = !string.IsNullOrEmpty(currentExcludedUsers) ? currentExcludedUsers.Split(',').ToList() : new List<string>();
            }

            if (holders == null)
                holders = FriendTechService.GetHolderDetails().Users ?? new List<Holder>();


            if (excludedUsers.Contains(user.Username))
                return;

            Holder holder = holders.Where(x => x.UserMapping != null)
                                   .Where(x => !string.IsNullOrEmpty(x.UserMapping.DiscordUserName))
                                   .Where(x => x.UserMapping.DiscordUserName.ToLower() == user.Username)
                                   .FirstOrDefault();

            if (holder == null)
            {
                await user.KickAsync("User is no longer holding required friend.tech shares");

                if (LogChannel != null)
                    await LogChannel.SendMessageAsync(string.Format("User '{0}' removed from the server for not holding any shares", user.Username));
            }

            await Task.CompletedTask;
        }

        private async Task HandleUserJoined(SocketGuildUser arg)
        {
            if (!Initialized) return;

            if (LogChannel != null)
                await LogChannel.SendMessageAsync(string.Format("User '{0}' joined the server", arg.Username));

            await ValidateUser(arg);
        }

        #endregion

        #region Private API

        private async Task ClientLog(LogMessage arg)
        {
            Console.WriteLine(arg);
            await Task.CompletedTask;
        }

        #endregion
    }
}
