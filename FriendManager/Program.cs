using CustomSpectreConsole;
using OfficeOpenXml;
using Spectre.Console;
using FriendManager.Functions;
using System.Configuration;

class Program
{
    private const string ApplicationName = "Friend Manager";
    private const string VersionNumber = "1.0";

    static HttpClient WebClient { get; set; }

    static async Task Main(string[] args)
    {
        ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
        WTelegram.Helpers.Log = (s, a) => RedirectLogs();

        WebClient = new HttpClient();

        Console.Clear();

        SelectionPrompt<ListOption> prompt = new SelectionPrompt<ListOption>();
        prompt.Title = "Select an option:";
        List<ListOption> options = CreateListOptions();
        prompt.AddChoices(options);

        bool printMenuHeading = true;

        while (true)
        {
            if (printMenuHeading)
                PrintMenuHeading();

            ListOption option = AnsiConsole.Prompt(prompt);

            if (option.Function != null || option.IsHelpOption)
            {
                try
                {
                    if (option is ManagerBase)
                    {
                        ((ManagerBase)option).WriteHeaderToConsole();
                        ((ManagerBase)option).WebClient = WebClient;
                    }

                    printMenuHeading = true;

                    if (option.IsHelpOption)
                    {
                        printMenuHeading = false;
                        AnsiConsole.Clear();
                        ((ListOption<List<ListOption>, bool>)option).Function(options);
                    }
                    else
                    {
                        option.Function();
                        AnsiConsole.Clear();
                    }
                }
                catch (Exception e)
                {
                    if (e.Message == GlobalConstants.Commands.EXIT)
                        break;
                    else

                        AnsiConsole.Clear();

                    if (e.Message != GlobalConstants.Commands.MENU)
                        AnsiConsole.Write(string.Format("{0}\n\n", e.Message));
                }
            }
            else
            {
                break;
            }
        }
    }

    private static void RedirectLogs()
    {
    }

    private static void PrintMenuHeading()
    {
        Rule rule = new Rule(string.Format("[green]{0} v{1}[/]\n", ApplicationName, VersionNumber)).DoubleBorder<Rule>();
        AnsiConsole.Write(rule);
    }

    private static List<ListOption> CreateListOptions()
    {
        List<ListOption> listOptions = new List<ListOption>();

        listOptions.Add(TelegramManager.Instance);
        listOptions.Add(DiscordManager.Instance);
        listOptions.Add(ConsoleFunction.GetHelpOption());
        listOptions.Add(new ListOption(GlobalConstants.SelectionOptions.Exit, null));

        return listOptions;
    }
}