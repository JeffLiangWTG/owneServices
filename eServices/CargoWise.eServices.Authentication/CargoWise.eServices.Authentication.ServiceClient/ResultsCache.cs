using System;

namespace CargoWise.eServices.Authentication.ServiceClient
{
	public class ResultCache
	{
		internal bool Result { get; set; }
		internal DateTime ExpirationTime { get; set; }
		internal string SystemId { get; set; }
		internal string EnterpriseCode { get; set; }
		internal string ServerCode { get; set; }
		internal string Password { get; set; }
		internal DateTime LastCheck { get; set; }
	}
}
