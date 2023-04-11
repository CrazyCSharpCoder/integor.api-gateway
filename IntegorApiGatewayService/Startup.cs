using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;

using IntegorAspHelpers.Middleware.WebApiResponse;
using IntegorAspHelpers.MicroservicesInteraction;
using IntegorAspHelpers.MicroservicesInteraction.Filters;
using IntegorAspHelpers.MicroservicesInteraction.Authorization.AuthenticationHandlers.Access;

using IntegorServicesInteraction.Authorization;
using IntegorAuthorizationInteraction;

using IntegorServiceConfiguration;
using IntegorServiceConfiguration.Authentication;
using IntegorServiceConfiguration.Controllers;
using IntegorServiceConfiguration.IntegorServicesInteraction;

namespace IntegorApiGatewayService
{
	public class Startup
	{
		private IConfiguration _cookieTypesConfiguration;

        public Startup()
        {
			_cookieTypesConfiguration = new ConfigurationBuilder()
				.AddJsonFile("cookie_types_configuration.json")
				.Build();
        }

        public void ConfigureServices(IServiceCollection services)
		{
			services.AddSingleton(new AuthorizationServiceConfiguration()
			{
				// TODO start getting object from file
				Url = "http://localhost:5179"
			});
			services.AddScoped<IAuthorizationServiceAuthApi, AuthorizationServiceAuthApi>();
			services.AddScoped<IAuthorizationServiceUsersApi, AuthorizationServiceUsersApi>();

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

			services.AddAuthentication(ValidateAccessAuthenticationDefaults.AuthenticationScheme)
				.AddAccessAuthenticationViaMicroservice(ValidateAccessAuthenticationDefaults.AuthenticationScheme, null);

			services.AddAuthorizationServices(_cookieTypesConfiguration);
			services.AddAuthenticationTokensSending();

			services.AddAuthenticationTokensProcessing();

			services.AddUserReceiving();
			services.AddUserSending();

			services.AddSingleton<ServiceResponseToActionResultHelper>();
		}

		public void Configure(IApplicationBuilder app)
		{
			app.UseWebApiExceptionsHandling(WriteJsonBody);
			app.Use(async (context, next) =>
			{
				try
				{
					await next(context);
				}
				catch (Exception exc)
				{
					await Console.Out.WriteLineAsync(exc.Message);
					throw;
				}
			});
			app.UseWebApiStatusCodesHandling(WriteJsonBody);

			app.UseRouting();

			app.UseAuthentication();
			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}

		private async Task WriteJsonBody(HttpResponse response, object body)
			 => await response.WriteAsJsonAsync(body);
	}
}
