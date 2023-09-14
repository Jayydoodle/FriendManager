using CustomSpectreConsole;
using FriendManager.DAL.Discord;

namespace FriendManager
{
    public class ConnectionStrings : SettingsNode<ConnectionStrings>
    {
        public override SettingsNodeType Type => SettingsNodeType.DatabaseConnection;

        public static ConnectionStrings Discord => new ConnectionStrings()
        {
            Name = "DiscordConnectionStrings",
            Prompt = "\nEnter your the connection strings for your Postgres database: "
        };
    }
}
