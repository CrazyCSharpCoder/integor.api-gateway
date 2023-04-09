using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

using Microsoft.Extensions.DependencyInjection;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;

using IntegorSharedErrorHandlers;
using IntegorSharedResponseDecorators.Authorization.Decorators;

using IntegorAspHelpers.Middleware.WebApiResponse;

using IntegorErrorsHandling;

using IntegorServicesInteraction.Authorization;
using IntegorAuthorizationInteraction;

using IntegorServiceConfiguration;
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

			services.AddConfiguredControllers(exceptionConverters.ToArray());

			services.AddHttpContextServices();
			services.AddResponseDecorators();

			services.AddIntegorServices();

			// TODO move to IntegorServiceConfiguration
			services.AddSingleton<IHttpErrorsObjectParser<JsonElement>, StandardJsonHttpErrorsObjectParser>();
			services.AddSingleton<IErrorParser<JsonError, JsonElement>, JsonErrorParser>();

			services.AddAuthenticationTokensSending();
			services.AddUserReceiving();

			// TODO move to IntegorServiceConfiguration AddUserSending
			services.AddSingleton<UserResponseObjectDecorator>();
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
