using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(jakiekieszonkowe.Startup))]
namespace jakiekieszonkowe
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
