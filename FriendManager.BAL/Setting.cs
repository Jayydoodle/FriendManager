using CustomSpectreConsole;
using FriendManager.BAL.FriendTech;
using FriendManager.DAL.Discord;

namespace FriendManager
{
    public class Setting : SettingsNode<Setting>
    {

        #region Constants

        public const int MinSyncInterval = 5;
        public const int DefaultSyncInterval = 10;

        public const int MinPurgeInterval = 10;
        public const int DefaultPurgeInterval = 60;

        #endregion


        #region Properties

        public override SettingsNodeType Type => SettingsNodeType.Application;

        #endregion

        #region Instances
        
        public static Setting DiscordBotToken => new Setting()
        {
            Name = nameof(DiscordBotToken),
            Prompt = "\nEnter the token for the bot being used to manager your Discord Server: "
        };

        public static Setting DiscordLogChannelId => new Setting()
        {
            Name = nameof(DiscordLogChannelId),
            Prompt = "\nEnter the id for the channel used to send logs in your Discord Server: "
        };

        public static Setting DiscordExcludedUsers => new Setting()
        {
            Name = nameof(DiscordExcludedUsers),
            Prompt = "\nEnter a comma delimited list of users (Discord user names) that should be excluded from server purging: "
        };

        public static Setting DiscordPurgeInterval => new Setting()
        {
            Name = nameof(DiscordPurgeInterval),
            Prompt = string.Format("\nEnter the interval which invalid users should be removed from your Discord Server (in minutes).  Value must be greater than or equal to {0}: ", MinPurgeInterval)
        };

        public static Setting DiscordServerId => new Setting()
        {
            Name = nameof(DiscordServerId),
            Prompt = "\nEnter the id of your Discord Server: "
        };

        public static Setting DiscordSyncInterval => new Setting()
        {
            Name = nameof(DiscordSyncInterval),
            Prompt = string.Format("\nEnter the interval in which scheduled synchronization should run (in minutes).  Value must be greater than or equal to {0}: ", MinSyncInterval)
        };

        public static Setting DiscordServerExtractionSettings => new Setting()
        {
            Name = nameof(DiscordServerExtractionSettings),
            Prompt = "\nSelect the Discord Servers you wish to target for data extraction: "
        };

        public static Setting DiscordUserToken => new Setting()
        {
            Name = nameof(DiscordUserToken),
            Prompt = "\nEnter the auth token for your Discord User Account: "
        };

        public static Setting GoogleSheetId => new Setting()
        {
            Name = nameof(GoogleSheetId),
            Prompt = string.Format("\nEnter the sheet id to the " + GlobalConstants.MarkUp.Google + " sheet containing your holder mappings for Twitter to Discord/Telegram names.  " +
            "This sheet should have the following columns: [blue]{0}[/], [blue]{1}[/], [blue]{2}[/], [blue]{3}[/], with no @ symbols: ",
            nameof(HolderUserMapping.TwitterUserName), nameof(HolderUserMapping.DiscordUserName), nameof(HolderUserMapping.TelegramUserName), nameof(HolderUserMapping.WalletAddress))
        };

        public static Setting TelegramAPIHash => new Setting()
        {
            Name = nameof(TelegramAPIHash),
        };

        public static Setting TelegramAPIKey => new Setting()
        {
            Name = nameof(TelegramAPIKey),
        };

        public static Setting TelegramChatId => new Setting()
        {
            Name = nameof(TelegramChatId),
            Prompt = "\nEnter the Chat ID of the chat you wish to manage: "
        };

        public static Setting TelegramExcludedUsers => new Setting()
        {
            Name = nameof(TelegramExcludedUsers),
            Prompt = "\nEnter a comma delimited list of users (Telegram user names) that should be excluded from channel purging: "
        };

        public static Setting TelegramPhoneNumber => new Setting()
        {
            Name = nameof(TelegramPhoneNumber),
        };

        public static Setting TelegramPurgeInterval => new Setting()
        {
            Name = nameof(TelegramPurgeInterval),
            Prompt = "\nEnter the interval which invalid users should be removed from your Telegram group (in minutes): "
        };

        public static Setting WalletAddress => new Setting()
        {
            Name = nameof(WalletAddress),
            Prompt = "\nEnter your (public) [cyan]Friend.tech[/] wallet address: "
        };

        #endregion
    }
}
