using Microsoft.Extensions.DependencyInjection;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace IntegorApiGatewayService
{
	public class Startup
	{
		public void ConfigureServices(IServiceCollection services)
		{

		}

		public void Configure(IApplicationBuilder app)
		{
			app.Run(async context => await context.Response.WriteAsync("Hello World"));
		}
	}
}
