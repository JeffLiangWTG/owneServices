using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Registry.Business.ComplianceReportConfigurationLookups;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ComplianceReportConfiguration))]
	class ComplianceReportConfigurationTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestZDecimalsHaveCorrectDecimalPlacesComplianceReportConfiguration()
		{
			BizObj.Country = Core.Constants.CountryCodes.Australia;

			var countryList = new List<string>
			{
				nameof(BizObj.ExTaxAmountThreshold),
				nameof(BizObj.TaxAmountThreshold)
			};

			var tester = new DecimalPlacesAttributeTester(BizObj);
			tester.CheckSetter(countryList, nameof(BizObj.CountryCurrencyDecimalPlaces));

			BizObj.Country = Core.Constants.CountryCodes.Bahrain;
			AssertEquals(string.Format("{0} should match local currency", nameof(BizObj.CountryCurrencyDecimalPlaces)), 3, BizObj.CountryCurrencyDecimalPlaces);
			BizObj.Country = Core.Constants.CountryCodes.Australia;
			AssertEquals(string.Format("{0} should match local currency", nameof(BizObj.CountryCurrencyDecimalPlaces)), 2, BizObj.CountryCurrencyDecimalPlaces);
		}

		public void TestValidateCountry()
		{
			Assert("Default value is empty", BizObj.Country.IsEmpty);
			BizObj.ValidateCountry();
			AssertHasErrors(BizObj.CountryInfo);

			BizObj.Country = "AU";
			AssertNoErrors(BizObj.CountryInfo);

			BizObj.Country = "ZZ";
			AssertHasErrors(BizObj.CountryInfo);
		}

		public void TestValidateReportCode()
		{
			Assert("Default value is empty", BizObj.ReportCode.IsEmpty);
			BizObj.ValidateReportCode();
			AssertHasErrors(BizObj.ReportCodeInfo);

			BizObj.ReportCode = "ZZZ";
			AssertNoErrors("Any value will do", BizObj.ReportCodeInfo);

			var configurations = new ComplianceReportConfigurationCollection(Factory);
			BizObj.Country = "AU";
			configurations.Add(BizObj);

			var otherConfig = configurations.AddNew();
			otherConfig.Country = "AU";
			otherConfig.ReportCode = "ZZZ";
			AssertHasError(otherConfig.ReportCodeInfo, "The same Report Code already exists for the 'AU' country/region.");

			otherConfig.ReportCode = "XYZ";
			AssertNoErrors("Codes are unique per Country", BizObj.ReportCodeInfo);
		}

		public void TestValidateReportTitle()
		{
			Assert("Default value is empty", BizObj.ReportTitle.IsEmpty);
			AssertNoErrors("Is not required", BizObj.ReportTitleInfo);

			BizObj.ReportTitle = "XyZ";
			AssertNoErrors(BizObj.ReportTitleInfo);
		}

		public void TestValidateReportBaseTablePrefix()
		{
			FallbackLevel fallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			BizObj.CurrentFallbackLevel = fallbackLevel;
			AssertEquals("GenerateJournalEntriesStartDate not be set", DateTime.MinValue, ObjectFactory.Get<IAccounting>().GetGenerateJournalEntriesStartDate(GlbCompany.CurrentCompany.PK.ToGuid()));

			Assert("Default value is empty", BizObj.ReportBaseTablePrefix.IsEmpty);
			BizObj.ValidateReportBaseTablePrefix();
			AssertHasErrors("Is required", BizObj.ReportBaseTablePrefixInfo);

			BizObj.ReportBaseTablePrefix = "AH";
			AssertNoErrors(BizObj.ReportBaseTablePrefixInfo);

			BizObj.ReportBaseTablePrefix = "$$";
			AssertHasErrors("Invalid value", BizObj.ReportBaseTablePrefixInfo);

			BizObj.ReportBaseTablePrefix = "**";
			AssertNoErrors("This is for all transactions from different tables", BizObj.ReportBaseTablePrefixInfo);

			BizObj.ReportBaseTablePrefix = "AL";
			AssertNoErrors(BizObj.ReportBaseTablePrefixInfo);

			BizObj.ReportBaseTablePrefix = "ADH";
			AssertNoErrors(BizObj.ReportBaseTablePrefixInfo);

			BizObj.ReportBaseTablePrefix = "GLD";
			AssertHasErrors(@"The GLD - General Ledger Data"" table prefix can only be selected when ""Generate Journal Entries for Posted Accounting Transaction"" is set to ""Yes"" and a ""Generate Journal Entries - Start Date"" value is specified", BizObj.ReportBaseTablePrefixInfo);

			var mock = new Mock<IAccounting>();
			var testDataTime = new DateTime(2023, 1, 1);
			mock.Setup(m => m.GetGenerateJournalEntriesStartDate(Env.CurrentCompany.PK)).Returns(testDataTime);

			using (ObjectFactory.Substitute(mock.Object))
			{
				AssertEquals("GenerateJournalEntriesStartDate has been set", testDataTime, ObjectFactory.Get<IAccounting>().GetGenerateJournalEntriesStartDate(Env.CurrentCompany.PK));
				BizObj.ReportBaseTablePrefix = "GLD";
				AssertNoErrors(BizObj.ReportBaseTablePrefixInfo);
			}
		}

		public void TestValidateReportPeriodicity()
		{
			Assert("Default value is empty", BizObj.ReportPeriodicity.IsEmpty);
			BizObj.ValidateReportPeriodicity();
			AssertHasErrors("Is required", BizObj.ReportPeriodicityInfo);

			BizObj.ReportPeriodicity = "PER";
			AssertNoErrors(BizObj.ReportPeriodicityInfo);

			BizObj.ReportPeriodicity = "MNT";
			AssertNoErrors(BizObj.ReportPeriodicityInfo);

			BizObj.ReportPeriodicity = "ZZZ";
			AssertHasErrors("Invalid value", BizObj.ReportPeriodicityInfo);

			BizObj.ReportPeriodicity = "RNG";
			AssertNoErrors(BizObj.ReportPeriodicityInfo);
		}

		public void TestValidateTaxRegistrationType()
		{
			Assert("Default value is empty", BizObj.TaxRegistrationType.IsEmpty);

			BizObj.Country = "AU";
			Assert("Value is empty", BizObj.TaxRegistrationType.IsEmpty);
			BizObj.ValidateTaxRegistrationType();
			AssertHasErrors("Is required", BizObj.TaxRegistrationTypeInfo);

			BizObj.TaxRegistrationType = "ABN";
			AssertNoErrors(BizObj.TaxRegistrationTypeInfo);

			BizObj.TaxRegistrationType = "COD";
			AssertHasErrors("Not valid for AU", BizObj.TaxRegistrationTypeInfo);

			BizObj.Country = "IT";
			Assert("Cleared on changing Country", BizObj.TaxRegistrationType.IsEmpty);
		}

		public void TestValidateRepCountryRegistrationCode()
		{
			Assert("Default value is empty", BizObj.RepCountryRegistrationCode.IsEmpty);

			BizObj.Country = "AU";
			Assert("Value is empty", BizObj.RepCountryRegistrationCode.IsEmpty);
			BizObj.ValidateRepCountryRegistrationCode();
			AssertNoErrors("Is not required", BizObj.RepCountryRegistrationCodeInfo);

			BizObj.RepCountryRegistrationCode = "ABN";
			AssertNoErrors(BizObj.RepCountryRegistrationCodeInfo);

			BizObj.RepCountryRegistrationCode = "COD";
			AssertHasErrors("Not valid for AU", BizObj.RepCountryRegistrationCodeInfo);

			BizObj.Country = "IT";
			Assert("Cleared on changing Country", BizObj.RepCountryRegistrationCode.IsEmpty);
		}

		public void TestValidateReportLineGrouping()
		{
			Assert("Default value is empty", BizObj.ReportLineGrouping.IsEmpty);
			BizObj.ValidateReportLineGrouping();
			AssertNoErrors("Is not required", BizObj.ReportLineGroupingInfo);

			BizObj.ReportBaseTablePrefix = "AL";
			BizObj.ReportLineGrouping = "TXR";
			AssertNoErrors(BizObj.ReportLineGroupingInfo);

			BizObj.ReportLineGrouping = "HDR";
			AssertNoErrors(BizObj.ReportLineGroupingInfo);

			BizObj.ReportLineGrouping = "ZZZ";
			AssertHasErrors("Invalid value", BizObj.ReportLineGroupingInfo);

			BizObj.ReportLineGrouping = "ORG";
			AssertNoErrors(BizObj.ReportLineGroupingInfo);

			BizObj.ReportLineGrouping = "ORS";
			AssertNoErrors(BizObj.ReportLineGroupingInfo);

			BizObj.ReportLineGrouping = "DAB";
			AssertNoErrors(BizObj.ReportLineGroupingInfo);

			BizObj.ReportLineGrouping = "DBW";
			AssertNoErrors(BizObj.ReportLineGroupingInfo);

			BizObj.ReportLineGrouping = "DBP";
			AssertNoErrors(BizObj.ReportLineGroupingInfo);

			BizObj.ReportBaseTablePrefix = "ADH";
			BizObj.ReportLineGrouping = "TXR";
			AssertHasErrors("Invalid value", BizObj.ReportLineGroupingInfo);

			BizObj.ReportLineGrouping = "";
			AssertNoErrors(BizObj.ReportLineGroupingInfo);

			BizObj.ReportBaseTablePrefix = "**";
			BizObj.ReportLineGrouping = "";
			AssertNoErrors(BizObj.ReportLineGroupingInfo);

			BizObj.ReportLineGrouping = "DAB";
			AssertNoErrors(BizObj.ReportLineGroupingInfo);

			BizObj.ReportLineGrouping = "DBW";
			AssertNoErrors(BizObj.ReportLineGroupingInfo);

			BizObj.ReportBaseTablePrefix = "GLD";
			BizObj.ReportLineGrouping = "";
			AssertNoErrors(BizObj.ReportLineGroupingInfo);

			BizObj.ReportLineGrouping = "DAB";
			AssertNoErrors(BizObj.ReportLineGroupingInfo);

			BizObj.ReportLineGrouping = "DBW";
			AssertNoErrors(BizObj.ReportLineGroupingInfo);

			BizObj.ReportLineGrouping = "DBP";
			AssertNoErrors(BizObj.ReportLineGroupingInfo);
		}

		public void TestValidateReportLineOrdering()
		{
			Assert("Default value is empty", BizObj.ReportLineOrdering.IsEmpty);
			BizObj.ValidateReportLineOrdering();
			AssertNoErrors("Is not required", BizObj.ReportLineOrderingInfo);

			BizObj.Country = CountryCodes.Australia;
			BizObj.ReportBaseTablePrefix = "AH";
			BizObj.ReportLineOrdering = "LAS";
			AssertNoErrors(BizObj.ReportLineOrderingInfo);

			BizObj.ReportLineOrdering = "LDS";
			AssertNoErrors(BizObj.ReportLineOrderingInfo);

			BizObj.ReportLineOrdering = "CST";
			AssertNoErrors(BizObj.ReportLineOrderingInfo);

			BizObj.ReportLineOrdering = "CDN";
			AssertHasErrors("Invalid value", BizObj.ReportLineOrderingInfo);

			BizObj.Country = CountryCodes.Australia;
			BizObj.ReportBaseTablePrefix = "ADH";
			BizObj.ReportLineOrdering = "CDN";
			AssertNoErrors(BizObj.ReportLineOrderingInfo);

			BizObj.ReportLineOrdering = "CST";
			AssertHasErrors("Invalid value", BizObj.ReportLineOrderingInfo);

			BizObj.ReportLineOrdering = "FDN";
			AssertHasErrors("Invalid value", BizObj.ReportLineOrderingInfo);

			BizObj.Country = CountryCodes.Taiwan;
			BizObj.ReportBaseTablePrefix = "ADH";
			BizObj.ReportLineOrdering = "CDN";
			AssertNoErrors(BizObj.ReportLineOrderingInfo);

			BizObj.ReportLineOrdering = "FDN";
			AssertNoErrors(BizObj.ReportLineOrderingInfo);

			BizObj.ReportLineOrdering = "CST";
			AssertHasErrors("Invalid value", BizObj.ReportLineOrderingInfo);
		}

		public void TestValidateGoodsServiceType()
		{
			Assert("Default value is empty", BizObj.GoodsServiceType.IsEmpty);
			BizObj.ValidateGoodsServiceType();
			AssertNoErrors("Empty value means Both", BizObj.GoodsServiceTypeInfo);

			BizObj.GoodsServiceType = "GDS";
			AssertNoErrors(BizObj.GoodsServiceTypeInfo);

			BizObj.GoodsServiceType = "ZZZ";
			AssertHasErrors("Invalid value", BizObj.GoodsServiceTypeInfo);

			BizObj.GoodsServiceType = "SRV";
			AssertNoErrors(BizObj.GoodsServiceTypeInfo);
		}

		public void TestValidateReportAmountsRoundingTruncating()
		{
			AssertEquals("Default value", ZInt.Zero, BizObj.ReportAmountsRoundingTruncating);
			BizObj.ValidateReportAmountsRoundingTruncating();
			AssertNoErrors(BizObj.ReportAmountsRoundingTruncatingInfo);

			BizObj.ReportAmountsRoundingTruncating = -3;
			AssertNoErrors("Can be negative to round/truncate in this case to thousands", BizObj.ReportAmountsRoundingTruncatingInfo);

			BizObj.ReportAmountsRoundingTruncating = 1;
			AssertNoErrors("Round/truncate to one place after decimal point", BizObj.ReportAmountsRoundingTruncatingInfo);
		}

		public void TestValidateAmountThresholdLevel()
		{
			Assert("Default value is empty", BizObj.AmountThresholdLevel.IsEmpty);
			BizObj.ValidateAmountThresholdLevel();
			AssertNoErrors("Empty value means no threshold", BizObj.AmountThresholdLevelInfo);

			BizObj.AmountThresholdLevel = "HDR";
			AssertNoErrors(BizObj.AmountThresholdLevelInfo);

			BizObj.AmountThresholdLevel = "ZZZ";
			AssertHasErrors("Invalid value", BizObj.AmountThresholdLevelInfo);

			BizObj.AmountThresholdLevel = "ORG";
			AssertNoErrors(BizObj.AmountThresholdLevelInfo);
		}

		public void TestExTaxAmountThreshold()
		{
			Assert("Default value is empty", BizObj.AmountThresholdLevel.IsEmpty);
			AssertEquals(ZDecimal.Zero, BizObj.ExTaxAmountThreshold);
			Assert("Read only if no threshold level is set", BizObj.ExTaxAmountThresholdInfo.ReadOnly);

			BizObj.AmountThresholdLevel = "HDR";
			AssertEquals(false, BizObj.ExTaxAmountThresholdInfo.ReadOnly);

			BizObj.ExTaxAmountThreshold = 1000m;
			AssertNoErrors(BizObj.ExTaxAmountThresholdInfo);

			BizObj.AmountThresholdLevel = ZString.Empty;
			AssertEquals("Reset to 0 when no threshold is set", ZDecimal.Zero, BizObj.ExTaxAmountThreshold);
			Assert("Read only if no threshold level is set", BizObj.ExTaxAmountThresholdInfo.ReadOnly);
		}

		public void TestTaxAmountThreshold()
		{
			Assert("Default value is empty", BizObj.AmountThresholdLevel.IsEmpty);
			AssertEquals(ZDecimal.Zero, BizObj.TaxAmountThreshold);
			Assert("Read only if no threshold level is set", BizObj.TaxAmountThresholdInfo.ReadOnly);

			BizObj.AmountThresholdLevel = "HDR";
			AssertEquals(false, BizObj.TaxAmountThresholdInfo.ReadOnly);

			BizObj.TaxAmountThreshold = 100m;
			AssertNoErrors(BizObj.TaxAmountThresholdInfo);

			BizObj.AmountThresholdLevel = ZString.Empty;
			AssertEquals("Reset to 0 when no threshold is set", ZDecimal.Zero, BizObj.TaxAmountThreshold);
			Assert("Read only if no threshold level is set", BizObj.TaxAmountThresholdInfo.ReadOnly);
		}

		public void TestRecipientOrgPK()
		{
			Assert("Default value is empty", BizObj.RecipientOrgPK.IsEmpty);
			AssertNoErrors(BizObj.RecipientOrgPKInfo);

			BizObj.RecipientOrgPK = ZGuid.NewZGuid();
			AssertHasErrors(BizObj.RecipientOrgPKInfo);

			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			BizObj.RecipientOrgPK = org.PK;
			AssertNoErrors(BizObj.RecipientOrgPKInfo);

			BizObj.RecipientOrgPK = ZGuid.Empty;
			AssertNoErrors(BizObj.RecipientOrgPKInfo);
		}

		public void TestIncludeQueuedForPreviousPeriod()
		{
			BizObj.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.AllTransactions;
			Assert("Default value is false", !BizObj.IncludeQueuedForPreviousPeriod);
			AssertNoErrors(BizObj.IncludeQueuedForPreviousPeriodInfo);

			BizObj.IncludeQueuedForPreviousPeriod = true;
			Assert(BizObj.IncludeQueuedForPreviousPeriod);
			AssertHasErrors(BizObj.IncludeQueuedForPreviousPeriodInfo);

			BizObj.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.GeneralLedgerData;
			Assert(BizObj.IncludeQueuedForPreviousPeriod);
			AssertHasError(BizObj.IncludeQueuedForPreviousPeriodInfo, "Include previous queued records can't be true if the Table Prefix is set on General Ledger Data.");

			BizObj.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.TransactionHeader;
			AssertNoErrors(BizObj.IncludeQueuedForPreviousPeriodInfo);
		}

		public void TestIsDefaultReportType()
		{
			var configurations = new ComplianceReportConfigurationCollection(Factory);
			BizObj.Country = "AU";
			configurations.Add(BizObj);

			AssertEquals(1, BizObj.ParentCollection.Count);
			BizObj.IsDefaultReportType = true;
			AssertNoErrors(BizObj.IsDefaultReportTypeInfo);
			BizObj.IsDefaultReportType = false;
			AssertNoErrors(BizObj.IsDefaultReportTypeInfo);

			var reportConfig2 = configurations.AddNew();
			reportConfig2.IsDefaultReportType = false;
			AssertNoErrors(reportConfig2.IsDefaultReportTypeInfo);
			reportConfig2.IsDefaultReportType = true;
			AssertNoErrors(reportConfig2.IsDefaultReportTypeInfo);

			BizObj.IsDefaultReportType = true;
			AssertHasError(BizObj.IsDefaultReportTypeInfo, "There must be only one default report.");

			BizObj.IsDefaultReportType = false;
			AssertNoErrors(BizObj.IsDefaultReportTypeInfo);
			AssertNoErrors(reportConfig2.IsDefaultReportTypeInfo);
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected new ComplianceReportConfiguration BizObj
		{
			get { return (ComplianceReportConfiguration)base.BizObj; }
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new ComplianceReportConfiguration();
		}

		#endregion
	}
}
