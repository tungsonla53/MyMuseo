using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MyMuseo.Startup))]
namespace MyMuseo
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            
        }
    }
}
