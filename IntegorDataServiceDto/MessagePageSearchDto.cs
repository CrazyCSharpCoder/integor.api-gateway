using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegorDataServiceDto
{
	public class MessagePageSearchDto
	{
		public long MessageId { get; set; }
		public long ChatId { get; set; }

		public int PageSize { get; set; }

		public BotEventsFilter Filter { get; set; } = null!;
	}
}
