using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CargoWise.eHub.Portal.Models
{
	public class ClientFilterViewModel
	{
		public string ID { get; set; }
		public string Name { get; set; }
        public string AS2Code { get; set; }
		public bool IsAirline { get; set; }
		public string AirlineCode { get; set; }
		public string AirlinePrefix { get; set; }
	}
}
