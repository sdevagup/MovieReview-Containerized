using System;
using System.Configuration;
using System.Data.Entity;
using System.Diagnostics;
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
            var envConnection = Environment.GetEnvironmentVariable("ConnectionStrings__MovieReview");

            if (!string.IsNullOrEmpty(envConnection))
            {
                var settings = ConfigurationManager.ConnectionStrings["MovieReview"];
                if (settings == null)
                {
                    var connectionStringSettings = new ConnectionStringSettings("MovieReview", envConnection, "System.Data.SqlClient");
                    ConfigurationManager.ConnectionStrings.Add(connectionStringSettings);
                }
                else
                {
                    typeof(ConfigurationElement).GetField("_bReadOnly", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                        ?.SetValue(settings, false);
                    settings.ConnectionString = envConnection;
                }

                Trace.WriteLine($"[Startup] Using DB Connection from ENV: {envConnection}");
            }
            else
            {
                var localConn = ConfigurationManager.ConnectionStrings["MovieReview"]?.ConnectionString;
                Trace.WriteLine($"[Startup] Using DB Connection from Web.config: {localConn}");
            }

            Database.SetInitializer<MovieReviewDbContext>(
                new CreateDatabaseIfNotExists<MovieReviewDbContext>()
            );

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
                    ctx.Database.Connection.Close();
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[Startup] Database initialization FAILED: {ex.Message}");
            }
        }
    }
}
