using CustomSpectreConsole;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Util.Store;
using Newtonsoft.Json;
using OfficeOpenXml;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Channels;
using TGFriendTracker;
using TGFriendTracker.BAL;
using TGFriendTracker.BAL.FriendTechTracker.BAL;
using TGFriendTracker.Functions;
using TL;
using WTelegram;
using Channel = TL.Channel;

class Program
{
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
        Rule rule = new Rule(string.Format("[green]FriendTracker v{0}[/]\n", VersionNumber)).DoubleBorder<Rule>();
        AnsiConsole.Write(rule);
    }

    private static List<ListOption> CreateListOptions()
    {
        List<ListOption> listOptions = new List<ListOption>();

        listOptions.Add(TelegramUserManager.Instance);
        listOptions.Add(ConsoleFunction.GetHelpOption());
        listOptions.Add(new ListOption(GlobalConstants.SelectionOptions.Exit, null));

        return listOptions;
    }
}