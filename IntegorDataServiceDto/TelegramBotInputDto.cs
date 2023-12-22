using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegorDataServiceDto
{
	public class TelegramBotInputDto
	{
		public string Title { get; set; } = null!;
		public string Token { get; set; } = null!;

		public string Description { get; set; } = null!;
	}
}
