using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CustomSpectreConsole;
using Microsoft.VisualBasic;
using Spectre.Console;
using System.IO;
using System.Net.NetworkInformation;
using System.Xml.Linq;

namespace TGFriendTracker
{
    public static class Settings
    {
        #region Constants

        public const string FileName = "settings.xml";

        #endregion

        #region Public API

        public static T GetDirectoryOrFile<T>(Node node, string prompt = null, bool isRequired = true)
        where T : FileSystemInfo
        {
            T info = null;
            string path = GetValue(node);

            if (!string.IsNullOrEmpty(path))
                info = (T)Activator.CreateInstance(typeof(T), new object[] { path });

            if (info == null || !info.Exists)
            {
                if (string.IsNullOrEmpty(prompt))
                    prompt = GetDefaultPromptByNode(node);

                info = Utilities.GetFileSystemInfoFromInput<T>(prompt, isRequired);

                if (info != null && info.Exists)
                    Update(node, info.FullName);
            }

            return info;
        }

        public static DirectoryInfo GetDirectory(Node node, string prompt = null, bool isRequired = true)
        {
            DirectoryInfo dir = GetDirectoryOrFile<DirectoryInfo>(node, prompt, isRequired);

            return dir;
        }

        public static FileInfo GetFile(Node node, string prompt = null, bool isRequired = true)
        {
            FileInfo file = GetDirectoryOrFile<FileInfo>(node, prompt, isRequired);
            return file;
        }

        public static DirectoryInfo GetDirectoryFromNode(Node node, string expectedPath)
        {
            DirectoryInfo dir = Settings.GetDirectory(node);
            string path = Path.Combine(dir.FullName, expectedPath);
            DirectoryInfo targetDir = new DirectoryInfo(path);

            while (!targetDir.Exists)
            {
                string message = string.Format("The target folder is expected at the path [red]{1}[/], but the directory does " +
                "not exist.  Please enter the correct path: ", path);

                path = Utilities.GetInput(message, x => !string.IsNullOrEmpty(x));
                targetDir = new DirectoryInfo(path);
            }

            return targetDir;
        }

        public static string GetSetting(Node node, Func<Node, string, bool> ValidationFunction = null, bool validateAlways = true, string prompt = null)
        {
            string value = GetValue(node);
            bool validated = false;

            if (!string.IsNullOrEmpty(value))
            {
                if(ValidationFunction == null || !validateAlways)
                    validated = true;
                else
                    validated = ValidationFunction(node, value);
            }
            
            while(!validated)
            {
                if (string.IsNullOrEmpty(prompt))
                    prompt = GetDefaultPromptByNode(node);

                value = Utilities.GetInput(prompt, x => !string.IsNullOrEmpty(x));

                if (ValidationFunction != null)
                    validated = ValidationFunction(node, value);
                else
                    validated = true;
            }

            Update(node, value);

            return value;
        }

        public static Node GetNode(string nodeName)
        {
            return Enum.Parse<Node>(nodeName);
        }

        public static Dictionary<Node, string> GetValues(Func<Node, bool> predicate = null)
        {
            Dictionary<Node, string> paths = new Dictionary<Node, string>();
            XDocument xml = GetDocument();

            IEnumerable<Node> nodes = Enum.GetValues<Node>();

            if (predicate != null)
                nodes = nodes.Where(predicate);

            nodes.ToList()
            .ForEach(x =>
            {
                XElement elem = xml.Root.Element(x.ToString());

                if (elem != null)
                    paths.Add(x, elem.Value);
            });

            return paths;
        }

        public static void Update(Node node, string value)
        {
            XDocument xml = GetDocument();
            XElement elem = xml.Root.Element(node.ToString());

            if (elem == null)
            {
                elem = new XElement(node.ToString());
                xml.Root.Add(elem);
            }

            elem.Value = value;
            xml.Save(FileName);
        }

        public static bool HasEntry(Node node)
        {
            return !string.IsNullOrEmpty(GetValue(node));
        }

        public static string GetValue(Node node)
        {
            XDocument xml = GetDocument();
            XElement elem = xml.Root.Element(node.ToString());

            return elem != null ? elem.Value : null;
        }

        #endregion

        #region Private API

        private static XDocument GetDocument()
        {
            XDocument xml = null;

            try { xml = XDocument.Load(FileName); }
            catch (Exception)
            {
                xml = new XDocument(new XElement("Settings"));

                Enum.GetNames<Node>()
                .ToList()
                .ForEach(x =>
                {
                    xml.Root.Add(new XElement(x));
                });
            }

            return xml;
        }

        private static string GetDefaultPromptByNode(Node node)
        {
            switch (node)
            {
                case Node.TelegramAPIKey:
                    return "Enter your API ID: ";

                case Node.TelegramAPIHash:
                    return "Enter your API Hash: ";

                case Node.TelegramPhoneNumber:
                    return "Enter your phone number: ";

                case Node.WalletAddress:
                    return "\nEnter your wallet address: ";

                case Node.TelegramChatId:
                    return "\nEnter the Chat ID of the chat you wish to manage: ";

                case Node.ExcelFilePath:
                    return "\nEnter the path to the excel file containing your user mappings for Twitter/Telegram names.  " +
                        "This sheet should have the following columns: [blue]Twitter[/], [blue]Telegram[/], [blue]Wallet Address[/] (optional), with no @ symbols: ";

                case Node.GoogleSheetId:
                    return "\nEnter the sheet id to the Google sheet containing your user mappings for Twitter/Telegram names.  " +
                        "This sheet should have the following columns: [blue]Twitter[/], [blue]Telegram[/], [blue]Wallet Address[/] (optional), with no @ symbols: ";

                case Node.TelegramRemovalInterval:
                    return "\nEnter the interval at which you want the scheduler to be run (in minutes): ";

                default:
                    return string.Format("Enter in a value for the {0}: ", node.ToString().SplitByCase()); ;
            }
        }

        #endregion

        #region Helper Classes

        public enum Node
        {
            TelegramAPIKey,
            TelegramAPIHash,
            TelegramPhoneNumber,
            TelegramRemovalInterval,
            TelegramExcludedUsers,
            WalletAddress,
            TelegramChatId,
            ExcelFilePath,
            GoogleSheetId
        }

        #endregion
    }
}
