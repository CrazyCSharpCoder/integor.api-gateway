using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using ExtensibleRefreshJwtAuthentication.Access;
using ExtensibleRefreshJwtAuthentication.Refresh;

using IntegorSharedResponseDecorators.Authorization.Attributes;

using IntegorPublicDto.Authorization.Users;

using IntegorServicesInteraction;
using IntegorServicesInteraction.Authorization;

using IntegorAspHelpers.MicroservicesInteraction;

using IntegorLogicShared.IntegorServices.Shared.ResponseValidation;
using Microsoft.AspNetCore.Authorization;
using IntegorAspHelpers.MicroservicesInteraction.Authorization;

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
		public IActionResult GetMe()
		{
			return Ok(_userCaching.GetCachedUser());
		}
	}
}
