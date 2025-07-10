using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class AccTaxRateFilterControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestAccTaxRateFilterControl_CheckTaxIDGridColumnsWhenAuxiliaryRateGridColumnsAreVisible()
		{
			using (var control = new AccTaxRateFilterControl(new AccTaxRateCollection(Factory), new AccTaxRateFilterBusinessObject(), true))
			{
				AssertEquals("Tax ID Grid Columns", 14, control.Grid.ColumnStyles.Count);
				AssertEquals("SystemCreateUser", "AT_SystemCreateUser (ZTextBoxColumnStyleInfo)", control.Grid.ColumnStyles[0].ToString());
				AssertEquals("SystemCreateTimeUtc", "AT_SystemCreateTimeUtc (ZDateEditColumnStyleInfo)", control.Grid.ColumnStyles[1].ToString());
				AssertEquals("SystemLastEditUser", "AT_SystemLastEditUser (ZTextBoxColumnStyleInfo)", control.Grid.ColumnStyles[2].ToString());
				AssertEquals("SystemLastEditTimeUtc", "AT_SystemLastEditTimeUtc (ZDateEditColumnStyleInfo)", control.Grid.ColumnStyles[3].ToString());
				AssertEquals("Code", "AT_Code (ZTextBoxColumnStyleInfo)", control.Grid.ColumnStyles[4].ToString());
				AssertEquals("Description", "AT_Description (ZTextBoxColumnStyleInfo)", control.Grid.ColumnStyles[5].ToString());
				AssertEquals("Tax Type", "AT_Type (ZTextBoxColumnStyleInfo)", control.Grid.ColumnStyles[6].ToString());
				AssertEquals("Rate", "RateForTodayForUIBinding (ZCalcEditColumnStyleInfo)", control.Grid.ColumnStyles[7].ToString());
				AssertEquals("Auxiliary Type", "AT_ExtraTaxRateType (ZTextBoxColumnStyleInfo)", control.Grid.ColumnStyles[8].ToString());
				AssertEquals("Extra Tax Rate", "ExtraRateForTodayForUIBinding (ZCalcEditColumnStyleInfo)", control.Grid.ColumnStyles[9].ToString());
				AssertEquals("Tax System", "AT_TaxSystemCode_ForDisplay (ZTextBoxColumnStyleInfo)", control.Grid.ColumnStyles[10].ToString());
				AssertEquals("Country", "AT_RN_NKCountry (ZTextBoxColumnStyleInfo)", control.Grid.ColumnStyles[11].ToString());
				AssertEquals("Tax Msg.", "AT_A9_DefaultVatClass (ZGuidFindBoxColumnStyleInfo)", control.Grid.ColumnStyles[12].ToString());
				AssertEquals("Is Active", "AT_IsActive (ZCheckBoxColumnStyleInfo)", control.Grid.ColumnStyles[13].ToString());
			}
		}

		[RequiresSTA]
		public void TestAccTaxRateFilterControl_CheckTaxIDGridColumnsWhenAuxiliaryRateGridColumnsAreInvisible()
		{
			using (var control = new AccTaxRateFilterControl(new AccTaxRateCollection(Factory), new AccTaxRateFilterBusinessObject(), false))
			{
				AssertEquals("Tax ID Grid Columns", 12, control.Grid.ColumnStyles.Count);
				AssertEquals("SystemCreateUser", "AT_SystemCreateUser (ZTextBoxColumnStyleInfo)", control.Grid.ColumnStyles[0].ToString());
				AssertEquals("SystemCreateTimeUtc", "AT_SystemCreateTimeUtc (ZDateEditColumnStyleInfo)", control.Grid.ColumnStyles[1].ToString());
				AssertEquals("SystemLastEditUser", "AT_SystemLastEditUser (ZTextBoxColumnStyleInfo)", control.Grid.ColumnStyles[2].ToString());
				AssertEquals("SystemLastEditTimeUtc", "AT_SystemLastEditTimeUtc (ZDateEditColumnStyleInfo)", control.Grid.ColumnStyles[3].ToString());
				AssertEquals("Code", "AT_Code (ZTextBoxColumnStyleInfo)", control.Grid.ColumnStyles[4].ToString());
				AssertEquals("Description", "AT_Description (ZTextBoxColumnStyleInfo)", control.Grid.ColumnStyles[5].ToString());
				AssertEquals("Tax Type", "AT_Type (ZTextBoxColumnStyleInfo)", control.Grid.ColumnStyles[6].ToString());
				AssertEquals("Rate", "RateForTodayForUIBinding (ZCalcEditColumnStyleInfo)", control.Grid.ColumnStyles[7].ToString());
				AssertEquals("Tax System", "AT_TaxSystemCode_ForDisplay (ZTextBoxColumnStyleInfo)", control.Grid.ColumnStyles[8].ToString());
				AssertEquals("Country", "AT_RN_NKCountry (ZTextBoxColumnStyleInfo)", control.Grid.ColumnStyles[9].ToString());
				AssertEquals("Tax Msg.", "AT_A9_DefaultVatClass (ZGuidFindBoxColumnStyleInfo)", control.Grid.ColumnStyles[10].ToString());
				AssertEquals("Is Active", "AT_IsActive (ZCheckBoxColumnStyleInfo)", control.Grid.ColumnStyles[11].ToString());
			}
		}

		[RequiresSTA]
		public void TestAccTaxRateFilterControl_WithPostingGroup()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				using (var control = new AccTaxRateFilterControl(new AccTaxRateCollection(Factory), new AccTaxRateFilterBusinessObject(), true))
				{
					AssertEquals(false, AccTaxRate.IsPostingGroupsEnabled(GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
					AssertNull(control.Grid.GetColumnStyle("AT_PostingGroupId"));
				}
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("VN"))
			{
				using (var control = new AccTaxRateFilterControl(new AccTaxRateCollection(Factory), new AccTaxRateFilterBusinessObject(), true))
				{
					AssertEquals(true, AccTaxRate.IsPostingGroupsEnabled(GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
					AssertNotNull(control.Grid.GetColumnStyle("AT_PostingGroupId"));
				}
			}
		}
	}
}
