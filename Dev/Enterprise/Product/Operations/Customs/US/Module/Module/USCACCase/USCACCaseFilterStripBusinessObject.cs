using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Module
{
	sealed class USCACCaseFilterStripBusinessObject : FilterStripBusinessObject
	{
		public USCACCaseFilterStripBusinessObject()
		{
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			result.AddTextFilter("Case Number", USCACCaseSchema.U5_CaseNumber);
			result.AddTextFilter("Case Status Code", USCACCaseSchema.U5_CaseStatus);
			result.AddDateFilter("Case Status Date", USCACCaseSchema.U5_CaseStatusDate);
			result.AddTextFilter("Description", USCACCaseSchema.U5_ShortDescription);
			result.AddTextFilter("Official Name", USCACCaseSchema.U5_OfficialName);
			result.AddTextFilter("Contact Name", USCACCaseSchema.U5_ContactName);
			result.AddTextFilter("Contact Office", USCACCaseSchema.U5_ContactOffice);
			result.AddTextFilter("Manufacturer ID", USCACCaseSchema.U5_ManufacturerMID);
			result.AddTextFilter("Manufacturer Name", USCACCaseSchema.U5_ManufacturerName);
			result.AddTextFilter("Foreign Exporter ID", USCACCaseSchema.U5_ForeignExporterMID);
			result.AddTextFilter("Foreign Exporter Name", USCACCaseSchema.U5_ForeignExporterName);
			result.AddTextFilter("Related Case Number", USCACCaseSchema.U5_RelatedCaseNumber);
			result.AddFilter(new USCountryCodeModuleFilter(USCACCaseSchema.U5_ISOCountryCode, Factory));

			var tariff = new TariffProvTariffModuleFilter("Tariff Number", GetTariffQuery);
			tariff.MaxLength = USCACCaseTariffSchema.U9_TariffNumber.MaxLength + 2;
			result.AddCustomFilter(tariff);
			result.AddFlagsFilter("Case Status", new string[] { "Exclude Inactive" }, new GetFlagsQuery[] { GetExcludeInactiveCaseStatusQuery });
			return result;
		}

		ZQuery GetExcludeInactiveCaseStatusQuery(ZBool value)
		{
			var result = new ZQuery();
			if (value)
			{
				result.AddToFilter(USCACCaseSchema.U5_CaseStatus, ACCaseStatusList.Codes.AC);
			}
			return result;
		}

		ZQuery GetTariffQuery(ZString tariff, ZString provTariff)
		{
			tariff = tariff.KeepNumericCharacters();

			var result = new ZDBOnlyQuery(typeof(USCACCase));
			var subQuery = new ZDBOnlySubQuery(typeof(USCACCaseTariff), USCACCaseTariffSchema.U9_CaseNumber);
			subQuery.AddToFilter(USCACCaseTariffSchema.U9_TariffNumber, SQLComparisonOperator.StartsWith, tariff);

			if (tariff.Length >= 4)
			{
				var innerQuery = new ZQuery();
				var tariffMatches = new List<string>();
				for (var i = 4; i <= tariff.Length; i++)
				{
					tariffMatches.Add(tariff.Left(i));
				}
				innerQuery.AddToFilter(JoinCondition.Or, USCACCaseTariffSchema.U9_TariffNumber, SQLComparisonOperator.Equal, tariffMatches);

				subQuery.AddToFilter(innerQuery, JoinCondition.Or);
			}

			result.AddSubQuery(USCACCaseSchema.U5_CaseNumber, subQuery, JoinCondition.And);
			return result;
		}
	}
}
