using System.Linq;

using Microsoft.AspNetCore.Mvc;

using IntegorAspHelpers.MicroservicesInteraction;
using IntegorServicesInteraction;
using IntegorErrorsHandling;

namespace IntegorApiGatewayService.Helpers
{
	public class IntegorServicesResponseStatusCodesHelper
	{
		private ServiceResponseToActionResultHelper _responseToResult;
		private IResponseErrorsObjectCompiler _errorsCompiler;

		public IntegorServicesResponseStatusCodesHelper(
			ServiceResponseToActionResultHelper responseToResult,
			IResponseErrorsObjectCompiler errorsCompiler)
        {
			_responseToResult = responseToResult;
			_errorsCompiler = errorsCompiler;
        }

        public IActionResult HandleStatusCodes<T>(
			ServiceResponse<T> response, params int[] statusCodes)
			where T : class
		{
			if (response.Ok || !statusCodes.Contains(response.StatusCode))
				return _responseToResult.ToActionResult(response);
			
			object errors = _errorsCompiler.CompileResponse(response.Errors!);
			return new ObjectResult(errors) { StatusCode = response.StatusCode };
		}
	}
}
