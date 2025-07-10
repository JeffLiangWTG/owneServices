using System.IO;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ComplianceReportConfigurationSetting))]
	sealed class ComplianceReportConfigurationSettingTest : RegistryBusinessObjectTemplateTestCase<ComplianceReportConfigurationSetting>
	{
		public void TestClearTaxRegistrationTypeIfCountryIsNotArgentina()
		{
			var newSetting = SetUpARConfiguration();
			AssertEquals("REC", newSetting.TaxRegistrationType);
			Assert("expect TaxRegistrationType is not readonly when country is Argentina", !newSetting.TaxRegistrationTypeInfo.ReadOnly);
			newSetting.Country = "PE";
			AssertEquals("expect empty string when country is not Argentina", "", newSetting.TaxRegistrationType);
			Assert("expect TaxRegistrationType is readonly when country is not Argentina", newSetting.TaxRegistrationTypeInfo.ReadOnly);
		}

		public void TestValidateTaxTaxRegistrationType()
		{
			var newSetting = SetUpARConfiguration();
			AssertNoErrors("Precondition: TaxRegistrationType should not have errors.", BizObj.TaxRegistrationTypeInfo);

			newSetting.TaxRegistrationType = "";
			AssertNoErrors("TaxRegistrationType can be blank", newSetting.TaxRegistrationTypeInfo);

			newSetting.TaxRegistrationType = "ABC";
			AssertHasErrors("TaxRegistrationType has invalid code", newSetting.TaxRegistrationTypeInfo);

			newSetting.TaxRegistrationType = "NOT";
			AssertNoErrors(newSetting.TaxRegistrationTypeInfo);
		}

		public void TestValidateSubType()
		{
			AssertNoErrors("Precondition: ComplianceSubType should not have errors.", BizObj.ComplianceSubTypeInfo);

			BizObj.ComplianceSubType = "";
			AssertNoErrors("ComplianceSubType can be blank", BizObj.ComplianceSubTypeInfo);

			BizObj.ComplianceSubType = "AAA";
			AssertHasErrors("ComplianceSubType has invalid code", BizObj.ComplianceSubTypeInfo);

			BizObj.ComplianceSubType = "TXI";
			Assert(!BizObj.ComplianceSubTypeInfo.HasErrors());
		}

		public void TestValidateLedgerType()
		{
			AssertLedgerTypeForBaseTable("AH");
			AssertLedgerTypeForBaseTable("AL");
			AssertLedgerTypeForBaseTable("**");

			BizObj.ComplianceSubType = "AAA";
			BizObj.LedgerType = "";
			AssertNoErrors("LedgerType can be blank when ComplianceSubType is set", BizObj.LedgerTypeInfo);
		}

		void AssertLedgerTypeForBaseTable(string tablePrefix)
		{
			ReportConfiguration.ReportBaseTablePrefix = tablePrefix;

			Assert("Precondition: ComplianceSubTypeHas a value.", !BizObj.ComplianceSubType.IsEmpty);
			var saveCompliance = BizObj.ComplianceSubType;

			BizObj.LedgerType = "";
			AssertNoErrors(tablePrefix + " table prefix, LedgerType can be blank when ComplianceSubType is not empty", BizObj.LedgerTypeInfo);

			BizObj.ComplianceSubType = "";
			BizObj.ValidateLedgerType();
			AssertHasErrors(tablePrefix + " table prefix, LedgerType can not be blank when ComplianceSubType is empty", BizObj.LedgerTypeInfo);

			foreach (CodeDescriptionPair ledger in BizObj.Lookups.LedgerTypeList)
			{
				BizObj.LedgerType = ledger.Code;
				AssertNoErrors(tablePrefix + " table prefix, LedgerType '" + ledger.Code + "' is valid", BizObj.LedgerTypeInfo);
			}

			BizObj.LedgerType = "$$";
			AssertHasErrors(tablePrefix + " table prefix, invalid LedgerType '$$'", BizObj.LedgerTypeInfo);
			BizObj.ComplianceSubType = saveCompliance;
		}

		public void TestValidateInvoiceType()
		{
			ReportConfiguration.ReportBaseTablePrefix = "AH";
			AssertInvoiceTypesForLedger(LedgerTypes.AccountsReceivable);
			AssertInvoiceTypesForLedger(LedgerTypes.AccountsPayable);

			ReportConfiguration.ReportBaseTablePrefix = "AL";
			AssertInvoiceTypesForLedger(LedgerTypes.AccountsReceivable);
			AssertInvoiceTypesForLedger(LedgerTypes.AccountsPayable);

			ReportConfiguration.ReportBaseTablePrefix = "**";
			AssertInvoiceTypesForLedger(LedgerTypes.AccountsReceivable);
			AssertInvoiceTypesForLedger(LedgerTypes.AccountsPayable);
			AssertInvoiceTypesForLedger(LedgerTypes.CashBook);
			AssertInvoiceTypesForLedger(LedgerTypes.General);
			AssertInvoiceTypesForLedger(LedgerTypes.JobCosting);
			AssertInvoiceTypesForLedger(ComplianceReportConfigurationSettingLookups.PseudoLedgerCodes.CashBasisTax);

			BizObj.ComplianceSubType = "";
			BizObj.LedgerType = "";
			BizObj.InvoiceType = "";
			AssertHasErrors("Empty InvoiceType should have errors when ComplianceSubType is not set.", BizObj.InvoiceTypeInfo);

			BizObj.ComplianceSubType = "AAA";
			BizObj.LedgerType = "";
			BizObj.InvoiceType = "";
			BizObj.ValidateInvoiceType();
			AssertNoErrors("InvoiceType can be blank when ComplianceSubType is set.", BizObj.InvoiceTypeInfo);
		}

		void AssertInvoiceTypesForLedger(string ledger)
		{
			BizObj.LedgerType = ledger;
			AssertNoErrors("LedgerType '" + ledger + "' should be valid", BizObj.LedgerTypeInfo);

			BizObj.ComplianceSubType = "";
			BizObj.InvoiceType = "";
			BizObj.ValidateInvoiceType();
			AssertHasErrors("InvoiceType can not be blank", BizObj.InvoiceTypeInfo);

			foreach (CodeDescriptionPair type in BizObj.Lookups.InvoiceTypeList)
			{
				BizObj.InvoiceType = type.Code;
				AssertNoErrors("Ledger '" + ledger + "', InvoiceType '" + type.Code + "' is valid", BizObj.InvoiceTypeInfo);
			}

			BizObj.InvoiceType = "$$$";
			AssertHasErrors("Ledger '" + ledger + "', invalid InvoiceType '$$$'", BizObj.InvoiceTypeInfo);
		}

		public void TestValidateTaxInvoiceRule()
		{
			AssertNoErrors("Precondition: TaxInvoiceRule should not have errors.", BizObj.TaxInvoiceRuleInfo);

			BizObj.TaxInvoiceRule = "";
			AssertNoErrors("TaxInvoiceRule can be blank", BizObj.TaxInvoiceRuleInfo);

			BizObj.TaxInvoiceRule = "ALL";
			Assert(!BizObj.TaxInvoiceRuleInfo.HasErrors());

			BizObj.TaxInvoiceRule = "AAA";
			AssertHasErrors("InvoiceType has invalid code", BizObj.TaxInvoiceRuleInfo);

			BizObj.TaxInvoiceRule = "AMT";
			AssertNoErrors(BizObj.TaxInvoiceRuleInfo);

			BizObj.TaxInvoiceRule = "TID";
			AssertNoErrors(BizObj.TaxInvoiceRuleInfo);

			BizObj.TaxInvoiceRule = "TXN";
			AssertNoErrors(BizObj.TaxInvoiceRuleInfo);

			BizObj.TaxInvoiceRule = "TXX";
			AssertNoErrors(BizObj.TaxInvoiceRuleInfo);

			BizObj.TaxInvoiceRule = "TXA";
			AssertNoErrors(BizObj.TaxInvoiceRuleInfo);

			BizObj.TaxInvoiceRule = "TXR";
			AssertNoErrors(BizObj.TaxInvoiceRuleInfo);

			BizObj.TaxInvoiceRule = "RVS";
			AssertNoErrors(BizObj.TaxInvoiceRuleInfo);

			BizObj.TaxInvoiceRule = "EXL";
			AssertNoErrors(BizObj.TaxInvoiceRuleInfo);

			BizObj.TaxInvoiceRule = "NON";
			AssertNoErrors(BizObj.TaxInvoiceRuleInfo);
		}

		public void TestValidateOriginalRule()
		{
			AssertNoErrors("Precondition: OriginalRule should not have errors.", BizObj.OriginalRuleInfo);

			BizObj.OriginalRule = "";
			AssertNoErrors("OriginalRule can be blank", BizObj.OriginalRuleInfo);

			BizObj.OriginalRule = "AAA";
			AssertHasErrors("OriginalRule has invalid code", BizObj.OriginalRuleInfo);

			BizObj.OriginalRule = "ALL";
			Assert(!BizObj.OriginalRuleInfo.HasErrors());
		}

		public void TestValidateDisbursementRule()
		{
			AssertNoErrors("Precondition: DisbursementRule should not have errors.", BizObj.DisbursementRuleInfo);

			BizObj.DisbursementRule = "";
			AssertNoErrors("DisbursementRule can be blank", BizObj.DisbursementRuleInfo);

			BizObj.DisbursementRule = "AAA";
			AssertHasErrors("DisbursementRule has invalid code", BizObj.DisbursementRuleInfo);

			BizObj.DisbursementRule = "ALL";
			Assert(!BizObj.DisbursementRuleInfo.HasErrors());
		}

		public void TestValidateOrganisationCategory()
		{
			AssertNoErrors("Precondition: OrganisationCategory should not have errors.", BizObj.OrganisationCategoryInfo);

			BizObj.OrganisationCategory = "";
			AssertNoErrors("OrganisationCategory can be blank", BizObj.OrganisationCategoryInfo);

			BizObj.OrganisationCategory = "AAA";
			AssertHasErrors("OrganisationCategory has invalid code", BizObj.OrganisationCategoryInfo);

			BizObj.OrganisationCategory = OrgConstants.Category.Government;
			AssertNoErrors(BizObj.OrganisationCategoryInfo);
		}

		public void TestCheckDifferentTaxInvoiceRuleExist()
		{
			var newSetting = ReportConfiguration.Settings.AddNew();
			newSetting.Country = "PE";
			newSetting.ComplianceSubType = "TXI";
			newSetting.LedgerType = "AR";
			newSetting.InvoiceType = "INV";
			newSetting.TaxInvoiceRule = "TID";
			newSetting.DisbursementRule = "DSB";
			newSetting.OriginalRule = "ARO";
			newSetting.OrganisationLocation = "PE";
			AssertNoErrors("Precondition: TaxInvoiceRule should not have errors.", newSetting.TaxInvoiceRuleInfo);

			newSetting.TaxInvoiceRule = "AMT";
			AssertNoErrors("TaxInvoiceRule should not have errors.", newSetting.TaxInvoiceRuleInfo);

			newSetting.DisbursementRule = "NDB";
			newSetting.OriginalRule = "OTO";
			newSetting.TaxInvoiceRule = "AMT";
			AssertHasError(newSetting.TaxInvoiceRuleInfo, "Different tax invoice rule already exists in another setting for the same Report");

			newSetting.DisbursementRule = "DSB";
			newSetting.OriginalRule = "ARO";
			newSetting.TaxInvoiceRule = "TID";
			AssertNoErrors("TaxInvoiceRule should not have errors.", newSetting.TaxInvoiceRuleInfo);
		}

		public void TestExtraValidationControlledByArgentinaTaxRegistrationType()
		{
			var setting1 = SetUpARConfiguration();
			setting1.TaxRegistrationType = "REC";

			var setting2 = SetUpARConfiguration();
			setting2.TaxRegistrationType = "REC";
			AssertHasError(setting2.TaxRegistrationTypeInfo, "Setting with identical criteria already exists!");

			setting2.TaxRegistrationType = "NOT";
			AssertNoErrors("TaxRegistrationType should not have errors.", setting2.TaxRegistrationTypeInfo);

			setting2.DisbursementRule = "ALL";
			setting2.TaxRegistrationType = "REC";
			AssertHasError(setting2.TaxRegistrationTypeInfo, "Overlapped settings are detected, please check Original rule/Disbursement Rule is not overlapped");

			setting2.TaxRegistrationType = "NOT";
			AssertNoErrors("TaxRegistrationType should not have errors.", setting2.TaxRegistrationTypeInfo);

			setting2.DisbursementRule = "NDB";
			setting2.TaxInvoiceRule = "AMT";
			setting2.TaxRegistrationType = "REC";
			AssertHasError(setting2.TaxRegistrationTypeInfo, "Different tax invoice rule already exists in another setting for the same Report");

			setting2.TaxRegistrationType = "NOT";
			AssertNoErrors("TaxRegistrationType should not have errors.", setting2.TaxRegistrationTypeInfo);
		}

		public void TestReportingDateDefaultedBySettingLedgerType()
		{
			var setting = BizObj;

			var ledgersWithPostDate = new[] { LedgerTypes.AccountsReceivable, LedgerTypes.AccountsPayable };
			var ledgersReadOnly = setting.Lookups.LedgerTypeList.GetAllCodes().Except(ledgersWithPostDate);
			setting.LedgerType = "";

			CombineAssertions(() =>
			{
				foreach (var ledger in ledgersWithPostDate)
				{
					setting.ReportingDate = "";
					setting.LedgerType = ledger;
					AssertEquals("Defaulted to POS for Ledger: " + ledger, "POS", setting.ReportingDate);

					setting.ReportingDate = "TDE";
					setting.LedgerType = ledger;
					AssertEquals("No redefaulting by same Ledger: " + ledger, "TDE", setting.ReportingDate);
				}

				foreach (var ledger in ledgersReadOnly)
				{
					setting.ReportingDate = "POS";
					setting.LedgerType = ledger;
					AssertEquals("Defaulted to empty for Ledger: " + ledger, "", setting.ReportingDate);

					setting.ReportingDate = "***";
					setting.LedgerType = ledger;
					AssertEquals("No redefaulting by same Ledger: " + ledger, "***", setting.ReportingDate);
				}
			});
		}

		public void TestReportingDateReadOnly()
		{
			var setting = BizObj;

			var ledgersEditable = new[] { LedgerTypes.AccountsReceivable, LedgerTypes.AccountsPayable };
			var ledgersReadOnly = setting.Lookups.LedgerTypeList.GetAllCodes().Except(ledgersEditable);

			CombineAssertions(() =>
			{
				foreach (var ledger in ledgersEditable)
				{
					setting.LedgerType = ledger;
					AssertEquals("Editable for Ledger: " + ledger, false, setting.ReportingDateInfo.ReadOnly);
				}

				foreach (var ledger in ledgersReadOnly)
				{
					setting.LedgerType = ledger;
					Assert("ReadOnly for Ledger: " + ledger, setting.ReportingDateInfo.ReadOnly);
				}
			});
		}

		public void TestReportingDateValidation()
		{
			var setting = BizObj;

			setting.LedgerType = LedgerTypes.AccountsReceivable;
			AssertEquals("Defaulted by setting Ledger", "POS", setting.ReportingDate);

			setting.ReportingDate = "";
			AssertHasError(setting.ReportingDateInfo, "Please enter a value.");

			setting.ReportingDate = "INV";
			AssertNoErrors(setting.ReportingDateInfo);

			setting.ReportingDate = "DRD";
			AssertHasError(setting.ReportingDateInfo, "Enter a valid selection.");

			setting.LedgerType = LedgerTypes.AccountsPayable;
			AssertEquals("Defaulted by setting Ledger", "POS", setting.ReportingDate);

			setting.ReportingDate = "";
			AssertHasError(setting.ReportingDateInfo, "Please enter a value.");

			setting.ReportingDate = "INV";
			AssertNoErrors(setting.ReportingDateInfo);

			setting.ReportingDate = "XYZ";
			AssertHasError(setting.ReportingDateInfo, "Enter a valid selection.");

			setting.ReportingDate = "DRD";
			AssertNoErrors(setting.ReportingDateInfo);
		}

		public void TestCheckIdenticalConfigurationExist()
		{
			var newSetting = ReportConfiguration.Settings.AddNew();
			newSetting.Country = "PE";
			newSetting.ComplianceSubType = "TXI";
			newSetting.LedgerType = "AR";
			newSetting.InvoiceType = "INV";
			newSetting.TaxInvoiceRule = "TID";
			newSetting.DisbursementRule = "DSB";
			newSetting.OriginalRule = "ARO";
			newSetting.OrganisationLocation = "PE";
			AssertNoErrors("Precondition: OriginalRule should not have errors.", newSetting.OriginalRuleInfo);

			newSetting.DisbursementRule = "NDB";
			newSetting.OriginalRule = "OTO";
			AssertHasError(newSetting.OriginalRuleInfo, "Setting with identical criteria already exists!");
			newSetting.OriginalRule = "ARO";
			AssertNoErrors("OriginalRule should not have errors.", newSetting.OriginalRuleInfo);

			newSetting.OriginalRule = "OTO";
			AssertHasError(newSetting.OriginalRuleInfo, "Setting with identical criteria already exists!");
			newSetting.OrganisationLocation = "VN";
			AssertNoErrors("OrganisationLocation should not have errors.", newSetting.OrganisationLocationInfo);

			newSetting.OriginalRule = "ARO";
			AssertNoErrors("OriginalRule should not have errors.", newSetting.OriginalRuleInfo);

			newSetting.OriginalRule = "OTO";
			AssertNoErrors("OriginalRule should not have errors.", newSetting.OriginalRuleInfo);
		}

		public void TestCheckOverlappedConfigurationExist()
		{
			var newSetting = ReportConfiguration.Settings.AddNew();
			newSetting.Country = "PE";
			newSetting.ComplianceSubType = "TXI";
			newSetting.LedgerType = "AR";
			newSetting.InvoiceType = "INV";
			newSetting.TaxInvoiceRule = "TID";
			newSetting.DisbursementRule = "DSB";
			newSetting.OriginalRule = "ARO";
			newSetting.OrganisationLocation = "PE";
			AssertNoErrors("Precondition: OriginalRule should not have errors.", newSetting.OriginalRuleInfo);

			newSetting.DisbursementRule = "ALL";
			newSetting.OriginalRule = "ALL";
			AssertHasError(newSetting.OriginalRuleInfo, "Overlapped settings are detected, please check Original rule/Disbursement Rule is not overlapped");
			newSetting.OriginalRule = "ARO";
			AssertNoErrors("OriginalRule should not have errors.", newSetting.OriginalRuleInfo);

			newSetting.TaxInvoiceRule = "TID";
			newSetting.DisbursementRule = "ALL";
			newSetting.OriginalRule = "OTO";
			AssertHasError(newSetting.OriginalRuleInfo, "Overlapped settings are detected, please check Original rule/Disbursement Rule is not overlapped");

			newSetting.TaxInvoiceRule = "TXX";
			newSetting.OriginalRule = "OTO";
			AssertNoErrors("OriginalRule should not have errors.", newSetting.OriginalRuleInfo);

			newSetting.TaxInvoiceRule = "TID";
			newSetting.OriginalRule = "OTO";
			AssertHasError(newSetting.OriginalRuleInfo, "Overlapped settings are detected, please check Original rule/Disbursement Rule is not overlapped");

			newSetting.OrganisationLocation = "VN";
			newSetting.OriginalRule = "OTO";
			AssertNoErrors("OriginalRule should not have errors.", newSetting.OriginalRuleInfo);
		}

		public void TestCheckIdenticalConfigurationOrganisationCategory()
		{
			var newSetting1 = ReportConfiguration.Settings.AddNew();
			newSetting1.Country = "FR";
			newSetting1.LedgerType = "AR";
			newSetting1.InvoiceType = "INV";
			newSetting1.TaxInvoiceRule = "ALL";
			newSetting1.OriginalRule = "ALL";
			newSetting1.DisbursementRule = "ALL";
			newSetting1.OrganisationLocation = "ALX";
			newSetting1.OrganisationCategory = "BUS";

			var newSetting2 = ReportConfiguration.Settings.AddNew();
			newSetting2.Country = "FR";
			newSetting2.LedgerType = "AR";
			newSetting2.InvoiceType = "INV";
			newSetting2.TaxInvoiceRule = "ALL";
			newSetting2.OriginalRule = "ALL";
			newSetting2.DisbursementRule = "ALL";
			newSetting2.OrganisationLocation = "ALX";
			newSetting2.OrganisationCategory = "GOV";

			// Both configurations differ in OrgCategory
			AssertNoErrors(newSetting1.OrganisationCategoryInfo);
			AssertNoErrors(newSetting2.OrganisationCategoryInfo);

			// Now both configurations are identical
			newSetting2.OrganisationCategory = "BUS";
			AssertHasError(newSetting2.OrganisationCategoryInfo, "Setting with identical criteria already exists!");
		}

		public void TestCheckOverlappedConfigurationOrganisationCategory()
		{
			var newSetting1 = ReportConfiguration.Settings.AddNew();
			newSetting1.Country = "FR";
			newSetting1.LedgerType = "AR";
			newSetting1.InvoiceType = "INV";
			newSetting1.TaxInvoiceRule = "ALL";
			newSetting1.OriginalRule = "OTO";
			newSetting1.DisbursementRule = "ALL";
			newSetting1.OrganisationLocation = "ALX";
			newSetting1.OrganisationCategory = "BUS";

			var newSetting2 = ReportConfiguration.Settings.AddNew();
			newSetting2.Country = "FR";
			newSetting2.LedgerType = "AR";
			newSetting2.InvoiceType = "INV";
			newSetting2.TaxInvoiceRule = "ALL";
			newSetting2.OriginalRule = "ALL";
			newSetting2.DisbursementRule = "ALL";
			newSetting2.OrganisationLocation = "ALX";
			newSetting2.OrganisationCategory = "GOV";

			// Both configurations differ in OrganisationCategory selecting disjunct records
			Assert("Pre-requisite: No errors should be present", !newSetting1.HasErrors);
			Assert("Pre-requisite: No errors should be present", !newSetting2.HasErrors);

			// Now both configurations are overlapping (OrgCategories match, OriginalRule ALL overlaps OTO)
			newSetting2.OrganisationCategory = "BUS";
			Assert(newSetting2.HasErrors);
			AssertHasError(newSetting2.OrganisationCategoryInfo, "Overlapped settings are detected, please check Original rule/Disbursement Rule is not overlapped");

			// Now both configurations are disjunct again (different OrgCategory)
			newSetting2.OrganisationCategory = "GOV";
			Assert(!newSetting1.HasErrors);
			AssertNoErrors(newSetting1.OrganisationCategoryInfo);
			Assert(!newSetting2.HasErrors);
			AssertNoErrors(newSetting2.OrganisationCategoryInfo);
		}

		public void TestCheckDifferentTaxInvoiceRuleExistOrganizationCategory()
		{
			var newSetting1 = ReportConfiguration.Settings.AddNew();
			newSetting1.Country = "FR";
			newSetting1.LedgerType = "AR";
			newSetting1.InvoiceType = "INV";
			newSetting1.TaxInvoiceRule = "ALL";
			newSetting1.OriginalRule = "ALL";
			newSetting1.DisbursementRule = "ALL";
			newSetting1.OrganisationLocation = "ALX";
			newSetting1.OrganisationCategory = "BUS";

			var newSetting2 = ReportConfiguration.Settings.AddNew();
			newSetting2.Country = "FR";
			newSetting2.LedgerType = "AR";
			newSetting2.InvoiceType = "INV";
			newSetting2.TaxInvoiceRule = "RVS";
			newSetting2.OriginalRule = "ALL";
			newSetting2.DisbursementRule = "ALL";
			newSetting2.OrganisationLocation = "ALX";
			newSetting2.OrganisationCategory = "GOV";

			// Both configurations differ in OrgCategory and TaxInvoiceRule
			AssertNoErrors(newSetting1.OrganisationCategoryInfo);
			AssertNoErrors(newSetting2.OrganisationCategoryInfo);

			// Now both configurations are identical apart from the TaxInvoiceRule
			newSetting2.OrganisationCategory = "BUS";
			AssertHasError(newSetting2.OrganisationCategoryInfo, "Different tax invoice rule already exists in another setting for the same Report");
		}

		public void TestXmlSerializationRoundTrip()
		{
			var expected = ReportConfiguration.Settings.AddNew();
			expected.Country = "FR";
			expected.ComplianceSubType = "TXI";
			expected.LedgerType = "AR";
			expected.InvoiceType = "INV";
			expected.TaxInvoiceRule = "TID";
			expected.DisbursementRule = "ALL";
			expected.OriginalRule = "ALL";
			expected.OrganisationLocation = "FR";
			expected.TaxRegistrationType = "REC";
			expected.ReportingDate = "POS";
			expected.OrganisationCategory = "BUS";

			// Serialize the setting to XML and back
			ComplianceReportConfigurationSetting actual;
			using (var writer = new StringWriter())
			{
				var serializer = new XmlSerializer(typeof(ComplianceReportConfigurationSetting));
				serializer.Serialize(writer, expected);
				var serializedXml = writer.ToString();

				using (var reader = new StringReader(serializedXml))
				{
					actual = (ComplianceReportConfigurationSetting)serializer.Deserialize(reader);
				}
			}

			AssertNotNull(actual);
			AssertEquals(expected.Country, actual.Country);
			AssertEquals(expected.ComplianceSubType, actual.ComplianceSubType);
			AssertEquals(expected.LedgerType, actual.LedgerType);
			AssertEquals(expected.InvoiceType, actual.InvoiceType);
			AssertEquals(expected.TaxInvoiceRule, actual.TaxInvoiceRule);
			AssertEquals(expected.DisbursementRule, actual.DisbursementRule);
			AssertEquals(expected.OriginalRule, actual.OriginalRule);
			AssertEquals(expected.OrganisationLocation, actual.OrganisationLocation);
			AssertEquals(expected.TaxRegistrationType, actual.TaxRegistrationType);
			AssertEquals(expected.ReportingDate, actual.ReportingDate);
			AssertEquals(expected.OrganisationCategory, actual.OrganisationCategory);
		}

		#region Implementation

		protected override ComplianceReportConfigurationSetting GetBusinessObjectToClone()
		{
			return (ComplianceReportConfigurationSetting)GetNewBusinessObject();
		}

		protected override void SetUp()
		{
			base.SetUp();
			ReportConfiguration = new ComplianceReportConfiguration(Factory);

			BizObj = ReportConfiguration.Settings.AddNew();
			BizObj.Country = "PE";
			BizObj.ComplianceSubType = "TXI";
			BizObj.LedgerType = "AR";
			BizObj.InvoiceType = "INV";
			BizObj.TaxInvoiceRule = "TID";
			BizObj.DisbursementRule = "NDB";
			BizObj.OriginalRule = "OTO";
			BizObj.OrganisationLocation = "PE";
		}

		ComplianceReportConfigurationSetting SetUpARConfiguration()
		{
			var newSetting = ReportConfiguration.Settings.AddNew();
			newSetting.Country = "AR";
			newSetting.ComplianceSubType = "TXA";
			newSetting.LedgerType = "AR";
			newSetting.InvoiceType = "INV";
			newSetting.TaxInvoiceRule = "TID";
			newSetting.DisbursementRule = "NDB";
			newSetting.OriginalRule = "ALL";
			newSetting.TaxRegistrationType = "REC";
			return newSetting;
		}

		ComplianceReportConfiguration ReportConfiguration;

		protected override ComplianceReportConfigurationSetting GetBusinessObjectToSerialise()
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

		#endregion
	}
}
