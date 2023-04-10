using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

using Microsoft.Extensions.DependencyInjection;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;

using IntegorAspHelpers.Middleware.WebApiResponse;
using IntegorAspHelpers.MicroservicesInteraction;
using IntegorAspHelpers.MicroservicesInteraction.Filters;

using IntegorServicesInteraction.Authorization;
using IntegorAuthorizationInteraction;

using IntegorServiceConfiguration;
using IntegorServiceConfiguration.Controllers;
using IntegorServiceConfiguration.IntegorServicesInteraction;

namespace IntegorApiGatewayService
{
	public class Startup
	{
        public void ConfigureServices(IServiceCollection services)
		{
			services.AddSingleton(new AuthorizationServiceConfiguration()
			{
				// TODO start getting object from file
				Url = "http://localhost:5179"
			});
			services.AddScoped<IAuthorizationServiceAuthApi, AuthorizationServiceAuthApi>();

			Type exceptionBaseConverter = services.AddExceptionConverting();

			IEnumerable<Type> exceptionConverters = services
				.AddDatabaseExceptionConverters()
				.Prepend(exceptionBaseConverter);

			services.AddPrimaryTypesErrorConverters();

			services.AddSingleton<ApplicationServiceErrorsTranslationFilterAttribute>();

			services.AddControllersWithProcessedMarking(options =>
			{
				options.Filters.AddErrorsDecoration();
				options.Filters.AddErrorsHandling(exceptionConverters.ToArray());
				options.Filters.AddServiceErrorsToActionResult();
				options.Filters.AddSetProcessedByDefault();
			})
			.ConfigureApiBehaviorOptions(options =>
			{
				options.SetDefaultInvalidModelStateResponseFactory();
			});

			services.AddHttpContextServices();
			services.AddResponseDecorators();

			services.AddIntegorServicesJsonErrorsParsing();
			services.AddServicesErrorsToActionResultTranslation();

			services.AddAuthenticationTokensSending();

			services.AddUserReceiving();
			services.AddUserSending();

			services.AddSingleton<ServiceResponseToActionResultHelper>();
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
