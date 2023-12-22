using System.Collections.Generic;
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

using IntegorServicesInteractionHelpers;

using IntegorAspHelpers.MicroservicesInteraction;

using ExternalServicesConfiguration;

using IntegorDataServiceDto;
using IntegorServicesInteraction;

using IntegorApiGatewayShared.ObjectParsers;

namespace IntegorApiGatewayService.Controllers.DataService
{
	using Helpers;

	[Authorize]
	[ApiController]
	[Route("bots")]
	public class BotsController : ControllerBase
	{
		private AuthenticatedJsonServicesRequestProcessor<DataServiceConfiguration> _requestProcessor;
		private UserAuthentication _authentication;

		private DefaultObjectParser _objectParser;
		private IntegorServicesResponseStatusCodesHelper _responseHelper;

		public BotsController(
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
				sendAccessTokenAccessor, sendRefreshTokenAccessor, errorParser, dataServiceOptions, "bots");

			_authentication = new UserAuthentication(
				accessTokenAccessor.GetFromRequest(),
				refreshTokenAccessor.GetFromRequest());

			_objectParser = objectParser;
			_responseHelper = responseHelper;
        }

        [HttpPost]
		public async Task<IActionResult> RegisterBotAsync(TelegramBotInputDto botDto)
		{
			ServiceResponse<object> response = await _requestProcessor.ProcessAsync(
				_objectParser, HttpMethod.Post,
				dtoBody: botDto, authentication: _authentication);

			return _responseHelper.HandleStatusCodes(response, StatusCodes.Status400BadRequest);
		}

		[HttpPut("{botId:long}")]
		public async Task<IActionResult> UpdateBotAsync(long botId, TelegramBotInputDto botDto)
		{
			ServiceResponse<object> response = await _requestProcessor.ProcessAsync(
				_objectParser, HttpMethod.Put, botId.ToString(),
				dtoBody: botDto, authentication: _authentication);

			return _responseHelper.HandleStatusCodes(response, StatusCodes.Status400BadRequest);
		}

		[HttpGet("{botId:long}")]
		public async Task<IActionResult> GetBotByIdAsync(long botId)
		{
			ServiceResponse<object> response = await _requestProcessor.ProcessAsync(_objectParser,
				HttpMethod.Get, botId.ToString(), authentication: _authentication);

			return _responseHelper.HandleStatusCodes(response, StatusCodes.Status400BadRequest);
		}

		[HttpGet("my-bots")]
		public async Task<IActionResult> GetMyBotsAsync()
		{
			ServiceResponse<object> response = await _requestProcessor.ProcessAsync(_objectParser,
				HttpMethod.Get, "all", authentication: _authentication);

			return _responseHelper.HandleStatusCodes(response, StatusCodes.Status400BadRequest);
		}
	}
}
