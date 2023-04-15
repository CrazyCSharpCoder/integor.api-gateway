using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

using Microsoft.AspNetCore.Builder;

using IntegorAspHelpers.Middleware.WebApiResponse;
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
			// Configuring API
			services.AddSingleton(new AuthorizationServiceConfiguration()
			{
				// TODO start getting object from file
				Url = "http://localhost:5179"
			});
			services.AddScoped<IAuthorizationServiceAuthApi, AuthorizationServiceAuthApi>();
			services.AddScoped<IAuthorizationServiceUsersApi, AuthorizationServiceUsersApi>();

			services.AddIntegorServicesJsonErrorsParsing();

			services.AddUserReceiving();
			services.AddUserSending();

			// Configuring errors handling
			Type exceptionBaseConverter = services.AddExceptionConverting();

			IEnumerable<Type> exceptionConverters = services
				.AddDatabaseExceptionConverters()
				.Prepend(exceptionBaseConverter);

			services.AddPrimaryTypesErrorConverters();

			services.AddResponseErrorsObjectCompiler();

			// Configuring MVC
			services.AddControllers(options =>
			{
				options.Filters.AddErrorsDecoration();
				options.Filters.AddErrorsHandling(exceptionConverters.ToArray());
				options.Filters.AddServiceErrorsToActionResult();
			})
			.ConfigureApiBehaviorOptions(options =>
			{
				options.SetDefaultInvalidModelStateResponseFactory();
			});

			// Configuring sending response helpers
			services.AddServiceErrorsToActionResultTranslation();
			services.AddStatusCodeResponseBodyFactory();
			services.AddErrorResponseDecorator();

			// Configuring authentication
			services.AddAuthorizationServices(_cookieTypesConfiguration);
			services.AddAuthenticationTokensSending();
			services.AddAuthenticationValidation();
			services.AddAuthenticationTokensProcessing();

			// Configuring HTTP and authentication strategies
			services.AddHttpContextAccessor();

			services.AddAuthentication(ValidateAccessAuthenticationDefaults.AuthenticationScheme)
				.AddAccessAuthenticationViaMicroservice(ValidateAccessAuthenticationDefaults.AuthenticationScheme, null);
		}

		public void Configure(IApplicationBuilder app)
		{
			app.UseWebApiExceptionsHandling();
			app.UseWebApiStatusCodesHandling();

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

			app.UseRouting();

			app.UseAuthentication();
			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}
