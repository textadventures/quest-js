using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(QuestJSWeb.Startup))]
namespace QuestJSWeb
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
