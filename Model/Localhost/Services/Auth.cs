using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TwitchBotV2.Model.Utils;

namespace TwitchBotV2.Model.Localhost.Services
{
    [WebService]
    public static class Auth
    {
        public static string Token(HttpListenerRequest request)
        {
            if (string.IsNullOrEmpty(request.Url?.Query))
            {
                return "<script>window.location = window.location.href.replace('#','?');</script>";
            }
            else
            {
                string token = request.QueryString["access_token"] ?? "SCHMOOPIIE";
                GlobalModel.TwithcAccountAuthorized?.Invoke(WebServer.Host, token);
                return "Закройте пожалуйста окно если оно не закрылось автоматически!<script>window.close();</script>";
            }
        }
    }
}
