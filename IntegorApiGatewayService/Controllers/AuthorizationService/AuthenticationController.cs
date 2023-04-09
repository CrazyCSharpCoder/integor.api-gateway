using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using IntegorSharedResponseDecorators.Authorization.Attributes;

using IntegorPublicDto.Authorization.Users;
using IntegorPublicDto.Authorization.Users.Input;

using IntegorServicesInteraction;
using IntegorServicesInteraction.Authorization;

namespace IntegorApiGatewayService.Controllers.AuthorizationService
{
    [ApiController]
	[Route("auth")]
    public class AuthenticationController : ControllerBase
    {
		private IAuthorizationServiceAuthApi _authApi;

		public AuthenticationController(IAuthorizationServiceAuthApi authApi)
        {
			_authApi = authApi;
        }

		[HttpPost("register")]
		[DecorateUserResponse]
        public async Task<IActionResult> RegisterAsync(RegisterUserDto registerDto)
        {
			ServiceResponse<UserAccountInfoDto> response;

			try
			{
				response = await _authApi.RegisterAsync(registerDto);
			}
			catch
			{
				// TODO handle error
				throw;
			}

			if (!response.Ok)
				// TODO handle exception when respnonse is not ok via SharedLogic
				throw new Exception("Not Ok");

			if (response.Value == null)
				// TODO add special type for microservice exception
				throw new Exception("Ok but value is null");

			return Ok(response.Value);
        }

		[HttpPost("login")]
		[DecorateUserResponse]
		public async Task<IActionResult> LoginAsync(LoginUserDto loginDto)
		{
			ServiceResponse<UserAccountInfoDto> response;

			try
			{
				response = await _authApi.LoginAsync(loginDto);
			}
			catch
			{
				// TODO handle error
				throw;
			}

			if (!response.Ok)
				// TODO handle exception when respnonse is not ok via SharedLogic
				throw new Exception("Not Ok");

			if (response.Value == null)
				// TODO add special type for microservice exception
				throw new Exception("Ok but value is null");

			return Ok(response.Value);
		}

		// TODO add logout
	}
}
