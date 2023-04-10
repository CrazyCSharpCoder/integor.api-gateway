using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using IntegorSharedResponseDecorators.Authorization.Attributes;

using IntegorPublicDto.Authorization.Users;
using IntegorPublicDto.Authorization.Users.Input;

using IntegorServicesInteraction;
using IntegorServicesInteraction.Authorization;

using IntegorAspHelpers.MicroservicesInteraction;

namespace IntegorApiGatewayService.Controllers.AuthorizationService
{
    [ApiController]
	[Route("auth")]
    public class AuthenticationController : ControllerBase
    {
		private IAuthorizationServiceAuthApi _authApi;
		private ServiceResponseToActionResultHelper _responseToResult;

		public AuthenticationController(
			IAuthorizationServiceAuthApi authApi,
			ServiceResponseToActionResultHelper responseToResult)
        {
			_authApi = authApi;
			_responseToResult = responseToResult;
        }

		[HttpPost("register")]
		[DecorateUserResponse]
        public async Task<IActionResult> RegisterAsync(RegisterUserDto registerDto)
        {
			ServiceResponse<UserAccountInfoDto> response = await _authApi.RegisterAsync(registerDto);
			return _responseToResult.ToActionResult(response);
		}

		[HttpPost("login")]
		[DecorateUserResponse]
		public async Task<IActionResult> LoginAsync(LoginUserDto loginDto)
		{
			ServiceResponse<UserAccountInfoDto> response = await _authApi.LoginAsync(loginDto);
			return _responseToResult.ToActionResult(response);
		}

		// TODO add logout
	}
}
