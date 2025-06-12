using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CargoWise.eHub.Portal.Models
{
	[Serializable]
	public class ClientSetView
	{
		public Guid? SenderId { get; set; }
		public Guid? RecipientId { get; set; }
		public string Name { get; set; }
		public string Code { get; set; }
	}
}