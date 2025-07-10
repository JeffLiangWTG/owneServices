using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;

namespace Enterprise.MasterFiles.Business.Testing.Accounting
{
	[TestedType(typeof(AccPOSConfiguration))]
	class AccPOSConfigurationTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var config = Factory.New<AccPOSConfiguration>();
			AssertEquals(ZGuid.Empty, config.PSC_GC);
			AssertEquals(ZString.Empty, config.PSC_Ledger);
			AssertEquals(AccPOSChargeTypeList.Codes.CostAndRevenue, config.PSC_ChargeType);
			AssertEquals(ZString.Empty, config.PSC_ParentTableCode);
			AssertEquals(ZGuid.Empty, config.PSC_ParentId);
			AssertEquals(JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All, config.PSC_JobType);
			AssertEquals(ZString.Empty, config.PSC_IncoTerm);
			AssertEquals(Constants.FreightShipmentDirection.Code.All, config.PSC_ServiceDirection);
			AssertEquals(JobConfigurationSelectorLookups.ModeAdditionalCodes.All, config.PSC_TransportMode);
			AssertEquals(ZString.Empty, config.PSC_TaxRegistrationType);
			AssertEquals(ZString.Empty, config.PSC_NK_Branch);
			AssertEquals(ZString.Empty, config.PSC_SupplyType);
			AssertEquals(ZString.Empty, config.PSC_PlaceOfSupplyRule);
		}

		public void TestChargeType()
		{
			var config = CreateBusinessObjectForTest();

			config.PSC_ChargeType = AccPOSChargeTypeList.Codes.CostAndRevenue;
			AssertEquals(ZString.Empty, config.PSC_Ledger);

			config.PSC_ChargeType = AccPOSChargeTypeList.Codes.Revenue;
			AssertEquals(LedgerTypes.AccountsReceivable, config.PSC_Ledger);

			config.PSC_ChargeType = AccPOSChargeTypeList.Codes.Cost;
			AssertEquals(LedgerTypes.AccountsPayable, config.PSC_Ledger);

			config.PSC_ChargeType = "XYZ";
			AssertEquals("An invalid ChargeType should be persisted as an invalid Ledger", "..", config.PSC_Ledger);
			AssertEquals("An invalid ChargeType should be persisted without loss", "XYZ", config.PSC_ChargeType);
			config.PSC_ChargeType = AccPOSChargeTypeList.Codes.CostAndRevenue;

			config.PSC_Ledger = LedgerTypes.AccountsPayable;
			AssertEquals(AccPOSChargeTypeList.Codes.Cost, config.PSC_ChargeType);

			config.PSC_Ledger = LedgerTypes.AccountsReceivable;
			AssertEquals(AccPOSChargeTypeList.Codes.Revenue, config.PSC_ChargeType);

			config.PSC_Ledger = ZString.Empty;
			AssertEquals(AccPOSChargeTypeList.Codes.CostAndRevenue, config.PSC_ChargeType);

			config.PSC_Ledger = LedgerTypes.CashBook;
			AssertEquals("An invalid Ledger should not impact the ChargeType value", "", config.PSC_ChargeType);
		}

		public void TestCompany()
		{
			var config = CreateBusinessObjectForTest();

			config.PSC_GC = Env.CurrentCompany.PK;
			AssertNotNull(config.Company);
			AssertEquals(Env.CurrentCompany.PK, config.Company.PK);
			AssertEquals(Env.CurrentCompany.Code, config.Company.GC_Code);

			config.PSC_ParentTableCode = "AH";
			AssertNotNull("Company always uses PSC_GC and should ignore ParentTableCode", config.Company);
			AssertEquals(Env.CurrentCompany.PK, config.Company.PK);

			var newCompany = Factory.NewWithValidTestData<GlbCompany>();
			newCompany.GC_Code = "TST";
			config.PSC_GC = newCompany.PK;
			AssertNotNull(config.Company);
			AssertEquals(newCompany.PK, config.Company.PK);
			AssertEquals("TST", config.Company.GC_Code);

			config.PSC_GC = ZGuid.NewZGuid();
			AssertNull(config.Company);

			config.PSC_GC = ZGuid.Empty;
			AssertNull(config.Company);
		}

		public void TestChargeCode()
		{
			var config = CreateBusinessObjectForTest();
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "LVLTST";

			config.PSC_ParentTableCode = "AH";
			config.PSC_ParentId = chargeCode.PK;
			AssertNull("ChargeCode should be null when ParentTableCode is not AC", config.ChargeCode);

			config.PSC_ParentTableCode = AccChargeCodeSchema.Constants.Prefix;
			AssertNotNull(config.ChargeCode);
			AssertEquals(chargeCode.PK, config.ChargeCode.PK);
			AssertEquals("LVLTST", config.ChargeCode.AC_Code);

			config.PSC_ParentId = ZGuid.NewZGuid();
			AssertNull(config.ChargeCode);

			config.PSC_ParentId = ZGuid.Empty;
			AssertNull(config.ChargeCode);
		}

		public void TestChargeCodeGroup()
		{
			var config = CreateBusinessObjectForTest();
			var group = Factory.NewWithValidTestData<AccPOSChargeCodeGroup>();
			group.GRO_Code = "LVLTST";

			config.PSC_ParentTableCode = "AH";
			config.PSC_ParentId = group.PK;
			AssertNull("ChargeCodeGroup should be null when ParentTableCode is not GRO", config.ChargeCodeGroup);

			config.PSC_ParentTableCode = AccPOSChargeCodeGroupViewSchema.Constants.Prefix;
			AssertNotNull(config.ChargeCodeGroup);
			AssertEquals(group.PK, config.ChargeCodeGroup.PK);
			AssertEquals("LVLTST", config.ChargeCodeGroup.GRO_Code);

			config.PSC_ParentId = ZGuid.NewZGuid();
			AssertNull(config.ChargeCodeGroup);

			config.PSC_ParentId = ZGuid.Empty;
			AssertNull(config.ChargeCodeGroup);
		}

		public void TestLevel()
		{
			var config = CreateBusinessObjectForTest();

			config.PSC_ParentTableCode = ZString.Empty;
			AssertEquals(AccPOSConfigurationLevel.Company, config.Level);

			config.PSC_ParentTableCode = AccPOSChargeCodeGroupViewSchema.Constants.Prefix;
			AssertEquals(AccPOSConfigurationLevel.ChargeCodeGroup, config.Level);

			config.PSC_ParentTableCode = AccChargeCodeSchema.Constants.Prefix;
			AssertEquals(AccPOSConfigurationLevel.ChargeCode, config.Level);

			config.PSC_ParentTableCode = "00";
			AssertEquals(AccPOSConfigurationLevel.Null, config.Level);
		}

		public void TestLevelName()
		{
			var config = CreateBusinessObjectForTest();

			config.PSC_ParentTableCode = ZString.Empty;
			AssertEquals("Company", config.LevelName);

			config.PSC_ParentTableCode = AccPOSChargeCodeGroupViewSchema.Constants.Prefix;
			AssertEquals("Charge Code Group", config.LevelName);

			config.PSC_ParentTableCode = AccChargeCodeSchema.Constants.Prefix;
			AssertEquals("Charge Code", config.LevelName);

			config.PSC_ParentTableCode = "00";
			AssertEquals(string.Empty, config.LevelName);
		}

		public void TestLevelCode()
		{
			var config = CreateBusinessObjectForTest();

			config.PSC_ParentTableCode = ZString.Empty;
			AssertEquals(Env.CurrentCompany.Code, config.LevelCode);

			var group = Factory.NewWithValidTestData<AccPOSChargeCodeGroup>();
			group.GRO_Code = "LVLTSTGRP";
			config.PSC_ParentId = group.PK;
			config.PSC_ParentTableCode = AccPOSChargeCodeGroupViewSchema.Constants.Prefix;
			AssertEquals("LVLTSTGRP", config.LevelCode);

			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Code = "LVLTST";
			config.PSC_ParentId = chargeCode.PK;
			config.PSC_ParentTableCode = AccChargeCodeSchema.Constants.Prefix;
			AssertEquals("LVLTST", config.LevelCode);

			config.PSC_ParentTableCode = "00";
			AssertEquals(string.Empty, config.LevelCode);
		}

		public void TestReadOnly()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			var group = Factory.NewWithValidTestData<AccPOSChargeCodeGroup>();
			var config = CreateBusinessObjectForTest();
			var companyConfig = CreateBusinessObjectForTest();
			var groupConfig = CreateBusinessObjectForTest();
			groupConfig.PSC_ParentTableCode = AccPOSChargeCodeGroupViewSchema.Constants.Prefix;
			groupConfig.PSC_ParentId = group.PK;
			var chargeCodeConfig = CreateBusinessObjectForTest();
			chargeCodeConfig.PSC_ParentTableCode = AccChargeCodeSchema.Constants.Prefix;
			chargeCodeConfig.PSC_ParentId = chargeCode.PK;

			Assert("Config outside of collection is always editable", !config.ReadOnly);

			var companyCollection = new AccPOSConfigurationCollection(GlbCompany.CurrentCompany);
			companyCollection.Add(companyConfig);
			Assert("Company level config in company collection is editable", !companyConfig.ReadOnly);
			companyCollection.RemoveAll();

			var groupCollection = new AccPOSConfigurationCollection(group);
			groupCollection.Add(companyConfig);
			groupCollection.Add(groupConfig);
			Assert("Company level config in group collection is read only", companyConfig.ReadOnly);
			Assert("Group level config in group collection is editable", !groupConfig.ReadOnly);
			groupCollection.RemoveAll();

			var chargeCodeCollection = new AccPOSConfigurationCollection(chargeCode);
			chargeCodeCollection.Add(companyConfig);
			chargeCodeCollection.Add(groupConfig);
			chargeCodeCollection.Add(chargeCodeConfig);
			Assert("Company level config in charge code collection is read only", companyConfig.ReadOnly);
			Assert("Group level config in charge code collection is read only", groupConfig.ReadOnly);
			Assert("Charge code level config in charge code collection is editable", !chargeCodeConfig.ReadOnly);
			chargeCodeCollection.RemoveAll();
		}

		public void TestLookupKeyAndToString()
		{
			var config = CreateBusinessObjectForTest();
			AssertEquals("EDI|Company:EDI|ALL|ALL||ALL|ALL|||", config.LookupKey);
			AssertEquals("EDI|Company:EDI|ALL|ALL||ALL|ALL||| > BIL", config.ToString());

			config.PSC_JobType = JobInvoicingConsumerTypes.ShipmentCode;
			AssertEquals("EDI|Company:EDI|SHP|ALL||ALL|ALL|||", config.LookupKey);
			AssertEquals("EDI|Company:EDI|SHP|ALL||ALL|ALL||| > BIL", config.ToString());
			config.PSC_JobType = JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All;

			config.PSC_ChargeType = AccPOSChargeTypeList.Codes.Cost;
			AssertEquals("EDI|Company:EDI|ALL|COS||ALL|ALL|||", config.LookupKey);
			AssertEquals("EDI|Company:EDI|ALL|COS||ALL|ALL||| > BIL", config.ToString());
			config.PSC_ChargeType = AccPOSChargeTypeList.Codes.CostAndRevenue;

			config.PSC_IncoTerm = Constants.IncoTerms.FreeOnBoard;
			AssertEquals("EDI|Company:EDI|ALL|ALL|FOB|ALL|ALL|||", config.LookupKey);
			AssertEquals("EDI|Company:EDI|ALL|ALL|FOB|ALL|ALL||| > BIL", config.ToString());
			config.PSC_IncoTerm = ZString.Empty;

			config.PSC_ServiceDirection = Constants.FreightShipmentDirection.Code.Export;
			AssertEquals("EDI|Company:EDI|ALL|ALL||EXP|ALL|||", config.LookupKey);
			AssertEquals("EDI|Company:EDI|ALL|ALL||EXP|ALL||| > BIL", config.ToString());
			config.PSC_ServiceDirection = Constants.FreightShipmentDirection.Code.All;

			config.PSC_TransportMode = Constants.TransportModes.Sea;
			AssertEquals("EDI|Company:EDI|ALL|ALL||ALL|SEA|||", config.LookupKey);
			AssertEquals("EDI|Company:EDI|ALL|ALL||ALL|SEA||| > BIL", config.ToString());
			config.PSC_TransportMode = Constants.TransportModes.All;

			config.PSC_TaxRegistrationType = AccPOSTaxRegistrationList.Codes.LocalCustomer;
			AssertEquals("EDI|Company:EDI|ALL|ALL||ALL|ALL|LOC||", config.LookupKey);
			AssertEquals("EDI|Company:EDI|ALL|ALL||ALL|ALL|LOC|| > BIL", config.ToString());
			config.PSC_TaxRegistrationType = ZString.Empty;

			config.PSC_NK_Branch = GlbBranch.CurrentBranch.GB_Code;
			AssertEquals("EDI|Company:EDI|ALL|ALL||ALL|ALL||BNE|", config.LookupKey);
			AssertEquals("EDI|Company:EDI|ALL|ALL||ALL|ALL||BNE| > BIL", config.ToString());
			config.PSC_NK_Branch = ZString.Empty;

			config.PSC_SupplyType = SupplyTypeClassificationCodes.LOC;
			AssertEquals("EDI|Company:EDI|ALL|ALL||ALL|ALL|||LOC", config.LookupKey);
			AssertEquals("EDI|Company:EDI|ALL|ALL||ALL|ALL|||LOC > BIL", config.ToString());
			config.PSC_SupplyType = ZString.Empty;

			config.PSC_PlaceOfSupplyRule = AccPOSRuleList.Codes.SupplierLocation;
			AssertEquals("EDI|Company:EDI|ALL|ALL||ALL|ALL|||", config.LookupKey);
			AssertEquals("EDI|Company:EDI|ALL|ALL||ALL|ALL||| > SUP", config.ToString());
			config.PSC_PlaceOfSupplyRule = AccPOSRuleList.Codes.BillToPartyLocation;

			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "TSTCOD";
			config.PSC_ParentTableCode = AccChargeCodeSchema.Constants.Prefix;
			config.PSC_ParentId = chargeCode.PK;
			AssertEquals("EDI|ChargeCode:TSTCOD|ALL|ALL||ALL|ALL|||", config.LookupKey);
			AssertEquals("EDI|ChargeCode:TSTCOD|ALL|ALL||ALL|ALL||| > BIL", config.ToString());

			var group = Factory.NewWithValidTestData<AccPOSChargeCodeGroup>();
			group.GRO_Code = "TSTGRP";
			config.PSC_ParentTableCode = AccPOSChargeCodeGroupViewSchema.Constants.Prefix;
			config.PSC_ParentId = group.PK;
			AssertEquals("EDI|ChargeCodeGroup:TSTGRP|ALL|ALL||ALL|ALL|||", config.LookupKey);
			AssertEquals("EDI|ChargeCodeGroup:TSTGRP|ALL|ALL||ALL|ALL||| > BIL", config.ToString());
		}

		public void TestIsDuplicate()
		{
			var bizo = CreateBusinessObjectForTest();
			var duplicate = CreateBusinessObjectForTest();
			AssertNotEquals("Precondition: two different bizos", bizo.PK, duplicate.PK);

			Assert("Bizo should not be a duplicate of itself", !duplicate.IsDuplicateOf(duplicate));

			duplicate.PSC_GC = ZGuid.NewZGuid();
			Assert("Duplicate should consider GC", !duplicate.IsDuplicateOf(bizo));
			duplicate.PSC_GC = bizo.PSC_GC;
			Assert(duplicate.IsDuplicateOf(bizo));

			duplicate.PSC_ParentTableCode = "AH";
			Assert("Duplicate should consider ParentTableCode", !duplicate.IsDuplicateOf(bizo));
			duplicate.PSC_ParentTableCode = bizo.PSC_ParentTableCode;
			Assert(duplicate.IsDuplicateOf(bizo));

			duplicate.PSC_ParentId = ZGuid.NewZGuid();
			Assert("Duplicate should consider ParentId", !duplicate.IsDuplicateOf(bizo));
			duplicate.PSC_ParentId = bizo.PSC_ParentId;
			Assert(duplicate.IsDuplicateOf(bizo));

			duplicate.PSC_ChargeType = AccPOSChargeTypeList.Codes.Cost;
			Assert("Duplicate should consider ChargeType", !duplicate.IsDuplicateOf(bizo));
			duplicate.PSC_ChargeType = bizo.PSC_ChargeType;
			Assert(duplicate.IsDuplicateOf(bizo));

			duplicate.PSC_TransportMode = Constants.TransportModes.Courier;
			Assert("Duplicate should consider TransportMode", !duplicate.IsDuplicateOf(bizo));
			duplicate.PSC_TransportMode = bizo.PSC_TransportMode;
			Assert(duplicate.IsDuplicateOf(bizo));

			duplicate.PSC_JobType = JobInvoicingConsumerTypes.ShipmentCode;
			Assert("Duplicate should consider JopbType", !duplicate.IsDuplicateOf(bizo));
			duplicate.PSC_JobType = bizo.PSC_JobType;
			Assert(duplicate.IsDuplicateOf(bizo));

			duplicate.PSC_IncoTerm = Constants.IncoTerms.FreeOnBoard;
			Assert("Duplicate should consider Inco Term", !duplicate.IsDuplicateOf(bizo));
			duplicate.PSC_IncoTerm = bizo.PSC_IncoTerm;
			Assert(duplicate.IsDuplicateOf(bizo));

			duplicate.PSC_ServiceDirection = Constants.FreightShipmentDirection.Code.Domestic;
			Assert("Duplicate should consider JobType", !duplicate.IsDuplicateOf(bizo));
			duplicate.PSC_ServiceDirection = bizo.PSC_ServiceDirection;
			Assert(duplicate.IsDuplicateOf(bizo));

			duplicate.PSC_TaxRegistrationType = AccPOSTaxRegistrationList.Codes.ForeignOrganization;
			Assert("Duplicate should consider TaxRegistrationType", !duplicate.IsDuplicateOf(bizo));
			duplicate.PSC_TaxRegistrationType = Constants.CountryCodes.Afghanistan;
			Assert("Duplicate should consider TaxRegistrationType for country codes", !duplicate.IsDuplicateOf(bizo));
			duplicate.PSC_TaxRegistrationType = bizo.PSC_TaxRegistrationType;
			Assert(duplicate.IsDuplicateOf(bizo));

			duplicate.PSC_NK_Branch = "B01";
			Assert("Duplicate should consider Branch", !duplicate.IsDuplicateOf(bizo));
			duplicate.PSC_NK_Branch = bizo.PSC_NK_Branch;
			Assert(duplicate.IsDuplicateOf(bizo));

			duplicate.PSC_SupplyType = SupplyTypeClassificationCodes.LOC;
			Assert("Duplicate should consider Supply Type", !duplicate.IsDuplicateOf(bizo));
			duplicate.PSC_SupplyType = bizo.PSC_SupplyType;
			Assert(duplicate.IsDuplicateOf(bizo));

			duplicate.PSC_PlaceOfSupplyRule = AccPOSRuleList.Codes.SupplierLocation;
			Assert("Duplicate should not consider PlaceOfSupplyRule", duplicate.IsDuplicateOf(bizo));
			duplicate.PSC_PlaceOfSupplyRule = AccPOSRuleList.Codes.DeliveryCFS;
			Assert("Duplicate should not consider PlaceOfSupplyRule", duplicate.IsDuplicateOf(bizo));

			duplicate.PSC_JobType = JobInvoicingConsumerTypes.QuotedBookingCode;
			Assert("Duplicate should consider JopbType", !duplicate.IsDuplicateOf(bizo));
			duplicate.PSC_JobType = bizo.PSC_JobType;
			Assert(duplicate.IsDuplicateOf(bizo));

			duplicate.PSC_JobType = JobInvoicingConsumerTypes.WorkItemCode;
			Assert("Duplicate should consider JopbType", !duplicate.IsDuplicateOf(bizo));
			duplicate.PSC_JobType = bizo.PSC_JobType;
			Assert(duplicate.IsDuplicateOf(bizo));
		}

		public void TestUniqueIndexHandler_CompanyLevel()
		{
			var bizo = CreateBusinessObjectForTest();
			Factory.Save();

			var duplicate = CreateBusinessObjectForTest();
			duplicate.PSC_PlaceOfSupplyRule = AccPOSRuleList.Codes.SupplierLocation;
			AssertNotEquals("Precondition: two different bizos", bizo.PK, duplicate.PK);

			try
			{
				Factory.Save();
				Fail("Expected unique index violation");
			}
			catch (ZSaveException ex)
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZExceptionReporting.HandleSaveException(ex);
				Assert("Unique index handler should have shown an error", UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals(@"This Place of Supply Configuration has already been entered.

Company: 'EDI', Level: 'Company', Company: 'EDI', Job Type: 'ALL', Inco Term: '', Service Direction: 'ALL', Transport Mode: 'ALL', Tax Registration: '', Branch: '', Supply Type: ''.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestUniqueIndexHandler_GroupLevel()
		{
			var group = Factory.NewWithValidTestData<AccPOSChargeCodeGroup>();
			group.GRO_Code = "UNQTST";
			var bizo = CreateBusinessObjectForTest();
			bizo.PSC_ParentTableCode = AccPOSChargeCodeGroupViewSchema.Constants.Prefix;
			bizo.PSC_ParentId = group.PK;
			Factory.Save();

			var duplicate = CreateBusinessObjectForTest();
			duplicate.PSC_ParentTableCode = AccPOSChargeCodeGroupViewSchema.Constants.Prefix;
			duplicate.PSC_ParentId = group.PK;
			duplicate.PSC_PlaceOfSupplyRule = AccPOSRuleList.Codes.SupplierLocation;
			AssertNotEquals("Precondition: two different bizos", bizo.PK, duplicate.PK);

			try
			{
				Factory.Save();
				Fail("Expected unique index violation");
			}
			catch (ZSaveException ex)
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZExceptionReporting.HandleSaveException(ex);
				Assert("Unique index handler should have shown an error", UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals(@"This Place of Supply Configuration has already been entered.

Company: 'EDI', Level: 'Charge Code Group', Charge Code Group: 'UNQTST', Job Type: 'ALL', Inco Term: '', Service Direction: 'ALL', Transport Mode: 'ALL', Tax Registration: '', Branch: '', Supply Type: ''.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestUniqueIndexHandler_ChargeCodeLevel()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "UNQTST";
			var bizo = CreateBusinessObjectForTest();
			bizo.PSC_ParentTableCode = AccChargeCodeSchema.Constants.Prefix;
			bizo.PSC_ParentId = chargeCode.PK;
			Factory.Save();

			var duplicate = CreateBusinessObjectForTest();
			duplicate.PSC_ParentTableCode = AccChargeCodeSchema.Constants.Prefix;
			duplicate.PSC_ParentId = chargeCode.PK;
			duplicate.PSC_PlaceOfSupplyRule = AccPOSRuleList.Codes.SupplierLocation;
			AssertNotEquals("Precondition: two different bizos", bizo.PK, duplicate.PK);

			try
			{
				Factory.Save();
				Fail("Expected unique index violation");
			}
			catch (ZSaveException ex)
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZExceptionReporting.HandleSaveException(ex);
				Assert("Unique index handler should have shown an error", UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals(@"This Place of Supply Configuration has already been entered.

Company: 'EDI', Level: 'Charge Code', Charge Code: 'UNQTST', Job Type: 'ALL', Inco Term: '', Service Direction: 'ALL', Transport Mode: 'ALL', Tax Registration: '', Branch: '', Supply Type: ''.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestAllJCF_TransportModeValuesAreincludedInDbConstraint()
		{
			var bizo = CreateBusinessObjectForTest();
			CombineAssertions(() =>
				{
					foreach (var jobType in bizo.Lookups.JobTypeList.GetAllCodes())
					{
						bizo.PSC_JobType = jobType;

						foreach (var transportMode in bizo.Lookups.TransportModeList.GetAllCodes())
						{
							bizo.PSC_TransportMode = transportMode;
							AssertNoExceptionThrown($"Job Type: {jobType}, Transport Mode: {transportMode}", () => bizo.Factory.Save());
						}
					}
				}
			);

			// The TransportTypeGenericList contains all codes used by Customs. See it here: https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=%2FEnterprise%2FProduct%2FOperations%2FCustoms%2FShared%2FBusiness%2FBusiness%2FCodeDescriptionPairLists%2FTransportTypeGenericList.cs&_a=contents&version=GBmaster
			// But the ObjectFactory.GetType<IDeclarationTransportModeCodeDescriptionPairProvider>() used by Lookups gives a country specific subset.
			// That is why we have to explicitly list all the codes, except the empty one named as Unknown.
			CombineAssertions(() =>
			{
				bizo.PSC_JobType = JobInvoicingConsumerTypes.BrokerageCode;
				var transportTypeGenericList = new[] { "AIR", "FIX", "IWT", "OTH", "OWN", "MAI", "RAI", "ROA", "SEA" };

				foreach (var transportType in transportTypeGenericList)
				{
					bizo.PSC_TransportMode = transportType;
					AssertNoExceptionThrown($"Customs Transport Type: {transportType}", () => bizo.Factory.Save());
				}
			}
			);
		}

		public void TestPSC_DirectionReadOnly()
		{
			var overrideConfiguration = CreateBusinessObjectForTest();
			AssertNotNull("Precondition", overrideConfiguration);

			foreach (JobInvoicingConsumerType jobType in overrideConfiguration.Lookups.JobTypeList)
			{
				overrideConfiguration.PSC_JobType = jobType.Code;
				AssertEquals("ReadOnly [" + jobType + "]", overrideConfiguration.JobType == null || !overrideConfiguration.JobType.IsDirectionSupported, overrideConfiguration.PSC_ServiceDirectionInfo.ReadOnly);
			}
		}

		public void TestPSC_TransportModeReadOnly()
		{
			var overrideConfiguration = CreateBusinessObjectForTest();
			AssertNotNull("Precondition", overrideConfiguration);

			foreach (JobInvoicingConsumerType jobType in overrideConfiguration.Lookups.JobTypeList)
			{
				overrideConfiguration.PSC_JobType = jobType.Code;
				AssertEquals("ReadOnly [" + jobType + "]", overrideConfiguration.JobType == null || !overrideConfiguration.JobType.IsTransportModeSupported, overrideConfiguration.PSC_TransportModeInfo.ReadOnly);
			}
		}

		public void TestPSC_IncoTermReadOnly()
		{
			var overrideConfiguration = CreateBusinessObjectForTest();
			AssertNotNull("Precondition", overrideConfiguration);

			foreach (JobInvoicingConsumerType jobType in overrideConfiguration.Lookups.JobTypeList)
			{
				overrideConfiguration.PSC_JobType = jobType.Code;
				AssertEquals("ReadOnly [" + jobType + "]", overrideConfiguration.Lookups.IncoTermList.Count == 0, overrideConfiguration.PSC_IncoTermInfo.ReadOnly);
			}
		}

		#region IJobConfiguration test

		public void TestIJobConfigurationProperties()
		{
			var bizo = CreateBusinessObjectForTest();
			var config = (IJobConfiguration)bizo;
			AssertEquals(false, config.IncludeOptionsForAllJobTypes);
			AssertEquals(JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All, config.JobType);
			AssertEquals(Constants.FreightShipmentDirection.Code.All, config.ServiceDirection);
			AssertEquals(JobConfigurationSelectorLookups.ModeAdditionalCodes.All, config.TransportMode);

			bizo.PSC_JobType = JobInvoicingConsumerTypes.ShipmentCode;
			AssertEquals(JobInvoicingConsumerTypes.ShipmentCode, config.JobType);

			bizo.PSC_ServiceDirection = Constants.FreightShipmentDirection.Code.Import;
			AssertEquals(Constants.FreightShipmentDirection.Code.Import, config.ServiceDirection);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => CreateBusinessObjectForTest();

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => CreateBusinessObjectForTest();

		protected override BusinessObject GetNewBusinessObject() => CreateBusinessObjectForTest();

		protected AccPOSConfiguration CreateBusinessObjectForTest() => Factory.NewWithValidTestData<AccPOSConfiguration>();

		#endregion

		public void TestGetLogByStatus_WhenBranchNotShow()
		{
			Assert("EnableBranchLevelTaxOverrideRuleConfigurations is False", !AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.Value);

			var placeOfSupplyConfiguration = GetNewPOSConfigurationWithAllPropertiesPreset();
			AssertEquals($"Place of Supply Configuration Added: Source:Charge Code, Job Type:SHP, Charge Type:ALL, Inco Term:FOB, Direction:EXP, Transport Mode:SEA, Tax Reg:NON, Supply Type:LOC, Rule:SUP", placeOfSupplyConfiguration.GetLogByStatus(BusinessObjectLoggingExtension.AddedLogStatus));
			Factory.Save();

			ChangeAllPropertiesForPOSConfiguration(placeOfSupplyConfiguration);
			AssertEquals($"Place of Supply Configuration Deleted: Source:Charge Code, Job Type:SHP, Charge Type:ALL, Inco Term:FOB, Direction:EXP, Transport Mode:SEA, Tax Reg:NON, Supply Type:LOC, Rule:SUP", placeOfSupplyConfiguration.GetLogByStatus(BusinessObjectLoggingExtension.DeleteLogStatus, true));
			AssertEquals($"Place of Supply Configuration Added: Source:Charge Code Group, Job Type:FCN, Charge Type:COS, Inco Term:, Direction:IMP, Transport Mode:AIR, Tax Reg:FRO, Supply Type:LOA, Rule:CPL", placeOfSupplyConfiguration.GetLogByStatus(BusinessObjectLoggingExtension.AddedLogStatus));
		}

		public void TestGetLogByStatus_WhenBranchShown()
		{
			AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Assert("EnableBranchLevelTaxOverrideRuleConfigurations is True", AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.Value);

			var placeOfSupplyConfiguration = GetNewPOSConfigurationWithAllPropertiesPreset();
			AssertEquals($"Place of Supply Configuration Added: Source:Charge Code, Job Type:SHP, Charge Type:ALL, Inco Term:FOB, Direction:EXP, Transport Mode:SEA, Tax Reg:NON, Branch:SYD, Supply Type:LOC, Rule:SUP", placeOfSupplyConfiguration.GetLogByStatus(BusinessObjectLoggingExtension.AddedLogStatus));
			Factory.Save();

			ChangeAllPropertiesForPOSConfiguration(placeOfSupplyConfiguration);
			AssertEquals($"Place of Supply Configuration Deleted: Source:Charge Code, Job Type:SHP, Charge Type:ALL, Inco Term:FOB, Direction:EXP, Transport Mode:SEA, Tax Reg:NON, Branch:SYD, Supply Type:LOC, Rule:SUP", placeOfSupplyConfiguration.GetLogByStatus(BusinessObjectLoggingExtension.DeleteLogStatus, true));
			AssertEquals($"Place of Supply Configuration Added: Source:Charge Code Group, Job Type:FCN, Charge Type:COS, Inco Term:, Direction:IMP, Transport Mode:AIR, Tax Reg:FRO, Branch:SIN, Supply Type:LOA, Rule:CPL", placeOfSupplyConfiguration.GetLogByStatus(BusinessObjectLoggingExtension.AddedLogStatus));
		}

		public void TestGetLogReference()
		{
			var placeOfSupplyConfiguration = GetNewPOSConfigurationWithAllPropertiesPreset();

			var logReference = placeOfSupplyConfiguration.GetLogReference();
			AssertEquals(placeOfSupplyConfiguration.GetLogByStatus(BusinessObjectLoggingExtension.AddedLogStatus), logReference);
			Factory.Save();

			ChangeAllPropertiesForPOSConfiguration(placeOfSupplyConfiguration);

			logReference = placeOfSupplyConfiguration.GetLogReference();
			AssertEquals(placeOfSupplyConfiguration.GetLogByStatus(BusinessObjectLoggingExtension.DeleteLogStatus, true) + "\r\n" + placeOfSupplyConfiguration.GetLogByStatus(BusinessObjectLoggingExtension.AddedLogStatus), logReference);
		}

		AccPOSConfiguration GetNewPOSConfigurationWithAllPropertiesPreset()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();

			var placeOfSupplyConfiguration = chargeCode.PlaceOfSupplyConfigurations.AddNew();
			placeOfSupplyConfiguration.PSC_ParentTableCode = AccChargeCodeSchema.Constants.Prefix;
			placeOfSupplyConfiguration.PSC_JobType = "SHP";
			placeOfSupplyConfiguration.PSC_ChargeType = "ALL";
			placeOfSupplyConfiguration.PSC_IncoTerm = "FOB";
			placeOfSupplyConfiguration.PSC_ServiceDirection = "EXP";
			placeOfSupplyConfiguration.PSC_TransportMode = "SEA";
			placeOfSupplyConfiguration.PSC_TaxRegistrationType = "NON";
			placeOfSupplyConfiguration.PSC_NK_Branch = "SYD";
			placeOfSupplyConfiguration.PSC_SupplyType = "LOC";
			placeOfSupplyConfiguration.PSC_PlaceOfSupplyRule = "SUP";
			return placeOfSupplyConfiguration;
		}

		void ChangeAllPropertiesForPOSConfiguration(AccPOSConfiguration placeOfSupplyConfiguration)
		{
			placeOfSupplyConfiguration.PSC_ParentTableCode = AccPOSChargeCodeGroupViewSchema.Constants.Prefix;
			placeOfSupplyConfiguration.PSC_JobType = "FCN";
			placeOfSupplyConfiguration.PSC_ChargeType = "COS";
			placeOfSupplyConfiguration.PSC_IncoTerm = "";
			placeOfSupplyConfiguration.PSC_ServiceDirection = "IMP";
			placeOfSupplyConfiguration.PSC_TransportMode = "AIR";
			placeOfSupplyConfiguration.PSC_TaxRegistrationType = "FRO";
			placeOfSupplyConfiguration.PSC_NK_Branch = "SIN";
			placeOfSupplyConfiguration.PSC_SupplyType = "LOA";
			placeOfSupplyConfiguration.PSC_PlaceOfSupplyRule = "CPL";
		}
	}
}
