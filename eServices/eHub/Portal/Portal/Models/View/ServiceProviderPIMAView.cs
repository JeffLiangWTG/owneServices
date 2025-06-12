using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CargoWise.eHub.Portal.Models.View
{
	public class ServiceProviderPIMAView
	{
		public Guid ServiceProviderId { get; set; }
		public string ServiceProviderName { get; set; }
        public string IATA { get; set; }
        public object IATAHtmlAttributes { get; set; }
		public string PIMA { get; set; }
		public string Password { get; set; }
        public string Button { get; set; }
        public bool IsDefault { get; set; }
	}
}