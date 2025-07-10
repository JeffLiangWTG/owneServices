using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(ConsolidateChargeCodeFilterBusinessObject))]
	sealed class ConsolidateChargeCodeFilterBusinessObjectTest : AccChargeCodeFilterBusinessObjectTest
	{
		string CompaniesForChargeCodes(IEnumerable chargeCodes)
		{
			var companyCodes = new HashSet<string>();
			foreach (AccChargeCode chargeCode in chargeCodes)
			{
				companyCodes.Add(chargeCode.Company.GC_Code);
			}
			return string.Join(",", companyCodes.OrderBy(x => x).ToArray());
		}

		public void TestCompanyFilters()
		{
			var filterStripBizO = GetNewFilterStripBusinessObject();

			AccChargeCode normalChargeCode, normalChargeCodeLinked, globalChargeCode;
			AccChargeCodeTest.SetupGlobalChargeCodeScenario(
				Factory, out normalChargeCode, out normalChargeCodeLinked, out globalChargeCode);

			var filter = (ModuleTextFilter)filterStripBizO["CompanyCode"];
			filter.IsActive = true;
			var chargeCodes = new AccChargeCodesForConsolidationCollection(Factory);

			filter.Property = "";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			chargeCodes.AdditionalFilter = filterStripBizO.Filter;
			AssertEquals("Should show all charge codes", "DEM,EDI,SIN", CompaniesForChargeCodes(chargeCodes));

			filter.Property = "DEM";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			chargeCodes.AdditionalFilter = filterStripBizO.Filter;
			AssertEquals("Should show only demo charge codes", "DEM", CompaniesForChargeCodes(chargeCodes));

			filter.Property = "XXX";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			chargeCodes.AdditionalFilter = filterStripBizO.Filter;
			AssertEquals("No company called XXX, so no charge codes", 0, chargeCodes.Count);

			filter.IsActive = false;

			filter = (ModuleTextFilter)filterStripBizO["CompanyName"];
			filter.IsActive = true;
			chargeCodes.AdditionalFilter = filterStripBizO.Filter;
			chargeCodes = new AccChargeCodesForConsolidationCollection(Factory);

			filter.Property = "Demo Com";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			chargeCodes.AdditionalFilter = filterStripBizO.Filter;
			AssertEquals("Should show only demo charge codes", "DEM", CompaniesForChargeCodes(chargeCodes));

			filter.Property = "Demo con";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			chargeCodes.AdditionalFilter = filterStripBizO.Filter;
			AssertEquals("No company starts with Demo Con, so no charge codes", 0, chargeCodes.Count);
		}

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new ConsolidateChargeCodeFilterBusinessObject();
		}

		#endregion
	}
}
