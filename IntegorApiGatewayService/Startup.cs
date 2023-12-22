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

using ExternalServicesConfiguration;

using IntegorAspHelpers.MicroservicesInteraction.Authorization.RemoteAuthentication;

using IntegorApiGatewayShared.ObjectParsers;

namespace IntegorApiGatewayService
{
	using Helpers;

	public class Startup
	{
		private IConfiguration _cookieTypesConfiguration;
		private IConfiguration _authorizationServiceConfiguration;
		private IConfiguration _dataServiceConfiguration;
		private IConfiguration _botListeningServiceConfiguration;

        public Startup()
        {
			_cookieTypesConfiguration = new ConfigurationBuilder()
				.AddJsonFile("cookieTypesConfiguration.json")
				.Build();

			_authorizationServiceConfiguration = new ConfigurationBuilder()
				.AddJsonFile("ExternalServices/authorizationServiceConfiguration.json")
				.Build();

			_dataServiceConfiguration = new ConfigurationBuilder()
				.AddJsonFile("ExternalServices/dataServiceConfiguration.json")
				.Build();

			_botListeningServiceConfiguration = new ConfigurationBuilder()
				.AddJsonFile("ExternalServices/botListeningServiceConfiguration.json")
				.Build();
        }

        public void ConfigureServices(IServiceCollection services)
		{
			// Configuring API
			services.Configure<AuthorizationServiceConfiguration>(_authorizationServiceConfiguration);
			services.Configure<DataServiceConfiguration>(_dataServiceConfiguration);
			services.Configure<TelegramBotListeningServiceConfiguration>(_botListeningServiceConfiguration);

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

			// Configuring parsing
			services.AddSingleton<DefaultObjectParser>();

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

			// Add helpers
			services.AddSingleton<IntegorServicesResponseStatusCodesHelper>();

			// Configuring CORS
			services.AddCors(options =>
			{
				options.AddDefaultPolicy(builder =>
				{
					builder
						.WithOrigins("http://localhost:8080/", "http://localhost:8080")
						.AllowAnyHeader()
						.AllowAnyMethod()
						.AllowCredentials();
				});
			});
		}

		public void Configure(IApplicationBuilder app)
		{
			app.UseWebApiExceptionsHandling();
			app.UseWebApiStatusCodesHandling();

			app.UseRouting();
			app.UseCors();

			app.UseAuthentication();
			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}
