using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CargoWise.eHub.DataAccess.Models.CodeMapsTesting
{
	public class eHubUNLOCO
	{
		public string UNLOCOCode { get; set; }
		public string IATACode { get; set; }
		public Guid StateRef { get; set; }

		public eHubUNLOCO()
		{
		}
	}
}
