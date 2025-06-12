using System;
using System.Net.Http;

namespace CargoWise.eServices.Authentication.WebService
{
	public struct ResultCache
	{
		internal HttpResponseMessage Result { get; set; }
		internal DateTime ExpirationTime { get; set; }
		internal string SystemId { get; set; }
		internal string EnterpriseCode { get; set; }
		internal string ServerCode { get; set; }
		internal string Password { get; set; }
		internal DateTime LastCheck { get; set; }
	}
}
