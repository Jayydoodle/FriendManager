using CustomSpectreConsole.Settings;
using CustomSpectreConsole;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using FriendManager.BAL.Discord;
using FriendManager.BAL.FriendTechTracker.BAL;
using FriendManager.Functions;
using FriendManager.Services;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using TL;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Net.Mail;
using Color = Discord.Color;

namespace FriendManager.DiscordClients
{
    public class FriendBotClient : DiscordClientBase
    {
        #region Properties

        public ulong DiscordServerId;
        public string BotToken;

        private bool _logChannelChecked;
        private SocketTextChannel _logChannel;
        public SocketTextChannel LogChannel
        {
            get
            {
                if (_logChannel == null && !_logChannelChecked && LogChannelId > 0)
                {
                    _logChannel = Client.GetChannel(LogChannelId) as SocketTextChannel;
                    _logChannelChecked = true;
                }

                return _logChannel;
            }
            set
            {
                _logChannel = value;
            }
        }

        private List<DiscordRoleConfig> _roleConfigs;
        public List<DiscordRoleConfig> RoleConfigs
        {
            get
            {
                if (_roleConfigs == null)
                    _roleConfigs = DiscordManager.GetRoleConfigurations();

                return _roleConfigs;
            }
            set
            {
                _roleConfigs = value;
            }
        }

        private SocketGuild _guild;
        private SocketGuild Guild
        {
            get
            {
                if (_guild == null && Client != null)
                    _guild = Client.GetGuild(DiscordServerId);

                return _guild;
            }
            set
            {
                _guild = value;
            }
        }

        public ulong ChannelOfTheDayId { get; set; }

        private DiscordSocketClient Client;
        private CommandService Commands;
        private IServiceProvider Services;

        private RequestOptions RequestOptions { get; set; }
        private int DefaultMessageDelay = 0;
        private int ExceededMessageDelay = 1000;
        private int MessageDelay = 0;

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

            RequestOptions = new RequestOptions();
            RequestOptions.RatelimitCallback = LogRateLimit;

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

        #region Public API

        public async Task LogMessage(string message)
        {
            if (LogChannel != null)
                await LogChannel.SendMessageAsync(message);

            await Task.CompletedTask;
        }

        #endregion

        #region Synchronization

        public async Task SynchronizeChannels(List<DiscordChannelModel> peristantChannels, List<DiscordMessageDTO> messages)
        {
            StopRoutine = false;

            if (!Initialized || StopRoutine || messages == null) return;

            messages = messages.OrderBy(x => x.GuildName)
                               .ThenBy(x => x.ChannelName)
                               .ThenBy(x => x.SentAt)
                               .ToList();

            if (peristantChannels == null)
                peristantChannels = new List<DiscordChannelModel>();

            if (!messages.Any())
                return;

            List<SocketGuildChannel> channels = channels = Guild.Channels.ToList();
            List<DiscordGuildModel> guilds = await DiscordGuildModel.GetAll(new() { x => x.GuildId == Guild.Id });
            DiscordGuildModel guildModel = guilds != null ? guilds.FirstOrDefault() : null;

            if (guildModel == null)
            {
                guildModel = new DiscordGuildModel();
                guildModel.Id = Guild.Id;
                guildModel.Name = Guild.Name;
                await guildModel.Save();
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
                if (StopRoutine)
                    return;

                SocketCategoryChannel category = channels.FirstOrDefault(x => x.Name == guildGroup.Key.ParentChannelName) as SocketCategoryChannel;

                if (category == null && !string.IsNullOrEmpty(guildGroup.Key.ParentChannelName) && guildGroup.Key.ParentChannelId.HasValue)
                {
                    RestCategoryChannel newCategory = await Guild.CreateCategoryChannelAsync(guildGroup.Key.ParentChannelName);
                    category = Guild.GetChannel(newCategory.Id) as SocketCategoryChannel;

                    if (category != null)
                        channels.Add(category);
                }

                DiscordChannelModel categoryModel = null;

                if (category != null)
                {
                    bool wasNewCategory = false;
                    categoryModel = peristantChannels.FirstOrDefault(x => x.Id == category.Id);

                    if (categoryModel == null)
                    {
                        wasNewCategory = true;
                        categoryModel = new DiscordChannelModel();
                        categoryModel.Id = category.Id;
                        categoryModel.Name = category.Name;
                        categoryModel.GuildId = Guild.Id;
                        categoryModel.CreatedDate = DateTime.UtcNow; 
                    }

                    categoryModel.ParentChannelId = null;
                    categoryModel.SourceGuildId = guildGroup.Key.GuildId;
                    categoryModel.SourceGuildName = guildGroup.Key.GuildName;
                    categoryModel.SourceChannelId = guildGroup.Key.ParentChannelId.Value;
                    categoryModel.SourceChannelName = guildGroup.Key.ParentChannelName;
                    await categoryModel.Save();

                    if (wasNewCategory)
                    {
                        peristantChannels.Add(categoryModel);
                        await EnsureChannelPermissions(new List<DiscordChannelModel> { categoryModel });
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
                    if (StopRoutine)
                        return;

                    SocketTextChannel textChannel = channels.FirstOrDefault(x => x.Name == channelGroup.Key.ChannelName) as SocketTextChannel;

                    if (textChannel == null)
                    {
                        RestTextChannel newChannel = category != null ? await Guild.CreateTextChannelAsync(channelGroup.Key.ChannelName, x => x.CategoryId = category.Id)
                                                                      : await Guild.CreateTextChannelAsync(channelGroup.Key.ChannelName);

                        textChannel = Guild.GetTextChannel(newChannel.Id);

                        if (textChannel != null)
                            channels.Add(textChannel);
                    }

                    if (category != null && textChannel.CategoryId != category.Id)
                        await textChannel.ModifyAsync(x => x.CategoryId = category.Id);

                    DiscordChannelModel channelModel = peristantChannels.FirstOrDefault(x => x.Id == textChannel.Id);
                    bool wasNewChannel = false;

                    if (channelModel == null)
                    {
                        wasNewChannel = true;
                        channelModel = new DiscordChannelModel();
                        channelModel.Id = textChannel.Id;
                        channelModel.Name = textChannel.Name;
                        channelModel.GuildId = Guild.Id;
                        channelModel.CreatedDate = DateTime.UtcNow;
                    }

                    channelModel.ParentChannelId = categoryModel != null ? categoryModel.Id : (ulong?)null;
                    channelModel.SourceGuildId = channelGroup.Key.GuildId;
                    channelModel.SourceGuildName = channelGroup.Key.GuildName;
                    channelModel.SourceChannelId = channelGroup.Key.ChannelId;
                    channelModel.SourceChannelName = channelGroup.Key.ChannelName;
                    channelModel.SourceParentChannelId = channelGroup.Key.ParentChannelId;
                    channelModel.SourceChannelName = channelGroup.Key.ChannelName;
                    await channelModel.Save();

                    if (wasNewChannel)
                    {
                        peristantChannels.Add(channelModel);
                        await EnsureChannelPermissions(new List<DiscordChannelModel> { channelModel });
                    }

                    List<DiscordMessageDTO> channelGroupMessages = channelGroup.ToList();
                    int count = channelGroupMessages.Count();
                    ulong lastSynchedMessageId = 0;

                    DiscordChannelSyncLogModel channelSyncLog = new DiscordChannelSyncLogModel();
                    channelSyncLog.ChannelId = channelModel.Id;

                    for (int i = 0; i < count; i++)
                    {
                        if (StopRoutine)
                            return;

                        DiscordMessageDTO message = channelGroupMessages[i];

                        if (!string.IsNullOrEmpty(message.MessageContent))
                        {
                            List<string> content = message.MessageContent.ChunkSplit(2000).ToList();

                            foreach (var item in content)
                            {
                                await textChannel.SendMessageAsync(text: item, flags: MessageFlags.SuppressEmbeds, options: RequestOptions);
                                await Task.Delay(MessageDelay);
                            }
                        }

                        foreach (DiscordAttachmentDTO attachment in message.Attachments)
                        {
                            if (!string.IsNullOrEmpty(attachment.DownloadUrl))
                            {
                                await textChannel.SendMessageAsync(attachment.DownloadUrl, options: RequestOptions);
                                await Task.Delay(MessageDelay);
                            }
                        }

                        if (message.Embed != null)
                        {
                            var embed = new EmbedBuilder();

                            if(!string.IsNullOrEmpty(message.Embed.AuthorName))
                                embed.WithAuthor(message.Embed.AuthorName, message.Embed.AuthorIconUrl);

                            if (message.Embed.TimeStamp.HasValue)
                                embed.WithTimestamp(message.Embed.TimeStamp.Value);

                            if (!string.IsNullOrEmpty(message.Embed.FooterText))
                                embed.WithFooter(message.Embed.FooterText, message.Embed.FooterIconUrl);
                            
                            embed.ImageUrl = message.Embed.ImageUrl;
                            embed.WithColor(Color.Magenta);

                            message.Embed.Fields.ForEach(f => 
                            {
                                string name = !string.IsNullOrEmpty(f.Name) ? f.Name : "\u200b";

                                if (!string.IsNullOrEmpty(f.Content))
                                    embed.AddField(name, f.Content);
                            });

                            if (!string.IsNullOrEmpty(embed.ImageUrl) || (embed.Fields != null && embed.Fields.Any()))
                            {
                                await textChannel.SendMessageAsync(null, false, embed.Build(), options: RequestOptions);
                                await Task.Delay(MessageDelay);
                            }
                        }

                        channelSyncLog.LastSynchedMessageId = message.MessageId;
                        channelSyncLog.SynchedDate = DateTime.UtcNow;
                        await channelSyncLog.Save();
                    }
                }
            }


            await Task.CompletedTask;
        }

        public async Task RebuildChannels(List<DiscordChannelModel> peristantChannels)
        {
            List<SocketGuildChannel> channels = Guild.Channels.Where(x => peristantChannels.Any(y => y.Id == x.Id)).ToList();

            foreach (var item in channels)
            {
                SocketTextChannel channel = item as SocketTextChannel;

                if (channel == null)
                    continue;

                await DeleteChannelMessages(channel);
            }
        }

        public async Task EnsureChannelPermissions(List<DiscordChannelModel> peristantChannels)
        {
            List<SocketGuildChannel> channels = Guild.Channels.Where(x => peristantChannels.Any(y => y.Id == x.Id)).ToList();

            foreach (var roleConfig in RoleConfigs)
            {
                SocketRole role = Guild.Roles.FirstOrDefault(x => x.Id == roleConfig.RoleId);

                if (role == null)
                    continue;

                foreach (var channel in channels)
                {
                    if (channel.Id == ChannelOfTheDayId)
                    {
                        await channel.AddPermissionOverwriteAsync(Guild.EveryoneRole, OverwritePermissions.DenyAll(channel).Modify(viewChannel: PermValue.Allow, readMessageHistory: PermValue.Allow, addReactions: PermValue.Allow));
                        continue;
                    }

                    SocketTextChannel textChan = channel as SocketTextChannel;

                    if (textChan != null && (textChan.Id == ChannelOfTheDayId || textChan.CategoryId.HasValue && textChan.CategoryId.Value == ChannelOfTheDayId))
                    {
                        await channel.AddPermissionOverwriteAsync(Guild.EveryoneRole, OverwritePermissions.DenyAll(channel).Modify(viewChannel: PermValue.Allow, readMessageHistory: PermValue.Allow, addReactions: PermValue.Allow));
                        continue;
                    }

                    await channel.AddPermissionOverwriteAsync(Guild.EveryoneRole, OverwritePermissions.DenyAll(channel));
                    await channel.AddPermissionOverwriteAsync(role, OverwritePermissions.DenyAll(channel).Modify(viewChannel: PermValue.Allow, readMessageHistory: PermValue.Allow, addReactions: PermValue.Allow));
                }
            }
        }

        #endregion

        #region User Management

        public async Task PurgeInvalidUsers()
        {
            if (!Initialized) return;

            string currentExcludedUsers = AppSettings.GetValue(Setting.DiscordExcludedUsers);
            List<string> excludedUsers = !string.IsNullOrEmpty(currentExcludedUsers) ? currentExcludedUsers.Split(',').ToList() : new List<string>();

            List<Holder> holders = FriendTechService.GetHolderDetails().Users ?? new List<Holder>();

            await Guild.DownloadUsersAsync();

            foreach(var user in Guild.Users)
                await ValidateUser(user, holders, excludedUsers);

            await Task.CompletedTask;
        }

        public List<SocketRole> GetRoles()
        {
            return Guild.Roles.ToList();
        }

        public List<SocketGuildChannel> GetChannels()
        {
            return Guild.Channels.ToList();
        }

        private async Task ValidateUser(SocketGuildUser user, List<Holder> holders = null, List<string> excludedUsers = null)
        {
            if (!Initialized) return;

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
                                   .Where(x => x.UserMapping.DiscordUserName.ToLower() == user.Username.ToLower())
                                   .FirstOrDefault();

            await AssignUserRoles(user, holder);

            await Task.CompletedTask;
        }

        private async Task HandleUserJoined(SocketGuildUser arg)
        {
            if (!Initialized) return;

            await LogMessage(string.Format("User '{0}' joined the server", arg.Username));
            await ValidateUser(arg);
        }

        private async Task AssignUserRoles(SocketGuildUser user, Holder holder)
        {
            if (!Initialized) return;

            if(holder == null)
            {
                foreach (var config in RoleConfigs)
                {
                    if (user.Roles != null && user.Roles.Any(x => x.Id == config.RoleId))
                        await user.RemoveRoleAsync(config.RoleId);
                }

                return;
            }

            int.TryParse(holder.Balance, out int numKeys);

            DiscordRoleConfig roleConfig = null;

            foreach (var config in RoleConfigs)
            {
                if (numKeys >= config.NumKeys)
                {
                    roleConfig = config;
                }
                else
                {
                    if(user.Roles != null && user.Roles.Any(x => x.Id == config.RoleId))
                        await user.RemoveRoleAsync(config.RoleId);
                }
            }

            string errorMessage = string.Format("Could not find a role to assign the User '{0}' holding {1} keys", user.Username, numKeys);
            bool throwError = false;

            if (roleConfig != null)
            {
                if (user.Roles == null || !user.Roles.Any(x => x.Id == roleConfig.RoleId))
                    await user.AddRoleAsync(roleConfig.RoleId);
            }
            else
            {
                await LogMessage(errorMessage);
                throwError = true;
            }

            await Task.CompletedTask;
        }

        #endregion

        #region Private API

        private async Task ClientLog(LogMessage arg)
        {
            Console.WriteLine(arg);
            await Task.CompletedTask;
        }

        private async Task DeleteChannelMessages(SocketTextChannel channel)
        {
            var messages = await channel.GetMessagesAsync().FlattenAsync();

            while (messages != null && messages.Any())
            {
                await channel.DeleteMessagesAsync(messages);
                messages = await channel.GetMessagesAsync().FlattenAsync();
            }
        }

        protected async Task LogRateLimit(IRateLimitInfo info)
        {
            if (info.Remaining == 0)
            {
                string currentTime = DateTime.Now.ToLongTimeString();
                LogMessage(string.Format("Rate limit exceeded {0}\nBucket: {1}\nEndpoint: {2}", currentTime, info.Bucket, info.Endpoint));

                AnsiConsole.MarkupLine("[red]Rate Limit Exceeded[/]\n{0}", currentTime);
                AnsiConsole.WriteLine($"{info.IsGlobal} {info.Limit} {info.Remaining} {info.RetryAfter} {info.Reset} {info.ResetAfter} {info.Bucket} {info.Lag} {info.Endpoint}");
            }

            if (info.Remaining.HasValue && info.Remaining.Value <= 1)
            {
                MessageDelay = ExceededMessageDelay;
            }
            else
            {
                MessageDelay = DefaultMessageDelay;
            }
        }

        #endregion
    }
}
