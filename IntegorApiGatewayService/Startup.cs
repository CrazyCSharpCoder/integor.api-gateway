using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

using Microsoft.AspNetCore.Builder;

using IntegorAspHelpers.Middleware.WebApiResponse;

using IntegorServicesInteraction.Authorization;
using IntegorAuthorizationInteraction;

using IntegorServiceConfiguration;
using IntegorServiceConfiguration.Authentication;
using IntegorServiceConfiguration.Controllers;
using IntegorServiceConfiguration.IntegorServicesInteraction;

using IntegorAspHelpers.MicroservicesInteraction.Authorization.RemoteAuthentication;

namespace IntegorApiGatewayService
{
	public class Startup
	{
		private IConfiguration _cookieTypesConfiguration;
		private IConfiguration _authorizationServiceConfiguration;

        public Startup()
        {
			_cookieTypesConfiguration = new ConfigurationBuilder()
				.AddJsonFile("cookie_types_configuration.json")
				.Build();

			_authorizationServiceConfiguration = new ConfigurationBuilder()
				.AddJsonFile("authorization_service_configuration.json")
				.Build();
        }

        public void ConfigureServices(IServiceCollection services)
		{
			// Configuring API
			// services.Configure<AuthorizationServiceConfiguration>(_authorizationServiceConfiguration);
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
			services.AddDefaultStatusCodeResponseBodyFactory();
			services.AddErrorResponseDecorator();

			// Configuring authentication
			services.AddAuthorizationServices(_cookieTypesConfiguration);
			services.AddAuthenticationValidation();
			services.AddOnServiceProcessingTokenAuthentication();

			services.AddAuthenticationTokensSending();

			// Configuring HTTP and authentication strategies
			services.AddHttpContextAccessor();

			services.AddAuthentication(RemoteAccessRefreshAuthenticationDefaults.AuthenticationScheme)
				.AddRemoteAccessRefreshAuthentication(RemoteAccessRefreshAuthenticationDefaults.AuthenticationScheme, null);
		}

		public void Configure(IApplicationBuilder app)
		{
			app.UseWebApiExceptionsHandling();
			app.UseWebApiStatusCodesHandling();

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
