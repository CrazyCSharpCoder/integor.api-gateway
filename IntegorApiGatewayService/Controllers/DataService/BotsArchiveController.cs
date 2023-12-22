using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http;

using Microsoft.Extensions.Options;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using IntegorErrorsHandling;
using IntegorResponseDecoration.Parsing;

using ExtensibleRefreshJwtAuthentication.Access;
using ExtensibleRefreshJwtAuthentication.Refresh;

using IntegorServicesInteraction;
using IntegorServicesInteractionHelpers;

using IntegorAspHelpers.MicroservicesInteraction;

using ExternalServicesConfiguration;

using IntegorApiGatewayShared.ObjectParsers;

namespace IntegorApiGatewayService.Controllers.DataService
{
	[Authorize]
	[ApiController]
	[Route("bots")]
	public class BotsArchiveController : ControllerBase
	{
		private AuthenticatedJsonServicesRequestProcessor<DataServiceConfiguration> _requestProcessor;
		private UserAuthentication _authentication;

		private DefaultObjectParser _objectParser;
		private ServiceResponseToActionResultHelper _responseToResult;

		public BotsArchiveController(
			IOnServiceProcessingAccessTokenAccessor accessTokenAccessor,
			IOnServiceProcessingRefreshTokenAccessor refreshTokenAccessor,

			ISendRequestAccessTokenAccessor sendAccessTokenAccessor,
			ISendRequestRefreshTokenAccessor sendRefreshTokenAccessor,
			IDecoratedObjectParser<IEnumerable<IResponseError>, JsonElement> errorParser,

			DefaultObjectParser objectParser,
			ServiceResponseToActionResultHelper responseToResult,

			IOptions<DataServiceConfiguration> dataServiceOptions)
		{
			_requestProcessor = new AuthenticatedJsonServicesRequestProcessor<DataServiceConfiguration>(
				sendAccessTokenAccessor, sendRefreshTokenAccessor, errorParser, dataServiceOptions, "bots");

			_authentication = new UserAuthentication(
				accessTokenAccessor.GetFromRequest(),
				refreshTokenAccessor.GetFromRequest());

			_objectParser = objectParser;
			_responseToResult = responseToResult;
		}

		[HttpPost("{botId:long}/archive")]
		public async Task<IActionResult> ArchiveBotAsync(long botId)
		{
			ServiceResponse<object> response = await _requestProcessor.ProcessAsync(
				_objectParser, HttpMethod.Post, $"{botId}/archive",
				authentication: _authentication);

			return _responseToResult.ToActionResult(response);
		}
	

		[HttpPost("{botId:long}/unarchive")]
		public async Task<IActionResult> UnarchiveBotAsync(long botId)
		{
			ServiceResponse<object> response = await _requestProcessor.ProcessAsync(
				_objectParser, HttpMethod.Post, $"{botId}/unarchive",
				authentication: _authentication);

			return _responseToResult.ToActionResult(response);
		}

		[HttpGet("archived")]
		public async Task<IActionResult> GetArchivedBotsAsync()
		{
			ServiceResponse<object> response = await _requestProcessor.ProcessAsync(
				_objectParser, HttpMethod.Get, "archived", authentication: _authentication);

			return _responseToResult.ToActionResult(response);
		}
	}
}
