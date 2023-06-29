using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

using IntegorResponseDecoration.Parsing;

namespace IntegorApiGatewayShared.ObjectParsers
{
	public class DefaultObjectParser : IDecoratedObjectParser<object, JsonElement>
	{
		public DecoratedObjectParsingResult<object> ParseDecorated(JsonElement decoratedObject)
		{
			bool hasErrors = false;
			JsonElement errors;

			try
			{
				hasErrors = decoratedObject.TryGetProperty("errors", out errors);
			}
			catch { /*Ignore*/ }

			if (hasErrors)
				return new DecoratedObjectParsingResult<object>(false);

			return new DecoratedObjectParsingResult<object>(decoratedObject);
		}
	}
}
