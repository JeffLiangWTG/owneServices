using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Query.Wrapper;

namespace CargoWise.RefDbRepo.Common.Web.Auth
{
	public interface IAuthorizationHelper
	{
		bool IsAuthorized<T>(T data, string user);
		bool IsAuthorized<T>(string user);
		IEnumerable<T> InitAuthorizations<T>(IEnumerable<T> data, string user);
		IEnumerable<T> InitAuthorizationsForSelectExpandWrapper<T>(IEnumerable<T> data, string user) where T : ISelectExpandWrapper;
		IEnumerable<T> FilterAuthorizedData<T>(IEnumerable<T> data, string user);
	}
}
