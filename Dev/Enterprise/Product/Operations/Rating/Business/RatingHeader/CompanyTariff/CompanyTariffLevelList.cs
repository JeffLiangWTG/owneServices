using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	public class CompanyTariffLevelList
	{
		public CompanyTariffLevelList(BusinessObjectFactory factory)
		{
			businessObjectFactory = factory;
		}

		public CodeDescriptionPairList CompanyTariffLevelOverrideList
		{
			get
			{
				if (companyTariffLevelOverrideList == null)
				{
					var companyTariffLevelOverrideList = new CodeDescriptionPairList();
					var currentCompany = GlbCompany.GetCurrentCompany(businessObjectFactory);
					var filter = new ZQuery();
					var subQuery = new ZQuery();

					subQuery.AddToFilter(RatingHeaderSchema.TH_GC, SQLComparisonOperator.Equal, GetCurrentCompanyPK(currentCompany));
					subQuery.AddToFilter(JoinCondition.Or, RatingHeaderSchema.TH_GC, SQLComparisonOperator.IsBlank, null);

					filter.AddToFilter(RatingHeaderSchema.TH_RateType, SQLComparisonOperator.Equal, "GLB");
					filter.AddToFilter(subQuery);
					var localCompanyTariff = businessObjectFactory.Load<RatingHeader>(filter);

					foreach (var tariff in localCompanyTariff)
					{
						if (!companyTariffLevelOverrideList.ContainsCode(tariff.TH_GlobalRateLevel))
						{
							var description = localCompanyTariff.Cast<ICompanyTariff>().Any(x => x.TH_GlobalRateLevel == tariff.TH_GlobalRateLevel && x.TH_GC != tariff.TH_GC) ? Res.GetString("651b7e49-2eac-4d49-9c1f-5d5685fbdee7", "Global or Company Tariff Level {0}", tariff.TH_GlobalRateLevel) : tariff.TH_GlobalRateDescriptionMultilingual;
							companyTariffLevelOverrideList.AddPair(tariff.TH_GlobalRateLevel.ToString(), description);
						}
					}

					companyTariffLevelOverrideList.Sort();

					return companyTariffLevelOverrideList;
				}
				return companyTariffLevelOverrideList;
			}
		}

		readonly CodeDescriptionPairList companyTariffLevelOverrideList;

		readonly BusinessObjectFactory businessObjectFactory;

		ZGuid GetCurrentCompanyPK(GlbCompany currentCompany)
		{
			if (currentCompany == null)
			{
				var envCompany = Env.CurrentCompany;
				if (envCompany != null)
				{
					currentCompany = businessObjectFactory.Load<GlbCompany>(envCompany.PK);
				}

				if (currentCompany == null) // CurrentCompany should not be null, if it is null we need to check what happens.
				{
					var message = string.Format(
(NoResString)@"Current environment company PK is {0}
Current environment branch PK is {1}
Current environment user context is {2}", // Error reporting
									 (Env.CurrentCompany != null ? Env.CurrentCompany.PK : Guid.Empty),
									 (Env.CurrentBranch != null ? Env.CurrentBranch.PK : Guid.Empty),
									 (Env.CurrentUserContext != null && Env.CurrentUserContext.User != null ? Env.CurrentUserContext.User.LoginName : ""));
					ErrorReporter.ReportOnce("6cf69168-74dd-4c41-8c00-2b1afc4fcd55", message);
					return Guid.Empty;
				}
			}

			return currentCompany.PK;
		}
	}
}
