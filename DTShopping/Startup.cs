using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(DTShopping.Startup))]
namespace DTShopping
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
