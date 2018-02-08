using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(EverTrust.Startup))]
namespace EverTrust
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
