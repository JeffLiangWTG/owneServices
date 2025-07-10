using System;
using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Services.ServiceHost
{
	public interface IDeviceLicenceService
	{
		IEnumerable<CodeDescriptionPair> GetEnterpriseCodeList();

		IEnumerable<CodeDescriptionPair> GetServerCodeList(string enterpriseCode);

		Guid GetCustomer(string enterpriseCode, string serverCode);
	}
}
