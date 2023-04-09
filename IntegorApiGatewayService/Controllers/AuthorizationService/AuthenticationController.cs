using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using IntegorSharedResponseDecorators.Authorization.Attributes;

using IntegorPublicDto.Authorization.Users;
using IntegorPublicDto.Authorization.Users.Input;

using IntegorServicesInteraction;
using IntegorServicesInteraction.Exceptions;
using IntegorServicesInteraction.Authorization;

using IntegorAspHelpers.MicroservicesInteraction;

namespace IntegorApiGatewayService.Controllers.AuthorizationService
{
    [ApiController]
	[Route("auth")]
    public class AuthenticationController : ControllerBase
    {
		private IAuthorizationServiceAuthApi _authApi;
		private IServiceErrorsToActionResultTranslator _errorsToResult;

		public AuthenticationController(
			IAuthorizationServiceAuthApi authApi,
			IServiceErrorsToActionResultTranslator errorsToResult)
        {
			_authApi = authApi;
			_errorsToResult = errorsToResult;
        }

		[HttpPost("register")]
		[DecorateUserResponse]
        public async Task<IActionResult> RegisterAsync(RegisterUserDto registerDto)
        {
			ServiceResponse<UserAccountInfoDto> response = await _authApi.RegisterAsync(registerDto);

			if (!response.Ok)
			{
				if (response.Errors == null)
					throw new UnexpectedServiceResponseException("Response does not contain errors when it must");

				return _errorsToResult.ErrorsToActionResult(response.Errors);
			}

			// TODO solve problem of repetitive code in both methods
			if (response.Value == null)
				throw new UnexpectedServiceResponseException("Response does not contain the user when it must");

			return Ok(response.Value);
        }

		[HttpPost("login")]
		[DecorateUserResponse]
		public async Task<IActionResult> LoginAsync(LoginUserDto loginDto)
		{
			ServiceResponse<UserAccountInfoDto> response = await _authApi.LoginAsync(loginDto);

			if (!response.Ok)
			{
				if (response.Errors == null)
					throw new UnexpectedServiceResponseException("Response does not contain errors when it must");

				return _errorsToResult.ErrorsToActionResult(response.Errors);
			}


			if (response.Value == null)
				throw new UnexpectedServiceResponseException("Response does not contain the user when it must");

			return Ok(response.Value);
		}

		// TODO add logout
	}
}
