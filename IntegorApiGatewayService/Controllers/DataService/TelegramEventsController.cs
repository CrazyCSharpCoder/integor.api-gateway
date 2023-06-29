using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http;

using Microsoft.Extensions.Options;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using IntegorErrorsHandling;
using IntegorResponseDecoration.Parsing;

using ExtensibleRefreshJwtAuthentication.Access;
using ExtensibleRefreshJwtAuthentication.Refresh;

using IntegorServicesInteraction;
using IntegorServicesInteractionHelpers;

using ExternalServicesConfiguration;

using IntegorDataServiceDto;

using IntegorApiGatewayShared.ObjectParsers;

namespace IntegorApiGatewayService.Controllers.DataService
{
	using Helpers;

	[ApiController]
	[Authorize]
	[Route("telegram-events")]
	public class TelegramEventsController : ControllerBase
	{
		private AuthenticatedJsonServicesRequestProcessor<DataServiceConfiguration> _requestProcessor;
		private UserAuthentication _authentication;

		private DefaultObjectParser _objectParser;
		private IntegorServicesResponseStatusCodesHelper _responseHelper;

		public TelegramEventsController(
			IOnServiceProcessingAccessTokenAccessor accessTokenAccessor,
			IOnServiceProcessingRefreshTokenAccessor refreshTokenAccessor,

			ISendRequestAccessTokenAccessor sendAccessTokenAccessor,
			ISendRequestRefreshTokenAccessor sendRefreshTokenAccessor,
			IDecoratedObjectParser<IEnumerable<IResponseError>, JsonElement> errorParser,

			DefaultObjectParser objectParser,
			IntegorServicesResponseStatusCodesHelper responseHelper,

			IOptions<DataServiceConfiguration> dataServiceOptions)
        {
			_requestProcessor = new AuthenticatedJsonServicesRequestProcessor<DataServiceConfiguration>(
				sendAccessTokenAccessor, sendRefreshTokenAccessor, errorParser, dataServiceOptions, "telegram-events");

			_authentication = new UserAuthentication(
				accessTokenAccessor.GetFromRequest(),
				refreshTokenAccessor.GetFromRequest());

			_objectParser = objectParser;
			_responseHelper = responseHelper;
		}

        [HttpPost("{botId:long}/search-message")]
		public async Task<IActionResult> GetMessagePageAsync(
			long botId,
			MessagePageSearchDto searchDto)
		{
			ServiceResponse<object> response = await _requestProcessor.ProcessAsync(
				_objectParser, HttpMethod.Post, $"{botId}/search-message",
				dtoBody: searchDto, authentication: _authentication);

			return _responseHelper.HandleStatusCodes(
				response, StatusCodes.Status400BadRequest);
		}

		[HttpGet("{botId:long}")]
		public async Task<IActionResult> GetBotEventsAsync(
			long botId,
			[FromQuery] int startIndex, [FromQuery] int count,
			[FromQuery] BotEventsFilter filter)
		{
			IEnumerable<KeyValuePair<string, string>> query = typeof(BotEventsFilter).GetProperties()
				.Select(prop => new KeyValuePair<string, string?>(prop.Name, prop.GetValue(filter)?.ToString()))
				.Where(pair => pair.Value != null)!;

			query = query.Concat(new KeyValuePair<string, string>[]
			{
				new KeyValuePair<string, string>("startIndex", startIndex.ToString()),
				new KeyValuePair<string, string>("count", count.ToString()),
			});

			Dictionary<string, string> queryDictionary = query
				.ToDictionary(pair => pair.Key, pair => pair.Value);

			ServiceResponse<object> response = await _requestProcessor.ProcessAsync(
				_objectParser, HttpMethod.Get, botId.ToString(),
				queryParameters: queryDictionary, authentication: _authentication);

			return _responseHelper.HandleStatusCodes(
				response, StatusCodes.Status400BadRequest);
		}
	}
}
