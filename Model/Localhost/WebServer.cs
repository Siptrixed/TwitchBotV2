using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TwitchBotV2.Model.Twitch;

namespace TwitchBotV2.Model.Localhost
{
    public class WebServer
    {
        public static WebServer? Host;
        private static Dictionary<string, MethodInfo> Services = new Dictionary<string, MethodInfo>();
        private static void UpdateServices()
        {
            foreach (var service in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (service.GetCustomAttributes(typeof(WebServiceAttribute), true).Length > 0)
                {
                    foreach (var meth in service.GetMethods())
                    {
                        if (meth.GetCustomAttributes(typeof(WebMethodAttribute), true).Length > 0)
                        {
                            var pars = meth.GetParameters();
                            if (pars.Length == 1 && meth.ReturnParameter.ParameterType == typeof(string) &&
                                pars[0].ParameterType == typeof(HttpListenerRequest) && meth.IsStatic && !meth.IsConstructor)
                            {
                                Services.Add($"{service.Name}/{meth.Name}", meth);
                            }
                        }
                    }
                }
            }
        }
        public static void RunServer()
        {
            Task.Run(() => {
                UpdateServices();
                Host = new WebServer("http://localhost:8181/");
                Host.Run();
            });
        }
        private readonly HttpListener _listener = new HttpListener();
        private readonly Func<HttpListenerRequest, string> _responderMethod;

        public static string WebRequestу(HttpListenerRequest request)
        {
            var segs = request.Url?.AbsolutePath.Split('/');
            if (segs?.Length < 3) return "<script>alert('Some error in link, please close window');window.close();</script>";
            var servicename = $"{segs[1]}/{segs[2]}";
            if (Services.ContainsKey(servicename))
            {
                return (string)(Services[servicename].Invoke(null, new object[1] { request })??"");
            }
            return $"<script>alert('Service {servicename} not found, please close window');window.close();</script>";
        }

        #region Legacy
        private WebServer(IReadOnlyCollection<string> prefixes, Func<HttpListenerRequest, string> method)
        {
            if (!HttpListener.IsSupported)
            {
                throw new NotSupportedException("Needs Windows XP SP2, Server 2003 or later.");
            }

            // URI prefixes are required eg: "http://localhost:8080/test/"
            if (prefixes == null || prefixes.Count == 0)
            {
                throw new ArgumentException("URI prefixes are required");
            }

            foreach (var s in prefixes)
            {
                _listener.Prefixes.Add(s);
            }

            _responderMethod = method ?? throw new ArgumentException("responder method required");
            _listener.Start();
        }
        private WebServer(params string[] prefixes)
           : this(prefixes, WebRequestу)
        {
        }
        private WebServer(Func<HttpListenerRequest, string> method, params string[] prefixes)
           : this(prefixes, method)
        {
        }
        private void Run()
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                //Console.WriteLine("Webserver running...");
                try
                {
                    while (_listener.IsListening)
                    {
                        ThreadPool.QueueUserWorkItem(c =>
                        {
                            HttpListenerContext ctx = c as HttpListenerContext;
                            try
                            {
                                if (ctx == null)
                                {
                                    return;
                                }

                                var rstr = _responderMethod(ctx.Request);
                                var buf = Encoding.UTF8.GetBytes(rstr);
                                ctx.Response.ContentType = "text/html; charset=utf-8";
                                ctx.Response.ContentLength64 = buf.Length;
                                ctx.Response.OutputStream.Write(buf, 0, buf.Length);
                            }
                            catch
                            {
                                // ignored
                            }
                            finally
                            {
                                // always close the stream
                                if (ctx != null)
                                {
                                    ctx.Response.OutputStream.Close();
                                }
                            }
                        }, _listener.GetContext());
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            });
        }
        private void Stop()
        {
            _listener.Stop();
            _listener.Close();
        }
        #endregion
    }
}
