using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Hangfire;
using Hangfire.SqlServer;
using System.Collections.Specialized;
[assembly: OwinStartup(typeof(connect.App_Start.OwinStartup))]

namespace connect.App_Start
{
    public class OwinStartup
    {
        public void Configuration(IAppBuilder app)
        {
            // Map Dashboard to the `http://<your-app>/hangfire` URL.
            SqlServerStorageOptions options = new SqlServerStorageOptions();
            options.PrepareSchemaIfNecessary = true;
            JobStorage.Current = new Hangfire.SqlServer.SqlServerStorage("db_connection", options);
            
            //GlobalConfiguration.Configuration
                // Use connection string name defined in `web.config` or `app.config`
               // .UseSqlServerStorage("db_connection")
                // Use custom connection string
               // .UseSqlServerStorage(@"Server=.\sqlexpress; Database=Hangfire; Integrated Security=SSPI;");
            app.UseHangfireDashboard();
            app.UseHangfireServer();
        }
    }
}
