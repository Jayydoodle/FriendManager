using CustomSpectreConsole.Settings;
using CustomSpectreConsole;
using FriendManager.BAL.FriendTech;
using FriendManager.BAL.FriendTechTracker.BAL;
using Newtonsoft.Json;
using OfficeOpenXml;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FriendManager.Services
{
    public static class FriendTechService
    {
        #region Constants

        public const string FriendActivityEndpoint = "https://prod-api.kosetto.com/friends-activity/";
        public const string HolderDetailsEndpoint = "https://prod-api.kosetto.com/users/{0}/token/holders";

        #endregion

        #region Public API

        public static FriendTechHolderDetails GetHolderDetails()
        {
            HttpClient client = new HttpClient();

            string address = XMLSettings.GetSetting(Setting.WalletAddress);

            var request = new HttpRequestMessage(HttpMethod.Get, string.Format(HolderDetailsEndpoint, address));

            HttpResponseMessage response = client.Send(request);
            FriendTechHolderDetails details = new FriendTechHolderDetails();

            using (var reader = new StreamReader(response.Content.ReadAsStream()))
            {
                string body = reader.ReadToEnd();
                details = JsonConvert.DeserializeObject<FriendTechHolderDetails>(body);
            }

            if (details.Users == null)
                return details;

            List<HolderUserMapping> mappings = new List<HolderUserMapping>();

            string googleSheetId = XMLSettings.GetSetting(Setting.GoogleSheetId);

            Action<ExcelWorksheet> processSheet = (sheet) =>
            {
                List<HolderUserMapping> entries = ExcelUtil.ImportData<HolderUserMapping>(sheet);

                details.Users.ForEach(user =>
                {
                    user.UserMapping = entries.Where(x => x.TwitterUserName != null)
                                              .Where(x => x.TwitterUserName.ToLower() == user.TwitterUsername.ToLower())
                                              .Where(x => x.WalletAddress != null)
                                              .Where(x => x.WalletAddress.ToLower() == user.Address.ToLower())
                                              .FirstOrDefault() ?? new HolderUserMapping();
                });
            };

            GoogleSheetsUtil.ProcessSheet(googleSheetId, processSheet);

            return details;
        }

        #endregion
    }
}
