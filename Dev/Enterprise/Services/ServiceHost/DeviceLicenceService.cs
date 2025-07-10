using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Services.ServiceHost
{
	[CodeAlive("Used through TypeDecider")]
	public class DeviceLicenceService : IDeviceLicenceService
	{
		public IEnumerable<CodeDescriptionPair> GetEnterpriseCodeList()
		{
			return Enumerable.Empty<CodeDescriptionPair>();
		}
		public IEnumerable<CodeDescriptionPair> GetServerCodeList(string enterpriseCode)
		{
			return Enumerable.Empty<CodeDescriptionPair>();
		}
		public Guid GetCustomer(string enterpriseCode, string serverCode)
		{
			return Guid.Empty;
		}

		public abstract class MyTypeDecider : TypeDecider
		{
			public static Type GetTypeForCreate()
			{
				return TypeDecider.GetClientTypeDeciderFromType(typeof(IDeviceLicenceService))?.GetTypeForNew() ?? typeof(DeviceLicenceService);
			}
		}
	}
}
