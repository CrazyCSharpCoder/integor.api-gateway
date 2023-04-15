using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using IntegorSharedResponseDecorators.Authorization.Attributes;

using IntegorAspHelpers.MicroservicesInteraction.Authorization;
using IntegorAspHelpers.MicroservicesInteraction.Authorization.Filters;

namespace IntegorApiGatewayService.Controllers.AuthorizationService
{
	[ApiController]
	// TODO concat from AuthenticationController route
	[Route("auth/me")]
	public class ProfileController : ControllerBase
	{
		private IUserCachingService _userCaching;

		public ProfileController(IUserCachingService userCaching)
		{
			_userCaching = userCaching;
		}

		[HttpGet]
		[Authorize]
		[DecorateUserResponse]
		[ValidateUserAuthenticationFilter]
		public IActionResult GetMe()
		{
			return Ok(_userCaching.GetCachedUser());
		}
	}
}
