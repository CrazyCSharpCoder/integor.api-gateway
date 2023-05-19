using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using IntegorErrorsHandling;

using IntegorSharedResponseDecorators.Authorization.Attributes;

using IntegorPublicDto.Authorization.Users;
using IntegorPublicDto.Authorization.Users.Input;

using IntegorServicesInteraction;
using IntegorServicesInteraction.Authorization;

using IntegorAspHelpers.MicroservicesInteraction;
using IntegorAspHelpers.MicroservicesInteraction.Authorization;

using ExtensibleRefreshJwtAuthentication.Access;
using ExtensibleRefreshJwtAuthentication.Refresh;

namespace IntegorApiGatewayService.Controllers.AuthorizationService
{
    [ApiController]
	[Route("auth")]
    public class AuthenticationController : ControllerBase
    {
		private IAuthorizationServiceAuthApi _authApi;
		private ServiceResponseToActionResultHelper _responseToResult;

		private IOnServiceProcessingAccessTokenAccessor _accessTokenAccessor;
		private IOnServiceProcessingRefreshTokenAccessor _refreshTokenAccessor;

		public AuthenticationController(
			IAuthorizationServiceAuthApi authApi,
			ServiceResponseToActionResultHelper responseToResult,

			IOnServiceProcessingAccessTokenAccessor accessTokenAccessor,
			IOnServiceProcessingRefreshTokenAccessor refreshTokenAccessor)
        {
			_authApi = authApi;
			_responseToResult = responseToResult;

			_accessTokenAccessor = accessTokenAccessor;
			_refreshTokenAccessor = refreshTokenAccessor;
        }

		[HttpPost("register")]
		[DecorateUserResponse]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterUserDto registerDto)
        {
			ServiceResponse<UserAccountInfoDto> response = await _authApi.RegisterAsync(registerDto);
			CreateAuthenticationSaver().ApplyAuthentication(response.AuthenticationResult);

			return _responseToResult.ToActionResult(response);
		}

		[HttpPost("login")]
		[DecorateUserResponse]
		public async Task<IActionResult> LoginAsync([FromBody] LoginUserDto loginDto)
		{
			ServiceResponse<UserAccountInfoDto> response = await _authApi.LoginAsync(loginDto);
			CreateAuthenticationSaver().ApplyAuthentication(response.AuthenticationResult);

			return _responseToResult.ToActionResult(response);
		}

		[HttpPost("logout")]
		[DecorateUserResponse]
		public async Task<IActionResult> LogoutAsync(
			[FromServices] IResponseErrorsObjectCompiler errorsCompiler)
		{
			string? accessToken = _accessTokenAccessor.GetFromRequest();
			string? refreshToken = _refreshTokenAccessor.GetFromRequest();

			ServiceResponse<UserAccountInfoDto> response =
				await _authApi.LogoutAsync(accessToken, refreshToken);

			CreateAuthenticationSaver().ApplyAuthentication(response.AuthenticationResult);

			if (response.StatusCode == StatusCodes.Status401Unauthorized)
				return new ObjectResult(errorsCompiler.CompileResponse(response.Errors!))
				{
					StatusCode = StatusCodes.Status401Unauthorized
				};
			
			return _responseToResult.ToActionResult(response);
		}

		private UserAuthenticationResultSavingHelper CreateAuthenticationSaver()
			=> new UserAuthenticationResultSavingHelper(_accessTokenAccessor, _refreshTokenAccessor);
	}
}
