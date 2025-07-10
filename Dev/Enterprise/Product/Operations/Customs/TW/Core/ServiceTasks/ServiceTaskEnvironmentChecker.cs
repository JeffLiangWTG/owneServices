using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.TW.ServiceTasks
{
	public static class ServiceTaskEnvironmentChecker
	{
		public static string CheckTWCompanyHasCredentials()
		{
			return TWCompanyHasCredentials() ? ZString.Empty : ResString.GetMultilingualString("30BB882B-BE24-4115-A7B9-6CC061947362", "There is no credential configuration on Taiwan companies.");
		}

		static bool TWCompanyHasCredentials()
		{
			if (!companyHasCredentials.HasValue)
			{
				var factory = new BusinessObjectFactory();
				var companyPKs = GlbCompany.GetActiveCompanies(CountryCodes.Taiwan, factory).Select(x => x.PK);
				var result = false;
				if (companyPKs.Any())
				{
					var query = new ZQuery(GlbExternalPasswordSchema.GP_GC, companyPKs);
					query.AddToFilter(GlbExternalPasswordSchema.GP_PasswordStatus, PasswordStatusList.Codes.Valid);
					query.AddToFilter(GlbExternalPasswordSchema.GP_PasswordType, new[] { PasswordTypesList.Codes.TVA, PasswordTypesList.Codes.TVF, PasswordTypesList.Codes.NXM, PasswordTypesList.Codes.UVC });
					result = factory.Exists(typeof(GlbExternalPassword), query);
				}
				return result;
			}
			return companyHasCredentials.Value;
		}

		[ThreadStatic]
		static bool? companyHasCredentials;
	}
}
