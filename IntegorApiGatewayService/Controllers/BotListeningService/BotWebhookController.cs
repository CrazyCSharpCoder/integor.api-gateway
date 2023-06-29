using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;
using System.Net.Http;
using System.IO;

using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;

using IntegorErrorsHandling;
using IntegorResponseDecoration.Parsing;

using IntegorAspHelpers.MicroservicesInteraction;

using IntegorServicesInteraction;

using ExternalServicesConfiguration;
using IntegorServicesInteractionHelpers;

using IntegorApiGatewayShared.ObjectParsers;

namespace IntegorApiGatewayService.Controllers.BotListeningService
{
	[ApiController]
	[Route("bots")]
	public class BotWebhookController : ControllerBase
	{
		private TelegramBotListeningServiceConfiguration _serviceConfiguration;

		public BotWebhookController(
			IOptions<TelegramBotListeningServiceConfiguration> serviceOptions)
		{
			_serviceConfiguration = serviceOptions.Value;
		}

		[HttpGet("{botToken}/getWebhookInfo")]
		public async Task<IActionResult> GetWebhookInfoAsync(string botToken)
		{
			string url = _serviceConfiguration.Url + $"bot{botToken}/getWebhookInfo";

			using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);

			using HttpClient client = new HttpClient();
			using HttpResponseMessage response = await client.SendAsync(request);

			using Stream body = await response.Content.ReadAsStreamAsync();
			JsonElement jsonBody = await JsonSerializer.DeserializeAsync<JsonElement>(body);

			response.Dispose();

			return new ObjectResult(jsonBody)
			{
				StatusCode = (int)response.StatusCode
			};
		}
	}
}
