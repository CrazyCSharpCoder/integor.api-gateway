using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;

namespace IntegorApiGatewayService
{
	public class Program
	{
		public static void Main(string[] args)
		{
			IHostBuilder builder = Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseStartup<Startup>();
				});

			IHost host = builder.Build();
			host.Run();
		}
	}
}
