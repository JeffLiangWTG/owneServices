using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.Business.Testing
{
	public class CusStatementLineChargeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTaxOrFeeCodeList()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateTaxOrFee("ABS", 0.1, Core.Constants.CountryCodes.Turkey, 0.1, 0.1, "OTH", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
				helper.CreateTaxOrFee("GMS", 0.2, Core.Constants.CountryCodes.Turkey, 0.2, 0.2, "OTH", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
				helper.CreateTaxOrFee("TR3", 0.3, Core.Constants.CountryCodes.Turkey, 0.3, 0.3, "VAT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
				helper.CreateTaxOrFee("89", 0.4, Core.Constants.CountryCodes.Turkey, 0.4, 0.4, "OTH", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
				helper.CreateTaxOrFee("OBS", 0.5, Core.Constants.CountryCodes.Turkey, 0.5, 0.5, "VAT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

				Factory.Save();

				var statementLine = Factory.New<CusStatementLine>();
				statementLine.B3_EntryType = StatementLineEntryTypeList.Codes.MAN;
				var charge = Factory.New<CusStatementLineCharge>();
				statementLine.Charges.Add(charge);
				var cusStatementLineChargeLookups = charge.Lookups;

				AssertEquals("ABS, GMS", cusStatementLineChargeLookups.TaxOrFeeCodeList.CodesAsString);

				statementLine.B3_EntryType = StatementLineEntryTypeList.Codes.ETR;
				var charge2 = Factory.New<CusStatementLineCharge>();
				statementLine.Charges.Add(charge2);
				var cusStatementLineChargeLookups2 = charge2.Lookups;

				AssertEquals("89", cusStatementLineChargeLookups2.TaxOrFeeCodeList.CodesAsString);
			}
		}
	}
}
