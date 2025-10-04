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
            // Log startup to file
            try
            {
                var logPath = @"C:\inetpub\wwwroot\startup.log";
                File.AppendAllText(logPath, $"{DateTime.UtcNow:u} - Application_Start() triggered{Environment.NewLine}");
            }
            catch (Exception ex)
            {
                // Log file write failure to Trace if it happens
                Trace.WriteLine($"[Startup] Failed to write to startup.log: {ex.Message}");
            }

            // Resolve connection string (from ENV or Web.config) ---
            var envConnection = Environment.GetEnvironmentVariable("ConnectionStrings__MovieReview");

            if (!string.IsNullOrEmpty(envConnection))
            {
                try
                {
                    var settings = ConfigurationManager.ConnectionStrings["MovieReview"];
                    if (settings == null)
                    {
                        var connectionStringSettings = new ConnectionStringSettings("MovieReview", envConnection, "System.Data.SqlClient");
                        ConfigurationManager.ConnectionStrings.Add(connectionStringSettings);
                    }
                    else
                    {
                        typeof(ConfigurationElement)
                            .GetField("_bReadOnly", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                            ?.SetValue(settings, false);
                        settings.ConnectionString = envConnection;
                    }

                    Trace.WriteLine($"[Startup] Using DB Connection from ENV: {envConnection}");
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"[Startup] Failed to override connection string: {ex.Message}");
                }
            }
            else
            {
                var localConn = ConfigurationManager.ConnectionStrings["MovieReview"]?.ConnectionString;
                Trace.WriteLine($"[Startup] Using DB Connection from Web.config: {localConn}");
            }

            Database.SetInitializer<MovieReviewDbContext>(new CreateDatabaseIfNotExists<MovieReviewDbContext>());

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            try
            {
                using (var ctx = new MovieReviewDbContext())
                {
                    ctx.Database.Initialize(force: true);
                    ctx.Database.Connection.Open();
                    Trace.WriteLine("[Startup] Database initialization and connection successful!");
                    File.AppendAllText(@"C:\inetpub\wwwroot\startup.log", $"{DateTime.UtcNow:u} - Database initialization successful{Environment.NewLine}");
                    ctx.Database.Connection.Close();
                }
            }
            catch (Exception ex)
            {
                var err = $"[Startup] Database initialization FAILED: {ex.Message}";
                Trace.WriteLine(err);
                File.AppendAllText(@"C:\inetpub\wwwroot\startup.log", $"{DateTime.UtcNow:u} - {err}{Environment.NewLine}");
            }
        }
    }
}
