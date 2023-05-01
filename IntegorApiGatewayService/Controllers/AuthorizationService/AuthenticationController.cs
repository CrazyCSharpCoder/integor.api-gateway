using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using IntegorSharedResponseDecorators.Authorization.Attributes;

using IntegorPublicDto.Authorization.Users;
using IntegorPublicDto.Authorization.Users.Input;

using IntegorServicesInteraction;
using IntegorServicesInteraction.Authorization;

using IntegorAspHelpers.MicroservicesInteraction;

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
			AttachTokensToResponse(response.AuthenticationResult);

			return _responseToResult.ToActionResult(response);
		}

		[HttpPost("login")]
		[DecorateUserResponse]
		public async Task<IActionResult> LoginAsync([FromBody] LoginUserDto loginDto)
		{
			ServiceResponse<UserAccountInfoDto> response = await _authApi.LoginAsync(loginDto);
			AttachTokensToResponse(response.AuthenticationResult);

			return _responseToResult.ToActionResult(response);
		}

		private void AttachTokensToResponse(UserAuthentication authentication)
		{
			if (authentication.AccessToken != null)
				_accessTokenAccessor.AttachToResponse(authentication.AccessToken);

			if (authentication.RefreshToken != null)
				_refreshTokenAccessor.AttachToResponse(authentication.RefreshToken);
		}
	}
}
