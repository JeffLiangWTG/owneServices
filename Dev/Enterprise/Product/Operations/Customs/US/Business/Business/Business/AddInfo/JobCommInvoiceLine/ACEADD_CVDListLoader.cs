using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	class ACEADD_CVDListLoader
	{
		public ACEADD_CVDListLoader(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}
		readonly BusinessObjectFactory factory;

		public USCACCaseCollection GetCaseNumberList(ZString countryOfOrigin, ZString caseNo, ZString casePrefix, params ZString[] tariffNumbers)
		{
			var result = new USCACCaseCollection(factory);
			var searchCountryOfOrigin = countryOfOrigin;

			if (countryOfOrigin.StartsWith("X", StringComparison.OrdinalIgnoreCase))
			{
				searchCountryOfOrigin = Core.Constants.CountryCodes.Canada;
			}
			result.DefaultFilters(searchCountryOfOrigin, caseNo, casePrefix, tariffNumbers);
			result.AdditionalFilter = GetQuery(searchCountryOfOrigin, casePrefix, tariffNumbers);

			return result;
		}

		ZQuery GetQuery(ZString countryOfOrigin, ZString casePrefix, params ZString[] tariffNumbers)
		{
			var result = new ZQuery(USCACCaseSchema.U5_ISOCountryCode, countryOfOrigin);
			result.AddToFilter(USCACCaseSchema.U5_CaseNumber, SQLComparisonOperator.StartsWith, casePrefix);

			if (tariffNumbers.Length > 0)
			{
				result.AddToFilter(GetTariffQuery(tariffNumbers));
			}

			result.AddToFilter(GetEmptyTariffCases(), JoinCondition.Or);
			return result;
		}

		ZDBOnlyQuery GetTariffQuery(params ZString[] tariffNumbers)
		{
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(USCACCaseTariff), USCACCaseTariffSchema.U9_CaseNumber);
			List<string> tariffMatches = new List<string>();

			foreach (ZString oneTariff in tariffNumbers)
			{
				if (!oneTariff.IsEmpty)
				{
					ZString tariffNumber = oneTariff.KeepNumericCharacters();
					for (int i = 4; i <= tariffNumber.Length; i++)
					{
						tariffMatches.Add(tariffNumber.Left(i));
					}
				}
			}

			subQuery.AddToFilter(USCACCaseTariffSchema.U9_TariffNumber, tariffMatches);

			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(USCACCase));
			result.AddSubQuery(USCACCaseSchema.U5_CaseNumber, subQuery, JoinCondition.And);
			return result;
		}

		ZDBOnlyQuery GetEmptyTariffCases()
		{
			var subQuery = new ZDBOnlySubQuery(typeof(USCACCaseTariff), USCACCaseTariffSchema.U9_CaseNumber, true);

			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(USCACCase));
			result.AddSubQuery(USCACCaseSchema.U5_CaseNumber, subQuery, JoinCondition.And);
			return result;
		}
	}
}
