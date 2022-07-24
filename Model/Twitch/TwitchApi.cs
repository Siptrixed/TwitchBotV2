using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using TwitchBotV2.Model.Twitch.Utils;
using TwitchBotV2.Model.Utils;

namespace TwitchBotV2.Model.Twitch
{
    public static class TwitchApi
    {
        #region Construct
        private static HttpClient HTTP = new HttpClient();
        static TwitchApi()
        {
            HTTP.DefaultRequestHeaders.Add("Client-Id", TwitchConsts.ClientID);
        }
        #endregion

        #region API
        public static async Task<ParsedJson> GetRewards(this TwitchAccount account)
        {
            var response = await account.ApiGetRequestAsync($"https://api.twitch.tv/helix/channel_points/custom_rewards?broadcaster_id={account.UserID}");
            var test = response["data"];
            return test;
        }
        public static async Task<bool> UpdateRewardRedemptionStatus(this TwitchAccount account, string rewardId, string redeemId, bool cancel = false)
        {
            string setStatus = cancel ? "CANCELED" : "FULFILLED";
            var response = await account.ApiRequestAsync($"https://api.twitch.tv/helix/channel_points/custom_rewards/redemptions?broadcaster_id={account.UserID}&reward_id={rewardId}&id={redeemId}",
                                                            "PATCH",$"{{\"status\": \"{setStatus}\"}}", "application/json");
            var list = response["data"].List();
            return list.Count > 0 && list[0]["status"] == setStatus;
        }
        public static async Task<bool> ValidateAsync(this TwitchAccount account)
        {
            var response = await account.ApiGetRequestAsync("https://id.twitch.tv/oauth2/validate", "OAuth");
            account.Login = response["login"];
            account.UserID = response["user_id"];
            if (response["client_id"].ToString() != TwitchConsts.ClientID) return false;
            if (response["expires_in"] < 86400) return false;
            var scopes = TwitchConsts.RequaredScopes.ToList();
            foreach (var scope in response["scopes"].List())
            {
                scopes.Remove(scope);
            }
            if (scopes.Count > 0) return false;
            return true;
        }
        public static async Task<string> GetStreamerIDAsync(this TwitchAccount account, string login)
        {
            var response = await account.ApiGetRequestAsync($"https://api.twitch.tv/helix/users?login={login}");
            var list = response["data"].List();
            return list.Count > 0?list[0]["id"]:"0";
        }
        #endregion

        private static async Task<ParsedJson> ApiRequestAsync(this TwitchAccount account, string url, string method,string content, string contentType, string authorizationType = "Bearer")
        {
            try
            {
                HTTP.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authorizationType, account.Token);
                var httpcontent = new StringContent(content);
                httpcontent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                HttpResponseMessage response;
                switch (method)
                {
                    case "PATCH": response = await HTTP.PatchAsync(url, httpcontent); break;
                    default: response = await HTTP.PostAsync(url, httpcontent); break;
                }
                string body = await response.Content.ReadAsStringAsync();
                return new ParsedJson(body);
            }
            catch (HttpRequestException e)
            {
                return new ParsedJson($"{{'Error':'{e.Message}','Code':'{(e.StatusCode.HasValue ? e.StatusCode.Value : "Null")}'}}");
            }
            catch (Exception e)
            {
                return new ParsedJson($"{{'Error':'{e.Message}'}}");
            }
        }
        private static async Task<ParsedJson> ApiGetRequestAsync(this TwitchAccount account, string url, string authorizationType = "Bearer")
        {
            try
            {
                HTTP.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authorizationType, account.Token);
                var data = await HTTP.GetAsync(url);
                string body = await data.Content.ReadAsStringAsync();
                return new ParsedJson(body);
            }
            catch (HttpRequestException e)
            {
                return new ParsedJson($"{{'Error':'{e.Message}','Code':'{(e.StatusCode.HasValue?e.StatusCode.Value:"Null")}'}}");
            }
            catch (Exception e)
            {
                return new ParsedJson($"{{'Error':'{e.Message}'}}");
            }
        }
    }
}
