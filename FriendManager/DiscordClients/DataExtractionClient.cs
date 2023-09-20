extern alias Anarchy;
using Anarchy::Discord;
using Anarchy::Discord.Gateway;
using CustomSpectreConsole;
using FriendManager.BAL.Discord;
using FriendManager.Functions;
using Spectre.Console;

namespace FriendManager.DiscordClients
{
    public class DataExtractionClient : DiscordClientBase
    {
        #region Properties

        public FriendBotClient BotClient { get; set; }
        public DiscordSocketClient Client { get; set; }
        public string DiscordUserToken { get; set; }

        #endregion

        #region Public API

        public async override Task Initialize()
        {
            if (Client != null)
                return;

            if (string.IsNullOrEmpty(DiscordUserToken))
                throw new Exception(string.Format("Invalid setting '{0}', please double check your App.config", Setting.DiscordUserToken.Name));

            Client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                Intents = GatewayIntentBundles.Guilds | GatewayIntentBundles.GuildMessages | GatewayIntentBundles.GuildAdministration,
                Cache = true 
            });

            Client.OnLoggedIn += OnLoggedIn;
            Client.Login(DiscordUserToken);

            await base.Initialize();
        }

        public async override Task Shutdown()
        {
            await base.Shutdown();
            Client.Logout();
        }

        public async Task<List<DiscordMessageDTO>> DownloadGuildData(List<DiscordChannelModel> peristantChannels)
        {
            StopRoutine = false;

            if (!Initialized || StopRoutine)
                return new List<DiscordMessageDTO>();

            List<DiscordMessageDTO> messages = new List<DiscordMessageDTO>();

            var guilds = await Client.GetGuildsAsync().AwaitTimeout();

            if(guilds == null)
                return messages;

            List<DiscordGuildExtractionConfig> extractionSettings = DiscordManager.GetExtractionSettings();
            List<ulong> targerServerIds = extractionSettings.Select(x => x.GuildId).ToList();

            guilds = guilds.Where(x => targerServerIds.Contains(x.Id)).ToList();

            foreach(var guild in guilds) 
            {
                if (StopRoutine)
                    return messages;

                DiscordGuildExtractionConfig setting = extractionSettings.FirstOrDefault(x => x.GuildId == guild.Id);

                List<ulong> excludedChannelIds = null;

                if (setting != null)
                    excludedChannelIds = setting.ExcludedChannelIds;

                IReadOnlyList<GuildChannel> channels = await guild.GetChannelsAsync().AwaitTimeout();

                if (channels == null)
                    continue;

                if(excludedChannelIds != null)
                    channels = channels.Where(x => !excludedChannelIds.Contains(x.Id)).ToList();

                foreach (var x in channels.Where(x => x.IsText).ToList())
                {
                    if (StopRoutine)
                        return messages;

                    TextChannel channel = x as TextChannel;

                    DiscordChannelModel persistantChannel = peristantChannels.FirstOrDefault(chan => chan.SourceChannelId == channel.Id);
                    MessageFilters filter = null;

                    if(persistantChannel != null && persistantChannel.LatestLog != null)
                        filter = new MessageFilters() { AfterId = persistantChannel.LatestLog.LastSynchedMessageId };

                    Action onGetMessagesTimeout = async () =>
                    {
                        Logger.LogWarning(string.Format("The client timed out when trying to get messages from the channel [yellow]{0}[/] ({1})", channel.Name, channel.Id));

                        if (BotClient != null)
                            await BotClient.LogMessage(string.Format("The client timed out when trying to get messages from the channel \"{0}\" ({1})", channel.Name, channel.Id));
                    };

                    IReadOnlyList<DiscordMessage> channelMessages = await channel.GetMessagesAsync(filter).AwaitTimeout(logError: false, onActionTimeout: onGetMessagesTimeout);

                    if (channelMessages == null)
                        continue;

                    foreach (DiscordMessage message in channelMessages)
                    {
                        if (StopRoutine)
                            return messages;

                        GuildChannel parentChannel = channels.FirstOrDefault(c => c.Id == channel.ParentId);

                        DiscordMessageDTO dto = new DiscordMessageDTO();
                        dto.MessageId = message.Id;
                        dto.GuildId = guild.Id;
                        dto.GuildName = guild.Name;
                        dto.ChannelId = channel.Id;
                        dto.ChannelName = channel.Name;
                        dto.ParentChannelId = channel.ParentId;
                        dto.ParentChannelName = parentChannel != null ? parentChannel.Name : null;
                        dto.MessageContent = message.Content;
                        dto.SentAt = message.SentAt;

                        if (message.Embed != null)
                        {
                            dto.Embed = new DiscordEmbedDTO();
                            dto.Embed.TimeStamp = message.Embed.Timestamp;

                            if(message.Embed.Author != null)
                            {
                                dto.Embed.AuthorName = message.Embed.Author.Name;
                                dto.Embed.AuthorIconUrl = message.Embed.Author.IconUrl;
                                dto.Embed.AuthorIconProxyUrl = message.Embed.Author.IconProxyUrl;
                            }

                            if (message.Embed.Footer != null)
                            {
                                dto.Embed.FooterText = message.Embed.Footer.Text;
                                dto.Embed.FooterIconUrl = message.Embed.Footer.IconUrl;
                                dto.Embed.FooterIconProxyUrl = message.Embed.Footer.IconProxyUrl;
                            }

                            if (message.Embed.Fields != null)
                            {
                                foreach (var item in message.Embed.Fields)
                                {
                                    dto.Embed.Fields.Add(new DiscordEmbedFieldDTO()
                                    {
                                        Content = item.Content,
                                        Name = item.Name,
                                    });
                                }
                            }

                            if (message.Embed.Image != null)
                                dto.Embed.ImageUrl = message.Embed.Image.Url;
                        }

                        foreach (DiscordAttachment attachment in message.Attachments)
                        {
                            DiscordAttachmentDTO aDto = new DiscordAttachmentDTO();
                            aDto.AttachmentType = attachment.ContentType;
                            aDto.DownloadUrl = attachment.ProxyUrl;

                            dto.Attachments.Add(aDto);
                        }

                        messages.Add(dto);
                    }
                }
            }

            return messages;
        }

        public List<DiscordGuildDTO> GetAvailableGuilds()
        {
            var guilds = Client.GetGuilds();

            return guilds != null ? guilds.Select(x => new DiscordGuildDTO()
            {
                GuildId = x.Id,
                GuildName = x.Name,
            })
            .OrderBy(x => x.GuildName)
            .ToList() 
            : new List<DiscordGuildDTO>();
        }

        public List<DiscordChannelDTO> GetAvailableChannels()
        {
            var guilds = Client.GetGuilds();
            var channels = guilds.SelectMany(x => x.GetChannels()).ToList();

            return channels != null ? channels.Select(x => new DiscordChannelDTO()
            {
                ChannelId = x.Id,
                ChannelName = x.Name,
                ParentChannelId = x.ParentId,
                GuildId = x.Guild.Id,
                GuildName = guilds.First(y => y.Id == x.Guild.Id).Name,
            })
            .OrderBy(x => x.GuildName)
            .ThenBy(x => x.ChannelName)
            .ToList()
            : new List<DiscordChannelDTO>();
        }

        #endregion

        #region Private API

        private void OnLoggedIn(DiscordSocketClient client, LoginEventArgs args)
        {
            AnsiConsole.MarkupLine("\n\nYou are now logged in as as [blue]" + args.User.Username + "[/] (id " + args.User.Id + ")\n");
        }

        #endregion
    }
}
