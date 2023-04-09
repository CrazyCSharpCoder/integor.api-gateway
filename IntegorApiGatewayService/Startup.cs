using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;

using IntegorServiceConfiguration;
using IntegorAspHelpers.Middleware.WebApiResponse;

namespace IntegorApiGatewayService
{
	public class Startup
	{
        public void ConfigureServices(IServiceCollection services)
		{
			Type exceptionBaseConverter = services.AddExceptionConverting();

			IEnumerable<Type> exceptionConverters = services
				.AddDatabaseExceptionConverters()
				.Prepend(exceptionBaseConverter);

			services.AddPrimaryTypesErrorConverters();

			services.AddConfiguredControllers(exceptionConverters.ToArray());

			services.AddHttpContextServices();
			services.AddResponseDecorators();
		}

		public void Configure(IApplicationBuilder app)
		{
			app.UseWebApiExceptionsHandling(WriteJsonBody);
			app.UseWebApiStatusCodesHandling(WriteJsonBody);

			app.UseRouting();
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}

		private async Task WriteJsonBody(HttpResponse response, object body)
			 => await response.WriteAsJsonAsync(body);
	}
}
