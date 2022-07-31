using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TwitchBotV2.Model.Localhost.Services
{
    [WebService]
    public static class Widgets
    {
        [WebMethod]
        public static string Get(HttpListenerRequest request)
        {
            var wName = request.QueryString["W"];
            if (wName != null) {
                var wRes = Properties.Resources.ResourceManager.GetString($"Widget{wName}");
                if (wRes != null)
                    return wRes;
            }
            return "Виджет не найден";
        }
    }
}
