using System;
using System.Configuration;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using MovieReview.Data;

namespace MovieReview.Web
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            string logPath = @"C:\inetpub\wwwroot\startup.log";

            void SafeLog(string msg)
            {
                try
                {
                    File.AppendAllText(logPath, $"[{DateTime.Now}] {msg}{Environment.NewLine}");
                }
                catch (Exception e)
                {
                    // Fallback to Trace (goes to Application event log)
                    Trace.WriteLine($"[FallbackLog] {msg} (LogWriteFailed: {e.Message})");
                }
            }

            SafeLog("==== Application_Start triggered ====");

            try
            {
                var envConnection = Environment.GetEnvironmentVariable("ConnectionStrings__MovieReview");

                if (!string.IsNullOrEmpty(envConnection))
                {
                    // Step 2: Override Web.config connection
                    var settings = ConfigurationManager.ConnectionStrings["MovieReview"];
                    if (settings == null)
                    {
                        var connectionStringSettings =
                            new ConnectionStringSettings("MovieReview", envConnection, "System.Data.SqlClient");
                        ConfigurationManager.ConnectionStrings.Add(connectionStringSettings);
                    }
                    else
                    {
                        typeof(ConfigurationElement).GetField("_bReadOnly",
                            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                            ?.SetValue(settings, false);
                        settings.ConnectionString = envConnection;
                    }

                    SafeLog($"Using DB Connection from ENV: {envConnection}");
                }
                else
                {
                    var localConn = ConfigurationManager.ConnectionStrings["MovieReview"]?.ConnectionString;
                    SafeLog($"Using DB Connection from Web.config: {localConn}");
                }

                Database.SetInitializer(new CreateDatabaseIfNotExists<MovieReviewDbContext>());

                AreaRegistration.RegisterAllAreas();
                GlobalConfiguration.Configure(WebApiConfig.Register);
                FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
                RouteConfig.RegisterRoutes(RouteTable.Routes);
                BundleConfig.RegisterBundles(BundleTable.Bundles);

                // Step 5: Database test connection + creation
                using (var ctx = new MovieReviewDbContext())
                {
                    ctx.Database.Initialize(force: true);
                    ctx.Database.Connection.Open();
                    SafeLog("Database initialization and connection successful!");
                    ctx.Database.Connection.Close();
                }

                SafeLog("==== Application_Start complete ====");
            }
            catch (Exception ex)
            {
                SafeLog($"[Startup Fatal Error] {ex}");
            }
        }
    }
}
