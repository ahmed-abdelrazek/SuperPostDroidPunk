using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(WebApplicationPostTest.Areas.Identity.IdentityHostingStartup))]
namespace WebApplicationPostTest.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
            });
        }
    }
}