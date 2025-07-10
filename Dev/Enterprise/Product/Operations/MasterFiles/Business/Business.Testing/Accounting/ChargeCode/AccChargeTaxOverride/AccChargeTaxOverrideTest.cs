using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccChargeTaxOverride))]
	sealed class AccChargeTaxOverrideTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIsDuplicate()
		{
			AccChargeCode charge = Factory.New<AccChargeCode>();
			AccChargeTaxOverride tax1 = charge.TaxOverrides.AddNew();
			AccChargeTaxOverride tax2 = charge.TaxOverrides.AddNew();

			tax1.AO_Direction = "IMP";
			tax1.AO_IncoTerm = "EXP";

			tax2.AO_Direction = "IMP";
			tax2.AO_IncoTerm = "EXP";

			Assert(tax1.IsDuplicate(tax2));
			Assert(tax2.IsDuplicate(tax1));

			tax2.AO_CostSellAll = "C";
			Assert(!tax1.IsDuplicate(tax2));
			Assert(!tax2.IsDuplicate(tax1));

			tax1.AO_CostSellAll = "C";
			Assert(tax1.IsDuplicate(tax2));
			Assert(tax2.IsDuplicate(tax1));

			tax2.AO_CustomsStatus = "T1";
			Assert(!tax1.IsDuplicate(tax2));
			Assert(!tax2.IsDuplicate(tax1));

			tax1.AO_CustomsStatus = "T1";
			Assert(tax1.IsDuplicate(tax2));
			Assert(tax2.IsDuplicate(tax1));

			tax2.AO_HomeCountryOrZone = "US";
			Assert(!tax1.IsDuplicate(tax2));
			Assert(!tax2.IsDuplicate(tax1));

			tax1.AO_HomeCountryOrZone = "US";
			Assert(tax1.IsDuplicate(tax2));
			Assert(tax2.IsDuplicate(tax1));

			tax2.AO_VATExemptOnExportCharges = true;
			Assert(!tax1.IsDuplicate(tax2));
			Assert(!tax2.IsDuplicate(tax1));

			tax1.AO_VATExemptOnExportCharges = true;
			Assert(tax1.IsDuplicate(tax2));
			Assert(tax2.IsDuplicate(tax1));

			tax2.AO_OrganisationCategory = "GOV";
			Assert(!tax1.IsDuplicate(tax2));
			Assert(!tax2.IsDuplicate(tax1));

			tax1.AO_OrganisationCategory = "GOV";
			Assert(tax1.IsDuplicate(tax2));
			Assert(tax2.IsDuplicate(tax1));

			tax2.AO_SplitPaymentVATOrganisation = true;
			Assert(!tax1.IsDuplicate(tax2));
			Assert(!tax2.IsDuplicate(tax1));

			tax1.AO_SplitPaymentVATOrganisation = true;
			Assert(tax1.IsDuplicate(tax2));
			Assert(tax2.IsDuplicate(tax1));

			tax2.AO_GB = GlbBranch.CurrentBranch.PK;
			Assert(!tax1.IsDuplicate(tax2));
			Assert(!tax2.IsDuplicate(tax1));

			tax1.AO_GB = GlbBranch.CurrentBranch.PK;
			Assert(tax1.IsDuplicate(tax2));
			Assert(tax2.IsDuplicate(tax1));

			tax2.AO_TransactionContext = "ALL";
			Assert(!tax1.IsDuplicate(tax2));
			Assert(!tax2.IsDuplicate(tax1));

			tax1.AO_TransactionContext = "ALL";
			Assert(tax1.IsDuplicate(tax2));
			Assert(tax2.IsDuplicate(tax1));

			tax2.AO_SupplyType = "DSB";
			Assert(!tax1.IsDuplicate(tax2));
			Assert(!tax2.IsDuplicate(tax1));

			tax1.AO_SupplyType = "DSB";
			Assert(tax1.IsDuplicate(tax2));
			Assert(tax2.IsDuplicate(tax1));

			tax2.AO_DebtorRole = "AGT";
			Assert(!tax1.IsDuplicate(tax2));
			Assert(!tax2.IsDuplicate(tax1));

			tax1.AO_DebtorRole = "AGT";
			Assert(tax1.IsDuplicate(tax2));
			Assert(tax2.IsDuplicate(tax1));

			tax1.AO_DefaultingRule = "ART";
			tax2.AO_DefaultingRule = "SUM";
			Assert("DefaultingRule is not a condition for duplicatejudgments", tax1.IsDuplicate(tax2));
			Assert("DefaultingRule is not a condition for duplicatejudgments", tax2.IsDuplicate(tax1));
		}

		public void TestIsTaxFrameworkRelated()
		{
			var taxOverride = Factory.NewWithValidTestData<AccChargeTaxOverride>();

			AssertNull("Precondition:", taxOverride.TaxOverrideGroup);
			Assert(!taxOverride.IsTaxFrameworkRelated);

			var taxOverrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			taxOverride.AO_ParentID = taxOverrideGroup.PK;
			taxOverride.AO_ParentTableCode = AccTaxOverrideGroupSchema.Constants.Prefix;

			AssertNotNull("Precondition:", taxOverride.TaxOverrideGroup);
			Assert("Precondition:", !taxOverride.TaxOverrideGroup.IsTaxFrameworkRelated);
			Assert(!taxOverride.IsTaxFrameworkRelated);

			taxOverrideGroup.SetContext(AccTaxOverrideGroup.BusinessContext.TaxFramework);
			Assert("Precondition:", taxOverride.TaxOverrideGroup.IsTaxFrameworkRelated);
			Assert(taxOverride.IsTaxFrameworkRelated);

			taxOverrideGroup.TaxOverrideGroupTaxConfigurationPivots.AddNew();
			Assert("Precondition:", taxOverrideGroup.TaxOverrideGroupTaxConfigurationPivots.Count > 0);
			Assert(taxOverride.IsTaxFrameworkRelated);

			taxOverrideGroup.RemoveContext(AccTaxOverrideGroup.BusinessContext.TaxFramework);
			Assert("Precondition:", taxOverride.TaxOverrideGroup.IsTaxFrameworkRelated);
			Assert(taxOverride.IsTaxFrameworkRelated);
		}

		public void TestDebtorRoleShouldDisableInTaxFramework()
		{
			var overrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			var charge = Factory.NewWithValidTestData<AccChargeCode>();
			charge.AC_AX_TaxOverrideGroup = overrideGroup.PK;
			var taxOverride_fromGroup = charge.TaxOverrideGroup.TaxOverrides.AddNew();
			taxOverride_fromGroup.AO_CostSellAll = "REV";
			taxOverride_fromGroup.AO_JobType = "SHP";
			taxOverride_fromGroup.TaxOverrideGroup.Factory.SetContext(AccTaxOverrideGroup.BusinessContext.TaxFramework);
			AssertEquals("Precondition:", true, taxOverride_fromGroup.AO_DebtorRole_ReadOnly);

			taxOverride_fromGroup.TaxOverrideGroup.Factory.RemoveContext(AccTaxOverrideGroup.BusinessContext.TaxFramework);
			AssertEquals("Precondition:", false, taxOverride_fromGroup.AO_DebtorRole_ReadOnly);
		}

		public void TestChangeCostSellUpdatesSplitPaymentVATOrg()
		{
			AccChargeCode charge = Factory.New<AccChargeCode>();
			AccChargeTaxOverride @override = charge.TaxOverrides.AddNew();
			@override.AO_CostSellAll = "REV";
			@override.AO_SplitPaymentVATOrganisation = true;
			Assert(@override.AO_SplitPaymentVATOrganisation);
			@override.AO_CostSellAll = "CST";
			Assert(!@override.AO_SplitPaymentVATOrganisation);
			@override.AO_CostSellAll = "REV";
			@override.AO_SplitPaymentVATOrganisation = true;
			Assert(@override.AO_SplitPaymentVATOrganisation);
			@override.AO_CostSellAll = "ALL";
			Assert(!@override.AO_SplitPaymentVATOrganisation);
		}

		public void TestAO_SplitPaymentVATOrganisation_ReadOnly()
		{
			AccChargeCode charge = Factory.New<AccChargeCode>();
			AccChargeTaxOverride @override = charge.TaxOverrides.AddNew();
			@override.AO_CostSellAll = "REV";
			Assert(!@override.AO_SplitPaymentVATOrganisationInfo.ReadOnly);
			@override.AO_CostSellAll = "CST";
			Assert(@override.AO_SplitPaymentVATOrganisationInfo.ReadOnly);
			@override.AO_CostSellAll = "ALL";
			Assert(@override.AO_SplitPaymentVATOrganisationInfo.ReadOnly);
		}

		public void TestAO_DebtorRole_ReadOnly()
		{
			AccChargeCode charge = Factory.New<AccChargeCode>();
			AccChargeTaxOverride @override = charge.TaxOverrides.AddNew();
			@override.AO_CostSellAll = "REV";
			@override.AO_JobType = "SHP";
			Assert(!@override.AO_DebtorRoleInfo.ReadOnly);
			@override.AO_CostSellAll = "CST";
			Assert(@override.AO_DebtorRoleInfo.ReadOnly);
			@override.AO_CostSellAll = "REV";
			@override.AO_JobType = "FCN";
			Assert(@override.AO_DebtorRoleInfo.ReadOnly);
		}

		public void TestChangeCostSellOrJobTypeResetDebtorRole()
		{
			AccChargeCode charge = Factory.New<AccChargeCode>();
			AccChargeTaxOverride @override = charge.TaxOverrides.AddNew();
			@override.AO_CostSellAll = "REV";
			@override.AO_JobType = "SHP";
			@override.AO_DebtorRole = "AGT";

			@override.AO_CostSellAll = "CST";
			Assert(@override.AO_DebtorRole.IsEmpty);
			@override.AO_CostSellAll = "REV";
			@override.AO_DebtorRole = "AGT";

			@override.AO_JobType = "FCN";
			Assert(@override.AO_DebtorRole.IsEmpty);
		}

		public void TestAT_And_DefaultVATClass_ReadOnly()
		{
			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			AccChargeTaxOverride chargeTaxOverride = chargeCode.TaxOverrides.AddNew();

			chargeTaxOverride.AO_CreateTaxRecord = true;
			Assert(!chargeTaxOverride.AO_ATInfo.ReadOnly);
			Assert(!chargeTaxOverride.AO_A9_DefaultVATClassInfo.ReadOnly);

			chargeTaxOverride.AO_CreateTaxRecord = false;
			Assert(chargeTaxOverride.AO_ATInfo.ReadOnly);
			Assert(chargeTaxOverride.AO_A9_DefaultVATClassInfo.ReadOnly);

			chargeTaxOverride.AO_CreateTaxRecord = true;
			chargeTaxOverride.AO_DefaultingRule = Core.Constants.TaxOverrideDefaultingRule.Codes.CopyARAmount;
			Assert(chargeTaxOverride.AO_ATInfo.ReadOnly);
			Assert(!chargeTaxOverride.AO_A9_DefaultVATClassInfo.ReadOnly);
		}

		public void TestAO_CreateTaxRecord_SetSameValueTwice()
		{
			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			AccChargeTaxOverride chargeTaxOverride = chargeCode.TaxOverrides.AddNew();

			AccTaxRate rate = Factory.LoadTop1<AccTaxRate>(new ZQuery());
			AccInvMsg taxOverride = Factory.NewWithValidTestData<AccInvMsg>();

			chargeTaxOverride.AO_AT = rate.PK;
			chargeTaxOverride.AO_A9_DefaultVATClass = taxOverride.PK;
			chargeTaxOverride.AO_CreateTaxRecord = true;
			chargeTaxOverride.AO_CreateTaxRecord = true;
			AssertEquals(rate.PK, chargeTaxOverride.AO_AT);
			AssertEquals(taxOverride.PK, chargeTaxOverride.AO_A9_DefaultVATClass);

			chargeTaxOverride.AO_CreateTaxRecord = false;
			chargeTaxOverride.AO_CreateTaxRecord = false;
			AssertEquals(ZGuid.Empty, chargeTaxOverride.AO_AT);
			AssertEquals(ZGuid.Empty, chargeTaxOverride.AO_A9_DefaultVATClass);

			chargeTaxOverride.AO_CreateTaxRecord = true;
			AssertEquals(rate.PK, chargeTaxOverride.AO_AT);
			AssertEquals(taxOverride.PK, chargeTaxOverride.AO_A9_DefaultVATClass);
		}

		public void TestChangeCreateTaxRecordUpdateAT_and_DefaultVATClass()
		{
			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			AccChargeTaxOverride chargeTaxOverride = chargeCode.TaxOverrides.AddNew();

			AccTaxRate rate = Factory.LoadTop1<AccTaxRate>(new ZQuery());

			AssertAO_AT(rate.PK);
			AssertAO_AT(ZGuid.Empty);

			void AssertAO_AT(ZGuid expectedValue)
			{
				chargeTaxOverride.AO_AT = expectedValue;

				chargeTaxOverride.AO_CreateTaxRecord = false;
				AssertEquals(ZGuid.Empty, chargeTaxOverride.AO_AT);

				chargeTaxOverride.AO_CreateTaxRecord = true;
				AssertEquals(expectedValue, chargeTaxOverride.AO_AT);
			}

			AccInvMsg taxOverride = Factory.NewWithValidTestData<AccInvMsg>();
			AssertAO_A9_DefaultVATClass(taxOverride.PK);
			AssertAO_A9_DefaultVATClass(ZGuid.Empty);

			void AssertAO_A9_DefaultVATClass(ZGuid expectedValue)
			{
				chargeTaxOverride.AO_A9_DefaultVATClass = expectedValue;

				chargeTaxOverride.AO_CreateTaxRecord = false;
				AssertEquals(ZGuid.Empty, chargeTaxOverride.AO_A9_DefaultVATClass);

				chargeTaxOverride.AO_CreateTaxRecord = true;
				AssertEquals(expectedValue, chargeTaxOverride.AO_A9_DefaultVATClass);
			}
		}

		public void TestChangeAO_DefaultingRule_UpdateAT()
		{
			var chargeTaxOverride = Factory.New<AccChargeTaxOverride>();

			AccTaxRate rate = Factory.LoadTop1<AccTaxRate>(new ZQuery());

			chargeTaxOverride.AO_AT = rate.PK;
			chargeTaxOverride.AO_DefaultingRule = Core.Constants.TaxOverrideDefaultingRule.Codes.SumARAmount;
			AssertEquals(rate.PK, chargeTaxOverride.AO_AT);

			chargeTaxOverride.AO_DefaultingRule = Core.Constants.TaxOverrideDefaultingRule.Codes.CopyARAmount;
			AssertEquals(ZGuid.Empty, chargeTaxOverride.AO_AT);

			chargeTaxOverride.AO_DefaultingRule = Core.Constants.TaxOverrideDefaultingRule.Codes.SumARAmount;
			AssertEquals(rate.PK, chargeTaxOverride.AO_AT);
		}

		public void TestDirectionDefaultsOriginAndDestination()
		{
			RefCountry currentCountry = GlbCompany.CurrentCompany.Country;
			ZQuery findOtherCountryQuery = new ZQuery(RefCountrySchema.PK, SQLComparisonOperator.NotEqual, currentCountry.PK);
			RefCountry otherCountry = Factory.LoadTop1<RefCountry>(findOtherCountryQuery);

			AccChargeCode charge = Factory.New<AccChargeCode>();
			AccChargeTaxOverride @override = charge.TaxOverrides.AddNew();
			AssertFields(@override, "", "", "");

			@override.AO_Direction = "ALL";
			AssertFields(@override, "ALL", "ALL", "ALL");

			@override.AO_Direction = "IMP";
			AssertFields(@override, "IMP", "ALL", currentCountry.Code);

			@override.AO_Direction = "EXP";
			AssertFields(@override, "EXP", currentCountry.Code, "ALL");

			@override.AO_Direction = "DOM";
			AssertFields(@override, "DOM", currentCountry.Code, currentCountry.Code);

			@override.AO_Direction = "ALL";
			AssertFields(@override, "ALL", "ALL", "ALL");
		}

		public void TestOriginDefaultsDirectionAndDestination()
		{
			RefCountry currentCountry = GlbCompany.CurrentCompany.Country;
			ZQuery findOtherCountryQuery = new ZQuery(RefCountrySchema.PK, SQLComparisonOperator.NotEqual, currentCountry.PK);
			RefCountry otherCountry = Factory.LoadTop1<RefCountry>(findOtherCountryQuery);

			AccChargeCode charge = Factory.New<AccChargeCode>();
			AccChargeTaxOverride @override = charge.TaxOverrides.AddNew();
			AssertFields(@override, "", "", "");

			@override.AO_Origin = "ALL";
			AssertFields(@override, "", "ALL", "");

			SetFields(@override, "", "", "");
			@override.AO_Origin = currentCountry.Code;
			AssertFields(@override, "EXP", currentCountry.Code, "ALL");

			SetFields(@override, "IMP", "", "");
			@override.AO_Origin = currentCountry.Code;
			AssertFields(@override, "IMP", currentCountry.Code, "");

			SetFields(@override, "", "", "");
			@override.AO_Origin = otherCountry.Code;
			AssertFields(@override, "", otherCountry.Code, "");
		}

		public void TestDestinationDefaultsDirectionAndOrigin()
		{
			RefCountry currentCountry = GlbCompany.CurrentCompany.Country;
			ZQuery findOtherCountryQuery = new ZQuery(RefCountrySchema.PK, SQLComparisonOperator.NotEqual, currentCountry.PK);
			RefCountry otherCountry = Factory.LoadTop1<RefCountry>(findOtherCountryQuery);

			AccChargeCode charge = Factory.New<AccChargeCode>();
			AccChargeTaxOverride @override = charge.TaxOverrides.AddNew();
			AssertFields(@override, "", "", "");

			@override.AO_Destination = "ALL";
			AssertFields(@override, "", "", "ALL");

			SetFields(@override, "", "", "");
			@override.AO_Destination = currentCountry.Code;
			AssertFields(@override, "IMP", "ALL", currentCountry.Code);

			SetFields(@override, "EXP", "", "");
			@override.AO_Destination = currentCountry.Code;
			AssertFields(@override, "EXP", "", currentCountry.Code);

			SetFields(@override, "", "", "");
			@override.AO_Destination = otherCountry.Code;
			AssertFields(@override, "", "", otherCountry.Code);
		}

		public void TestReadonlyTransportMode()
		{
			AccChargeCode charge = Factory.New<AccChargeCode>();
			AccChargeTaxOverride @override = charge.TaxOverrides.AddNew();
			AssertFields(@override, "", "", "");

			@override.AO_Destination = "ALL";
			AssertFields(@override, "", "", "ALL");

			SetFields(@override, "", "", "");
			foreach (JobInvoicingConsumerType jobType in JobInvoicingConsumerTypes.NewOnlyJobInvoicingTypes())
			{
				@override.AO_JobType = "ALL"; //Before editing transport mode, the job type must be supported
				@override.AO_TransportMode = "AIR";
				AssertEquals("TransportMode Value [" + jobType + "]", "AIR", @override.AO_TransportMode);

				@override.AO_JobType = jobType.Code;
				if (@override.JobType != null && @override.JobType.IsTransportModeSupported)
				{
					AssertEquals("ReadOnly [" + jobType + "]", false, @override.AO_TransportModeInfo.ReadOnly);
					AssertEquals("TransportMode Value [" + jobType + "]", "AIR", @override.AO_TransportMode);
				}
				else
				{
					AssertEquals("ReadOnly [" + jobType + "]", true, @override.AO_TransportModeInfo.ReadOnly);
					AssertEquals("TransportMode Value [" + jobType + "]", "ALL", @override.AO_TransportMode);
				}
			}
		}

		public void TestReadonlyColumnsForNJRJobType()
		{
			var charge = Factory.New<AccChargeCode>();
			var chargeTaxOverride = charge.TaxOverrides.AddNew();
			chargeTaxOverride.AO_CostSellAll = AccChargeTaxOverrideLookups.Revenue;
			chargeTaxOverride.AO_TransactionContext = "INT";
			chargeTaxOverride.AO_DefaultingRule = "SUM";

			var expectedColumnsAndValues = new Dictionary<string, IZType>();
			expectedColumnsAndValues.Add(chargeTaxOverride.AO_DirectionInfo.Name, (ZString)"ALL");
			expectedColumnsAndValues.Add(chargeTaxOverride.AO_TransportModeInfo.Name, (ZString)"ALL");
			expectedColumnsAndValues.Add(chargeTaxOverride.AO_IncoTermInfo.Name, (ZString)"ALL");
			expectedColumnsAndValues.Add(chargeTaxOverride.AO_OriginInfo.Name, (ZString)"ALL");
			expectedColumnsAndValues.Add(chargeTaxOverride.AO_DestinationInfo.Name, (ZString)"ALL");
			expectedColumnsAndValues.Add(chargeTaxOverride.AO_CustomsStatusInfo.Name, ZString.Empty);
			expectedColumnsAndValues.Add(chargeTaxOverride.AO_DebtorRoleInfo.Name, ZString.Empty);

			chargeTaxOverride.AO_JobType = AccountingMasterFilesConstants.JobTypes.NonJobRelated;
			AssertReadonlyness(chargeTaxOverride, expectedColumnsAndValues.Keys, true);
			AssertValues(chargeTaxOverride, expectedColumnsAndValues, true);

			chargeTaxOverride.AO_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			chargeTaxOverride.AO_CostSellAll = AccChargeTaxOverrideLookups.Revenue;
			chargeTaxOverride.AO_Direction = "IMP";
			chargeTaxOverride.AO_TransportMode = "AIR";
			chargeTaxOverride.AO_IncoTerm = "FOB";
			chargeTaxOverride.AO_Origin = "NEU";
			chargeTaxOverride.AO_Destination = "EU";
			chargeTaxOverride.AO_CustomsStatus = "T1";
			chargeTaxOverride.AO_DebtorRole = "AGT";
			AssertReadonlyness(chargeTaxOverride, expectedColumnsAndValues.Keys, false);
			AssertValues(chargeTaxOverride, expectedColumnsAndValues, false);

			chargeTaxOverride.AO_JobType = AccountingMasterFilesConstants.JobTypes.NonJobRelated;
			AssertReadonlyness(chargeTaxOverride, expectedColumnsAndValues.Keys, true);
			AssertValues(chargeTaxOverride, expectedColumnsAndValues, true);
		}

		public void TestOriginAndDestinationPropertiesAreReadOnlyWhenDirectionIsReadOnly()
		{
			var charge = Factory.New<AccChargeCode>();
			var chargeTaxOverride = charge.TaxOverrides.AddNew();

			chargeTaxOverride.AO_JobType = JobInvoicingConsumerTypes.PostClearanceBrokerageCode;
			Assert(chargeTaxOverride.AO_DirectionInfo.ReadOnly);
			Assert(chargeTaxOverride.AO_OriginInfo.ReadOnly);
			Assert(chargeTaxOverride.AO_DestinationInfo.ReadOnly);

			chargeTaxOverride.AO_JobType = JobInvoicingConsumerTypes.ShipmentCode;
			Assert(!chargeTaxOverride.AO_DirectionInfo.ReadOnly);
			Assert(!chargeTaxOverride.AO_OriginInfo.ReadOnly);
			Assert(!chargeTaxOverride.AO_DestinationInfo.ReadOnly);
		}

		public void TestAO_DefaultingRule_ReadOnly()
		{
			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			AccChargeTaxOverride chargeTaxOverride = chargeCode.TaxOverrides.AddNew();

			chargeTaxOverride.AO_TransactionContext = Core.Constants.TaxOverrideTransactionContext.Codes.IntercompanyInvoiceImport;
			Assert(!chargeTaxOverride.AO_DefaultingRuleInfo.ReadOnly);

			chargeTaxOverride.AO_TransactionContext = Core.Constants.TaxOverrideTransactionContext.Codes.Standard;
			Assert(chargeTaxOverride.AO_DefaultingRuleInfo.ReadOnly);

			chargeTaxOverride.AO_TransactionContext = Core.Constants.TaxOverrideTransactionContext.Codes.All;
			Assert(chargeTaxOverride.AO_DefaultingRuleInfo.ReadOnly);
		}

		public void TestChangeTransactionContext_UpdateDefaultingRule()
		{
			var chargeTaxOverride = Factory.New<AccChargeTaxOverride>();

			AssertEquals("Precondition", Core.Constants.TaxOverrideDefaultingRule.Codes.NotApplicable, chargeTaxOverride.AO_DefaultingRule);
			AssertEquals("Precondition", Core.Constants.TaxOverrideTransactionContext.Codes.Standard, chargeTaxOverride.AO_TransactionContext);

			chargeTaxOverride.AO_TransactionContext = Core.Constants.TaxOverrideTransactionContext.Codes.IntercompanyInvoiceImport;
			AssertEquals(Core.Constants.TaxOverrideDefaultingRule.Codes.CopyARAmount, chargeTaxOverride.AO_DefaultingRule);

			chargeTaxOverride.AO_DefaultingRule = Core.Constants.TaxOverrideDefaultingRule.Codes.SumARAmount;
			chargeTaxOverride.AO_TransactionContext = Core.Constants.TaxOverrideTransactionContext.Codes.Standard;
			AssertEquals(Core.Constants.TaxOverrideDefaultingRule.Codes.NotApplicable, chargeTaxOverride.AO_DefaultingRule);

			chargeTaxOverride.AO_TransactionContext = Core.Constants.TaxOverrideTransactionContext.Codes.IntercompanyInvoiceImport;
			AssertEquals(Core.Constants.TaxOverrideDefaultingRule.Codes.SumARAmount, chargeTaxOverride.AO_DefaultingRule);
		}

		public void TestAO_SupplyType()
		{
			var chargeTaxOverride = Factory.New<AccChargeTaxOverride>();
			AssertEquals("Default is Empty", ZString.Empty, chargeTaxOverride.AO_SupplyType);

			chargeTaxOverride.AO_SupplyType = "DSB";
			AssertEquals("Setter is simple setting", "DSB", chargeTaxOverride.AO_SupplyType);

			chargeTaxOverride.AO_SupplyType = "BBB";
			AssertEquals("Setter is simple setting", "BBB", chargeTaxOverride.AO_SupplyType);

			chargeTaxOverride.AO_SupplyType = ZString.Empty;
			AssertEquals("Setter is simple setting", ZString.Empty, chargeTaxOverride.AO_SupplyType);
		}

		public void TestAO_Direction_Readonly()
		{
			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			AccChargeTaxOverride chargeTaxOverride = chargeCode.TaxOverrides.AddNew();

			chargeTaxOverride.AO_JobType = "SHP";
			Assert("AO_Direction should not be readonly", !chargeTaxOverride.AO_Direction_ReadOnly);

			chargeTaxOverride.AO_JobType = "BRK";
			Assert("AO_Direction should not be readonly", !chargeTaxOverride.AO_Direction_ReadOnly);

			chargeTaxOverride.AO_JobType = "NJR";
			Assert("AO_Direction should be readonly", chargeTaxOverride.AO_Direction_ReadOnly);

			chargeTaxOverride.AO_JobType = "WKI";
			Assert("AO_Direction should be readonly", chargeTaxOverride.AO_Direction_ReadOnly);
		}

		public void TestAO_Direction_Value()
		{
			AccChargeCode charge = Factory.New<AccChargeCode>();
			AccChargeTaxOverride chargeTaxOverride = charge.TaxOverrides.AddNew();

			chargeTaxOverride.AO_JobType = "WKI";
			AssertEquals("AO_Direction value should be ALL", "ALL", chargeTaxOverride.AO_Direction);

			chargeTaxOverride.AO_JobType = "NJR";
			AssertEquals("AO_Direction value should be ALL", "ALL", chargeTaxOverride.AO_Direction);

			chargeTaxOverride.AO_JobType = "BRK";
			AssertEquals("AO_Direction value should be ALL", "ALL", chargeTaxOverride.AO_Direction);
		}
		
		#region Test for Tax Id And Tax Message Mapping Validation

			public void TestTaxIDAndTaxMessageMappingValidation_WhenSetTaxRate()
		{
			var taxOverride = Factory.NewWithValidTestData<AccChargeTaxOverride>();
			var taxRate1 = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate1.AT_Code = "TaxRate01";
			var taxRate2 = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate2.AT_Code = "TaxRate02";
			var taxMsg1 = Factory.NewWithValidTestData<AccInvMsg>();
			taxMsg1.A9_Code = "TaxMsg01";
			var taxMsg2 = Factory.NewWithValidTestData<AccInvMsg>();
			taxMsg2.A9_Code = "TaxMsg02";
			Factory.Save();

			taxOverride.AO_CostSellAll = AccChargeTaxOverrideLookups.Revenue;
			var config = TestObjectCreator.CreateTaxIdAndTaxMessageCombinationRulesConfiguration(
				(TransactionLineTypes.Revenue, taxRate1, taxMsg1),
				(TransactionLineTypes.Revenue, taxRate2, taxMsg2));
			AccountingMasterFilesRegistry.Instance.TaxIdAndTaxMessageCombinationRules.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, config);

			taxOverride.AO_A9_DefaultVATClass = taxMsg1.PK;
			taxOverride.AO_AT = taxRate1.PK;
			AssertNoErrors(taxOverride.AO_A9_DefaultVATClassInfo);

			taxOverride.AO_AT = taxRate2.PK;
			AssertHasErrors("Error Mapping", taxOverride.AO_A9_DefaultVATClassInfo);
		}

		#endregion

		#region Implementation

		static void AssertReadonlyness(AccChargeTaxOverride chargeTaxOverride, IEnumerable<string> expectedFields, bool isReadonly)
		{
			if (isReadonly)
			{
				AssertContainsExactElementsInAnyOrder("All read-only properties should be from expected list.", expectedFields, chargeTaxOverride.ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(x => x.ReadOnly).Select(x => x.Name));
			}
			else
			{
				AssertContainsExactElementsInAnyOrder("All properties from expected list should not be read-only.", expectedFields, chargeTaxOverride.ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(x => expectedFields.Contains(x.Name) && !x.ReadOnly).Select(x => x.Name));
			}
		}

		static void AssertValues(AccChargeTaxOverride chargeTaxOverride, Dictionary<string, IZType> expectedColumnsAndValues, bool isValuesEqual)
		{
			if (isValuesEqual)
			{
				AssertContainsExactElementsInAnyOrder("All expected properties must have values different to expected.", expectedColumnsAndValues.Keys, chargeTaxOverride.ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(x => expectedColumnsAndValues.ContainsKey(x.Name) && x.Value.CompareTo(expectedColumnsAndValues[x.Name]) == 0).Select(x => x.Name));
			}
			else
			{
				AssertContainsExactElementsInAnyOrder("All expected properties must have the expected values.", expectedColumnsAndValues.Keys, chargeTaxOverride.ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(x => expectedColumnsAndValues.ContainsKey(x.Name) && x.Value.CompareTo(expectedColumnsAndValues[x.Name]) != 0).Select(x => x.Name));
			}
		}

		static void SetFields(AccChargeTaxOverride chargeTaxOverride, ZString direction, ZString origin, ZString destination)
		{
			using (chargeTaxOverride.GetDefaultingSuspender())
			{
				chargeTaxOverride.AO_Direction = direction;
				chargeTaxOverride.AO_Origin = origin;
				chargeTaxOverride.AO_Destination = destination;
			}
		}

		static void AssertFields(AccChargeTaxOverride chargeTaxOverride, ZString direction, ZString origin, ZString destination)
		{
			var expectedColumnsAndValues = new Dictionary<string, IZType>();
			expectedColumnsAndValues.Add(chargeTaxOverride.AO_DirectionInfo.Name, direction);
			expectedColumnsAndValues.Add(chargeTaxOverride.AO_OriginInfo.Name, origin);
			expectedColumnsAndValues.Add(chargeTaxOverride.AO_DestinationInfo.Name, destination);

			AssertValues(chargeTaxOverride, expectedColumnsAndValues, true);
		}

		#endregion

		AccountingTestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new AccountingTestObjectCreator(Factory));
		AccountingTestObjectCreator testObjectCreator;
	}
}
