using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DataRegistry.Business
{
	public static class DeclarationLockConfigHelper
	{
		public static string GetCompanyCountry(this RegistryBusinessObjectTemplate tpl)
		{
			var companyPK = tpl.CurrentFallbackLevel?.CompanyPK(false) ?? Guid.Empty;
			var company = companyPK == EnvProxy.Instance.CurrentCompany.PK
									? EnvProxy.Instance.CurrentCompany
									: (ICompany)((IFactoryProvider)Env.CurrentCompany).Factory.Load<IGlbCompany>(companyPK);
			return company?.Country?.Code ?? string.Empty;
		}
	}
}
