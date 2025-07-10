using System;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.US.Business.BIRD.Common;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input.Testing
{
	sealed class ENS10Test : BIRDHeaderUpdateTest
	{
		public void TestENS10LongValues()
		{
			var ens10 = new ENS10();
			ens10.DistrictPortOfEntry = "12345";
			ens10.ImporterOfRecordNumber = "1234567890123";
			ens10.UltimateConsigneeNumber = "1234567890123";
			ens10.CBPF4811ReferenceNumber = "1234567890123";
			ens10.EntryFilerCode = "1234";
			ens10.EntryType = "123";
			ens10.SuretyCode = "1234";
			ens10.StateOfDestination = "123";

			var serialised = string.Empty;

			AssertNoExceptionThrown(() => _ = ens10.Serialise());
		}

		public void TestIBIRDHeaderIDRecord()
		{
			var ens10 = new ENS10();
			ens10.EntryFilerCode = "ABC";
			ens10.EntryNumber = "12345678";

			AssertEquals("ABC", ((IBIRDHeaderIDRecord)ens10).EntryFilerCode);
			AssertEquals("12345678", ((IBIRDHeaderIDRecord)ens10).EntryNumber);
		}

		public void TestSetDefaultBIRDBranchOnImporter()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			var branch = company.Branches.AddNew();

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeaderWrapper.New(importer).ZO_GB = branch.PK;

			importer.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12-1234567AB");
			var cusAgentStaffMember = Factory.NewWithValidTestData<GlbStaff>();
			cusAgentStaffMember.GS_Code = "JLB";
			cusAgentStaffMember.GS_FullName = "JIMMY BEAN";
			cusAgentStaffMember.GS_WorkPhone = "+1 738 2956000";
			cusAgentStaffMember.GS_EmailAddress = "jimmy.beam@somecompany.somewhere";

			var testBranch2 = Factory.NewWithValidTestData<GlbBranch>();
			testBranch2.GB_Phone = "+1 738 294 5000";
			var fdaStaffMember = Factory.NewWithValidTestData<GlbStaff>();
			fdaStaffMember.GS_FullName = "JEREMY JONES";
			fdaStaffMember.GS_WorkPhone = "+1 738 2956005";
			fdaStaffMember.GS_GB_HomeBranch = testBranch2.PK;
			fdaStaffMember.GS_EmailAddress = "jeremy.jones@company.xx";
			Factory.Save();

			var ens10 = new ENS10();
			ens10.ImporterOfRecordNumber = "12-1234567AB";
			ens10.UltimateConsigneeNumber = "12-1234567CC";

			var declaration = Factory.New<JobDeclaration>();
			((IBIRDHeaderRecord)ens10).Update(declaration, new NotificationCollection());

			AssertEquals("PreCondition", importer.PK, declaration.JE_OH_Importer);
			AssertEquals(branch.PK, declaration.JE_GB);

			declaration.JE_DateOfArrival = ZDateTime.Today;
			DataRegistry.Business.USCustomsDataRegistry.Instance.BranchFDAContact.SetValue(Guid.Empty, testBranch2.PK.ToGuid(), Guid.Empty, fdaStaffMember.PK.ToGuid());
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.JE_GS_NKCusAgent = cusAgentStaffMember.GS_Code;

			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2008.11.0200";

			AssertEquals("JIMMY BEAN", declaration.US_FDAContactName);
			OrgHeaderWrapper.New(importer).ZO_GB = testBranch2.PK;
			((IBIRDHeaderRecord)ens10).Update(declaration, new NotificationCollection());
			AssertEquals("JEREMY JONES", declaration.US_FDAContactName);
		}

		public void TestNotifyPartyNumber()
		{
			var notifyParty = Factory.NewWithValidTestData<OrgHeader>();
			notifyParty.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12-1234567CB");

			var notifyParty1 = Factory.NewWithValidTestData<OrgHeader>();
			notifyParty1.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12-1234567BN");

			var notifyParty2 = Factory.NewWithValidTestData<OrgHeader>();
			notifyParty2.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12-1234567SS");

			var ior = Factory.NewWithValidTestData<OrgHeader>();
			var iorWrapper = OrgHeaderWrapper.New(ior);
			iorWrapper.ZO_NPID = "12-1234567SS";
			ior.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12-1234567II");

			Factory.Save();

			var ens10 = new ENS10();
			var declaration = Factory.New<JobDeclaration>();

			ens10.CBPF4811ReferenceNumber = "12-1234567CB";
			((IBIRDHeaderRecord)ens10).Update(declaration, new NotificationCollection());
			AssertEquals(notifyParty.PK, declaration.JE_OH_NotifyParty);

			ens10.CBPF4811ReferenceNumber = "12-1234567BN";
			((IBIRDHeaderRecord)ens10).Update(declaration, new NotificationCollection());
			AssertEquals(notifyParty1.PK, declaration.JE_OH_NotifyParty);

			ens10.CBPF4811ReferenceNumber = "12-1234567SS";
			((IBIRDHeaderRecord)ens10).Update(declaration, new NotificationCollection());
			AssertEquals(notifyParty2.PK, declaration.JE_OH_NotifyParty);

			ens10.CBPF4811ReferenceNumber = "12-1234567XX";
			var notifications = new NotificationCollection();
			((IBIRDHeaderRecord)ens10).Update(declaration, notifications);
			Assert(notifications.ContainsNotificationContaining("CBPF 4811 Notify Party can either be set up on Declaration or on Importer Of Record (Organization -> Config -> US Defaults)"));

			ens10.ImporterOfRecordNumber = "12-1234567II";
			ens10.CBPF4811ReferenceNumber = "12-1234567SS";

			declaration.JE_OH_NotifyParty = ZGuid.Empty;
			((IBIRDHeaderRecord)ens10).Update(declaration, new NotificationCollection());
			AssertEquals(ZGuid.Empty, declaration.JE_OH_NotifyParty);
		}

		public void TestSetImporterOrConsigneeWithIncompleteNumbers()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();

			importer.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12-123456700");
			Factory.Save();

			var ens10 = new ENS10();
			ens10.ImporterOfRecordNumber = "12-1234567";

			var declaration = Factory.New<JobDeclaration>();
			((IBIRDHeaderRecord)ens10).Update(declaration, new NotificationCollection());

			AssertEquals("Should have padded with two zeros before searching", importer.PK, declaration.JE_OH_Importer);
		}

		public void TestDoNotSetEntryFilerCodeIfEmpty()
		{
			Business.Testing.DeclarationTestHelper.SetEntryFilerCode("XJ5");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			AssertEquals("PreCondition", "XJ5", declaration.US_EntryFilerCode);

			var ens10 = new ENS10();
			((IBIRDHeaderRecord)ens10).Update(declaration, new NotificationCollection());

			AssertEquals("Entry filer code should stay as it is readonly for import", "XJ5", declaration.US_EntryFilerCode);
		}

		protected override IBIRDHeaderRecord[] GetPopulatedHeaderRecords()
		{
			ENS10 ens10 = new ENS10();

			ens10.UpdateActionCode = "A";
			ens10.DistrictPortOfEntry = "8888";
			ens10.ImporterOfRecordNumber = "12-1234567AB";
			ens10.UltimateConsigneeNumber = "12-1234567CC";
			ens10.LiveEntryIndicator = 1;
			ens10.MissingDocumentCodes = "0199";
			ens10.BondType = BondTypeList.Codes.SingleTransactionBond;
			ens10.EstimatedEntryDate = new ZDate(2009, 6, 1);
			ens10.EntryFilerCode = "ABC";
			ens10.EntryNumber = "12345678";
			ens10.EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			ens10.SuretyCode = "089";
			ens10.StateOfDestination = "IL";
			ens10.OGALineReleaseIndicator = 1;

			ENS10 ens10WithTempImpNumber = new ENS10();
			ens10WithTempImpNumber.UpdateActionCode = "A";
			ens10WithTempImpNumber.DistrictPortOfEntry = "8888";
			ens10WithTempImpNumber.CBPF4811ReferenceNumber = "121234567CB";
			ens10WithTempImpNumber.LiveEntryIndicator = 0;
			ens10WithTempImpNumber.MissingDocumentCodes = "01";
			ens10WithTempImpNumber.BondType = "0";
			ens10WithTempImpNumber.EntryFilerCode = "ABC";
			ens10WithTempImpNumber.EntryNumber = "12345678";
			ens10WithTempImpNumber.EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			ens10WithTempImpNumber.SuretyCode = "089";
			ens10WithTempImpNumber.StateOfDestination = "IL";

			return new IBIRDHeaderRecord[] { ens10, ens10WithTempImpNumber };
		}

		protected override void PrepareDeclaration(JobDeclaration declaration, IBIRDHeaderRecord headerRecord)
		{
			base.PrepareDeclaration(declaration, headerRecord);

			ENS10 ens10 = (ENS10)headerRecord;

			if (ens10.ImporterOfRecordNumber == "12-1234567AB")
			{
				OrgHeader importerOfRecord = Factory.NewWithValidTestData<OrgHeader>();
				importerOfRecord.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12-1234567AB", GlbCompany.CurrentCompany.Country);
			}

			if (ens10.UltimateConsigneeNumber == "12-1234567CC")
			{
				OrgHeader ultimateConsignee = Factory.NewWithValidTestData<OrgHeader>();
				ultimateConsignee.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12-1234567CC", GlbCompany.CurrentCompany.Country);
			}

			if (ens10.CBPF4811ReferenceNumber == "121234567CB")
			{
				OrgHeader ultimateConsignee2 = Factory.NewWithValidTestData<OrgHeader>();
				ultimateConsignee2.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "121234567CB", GlbCompany.CurrentCompany.Country);
			}

			if (ens10.CBPF4811ReferenceNumber == "121234567BN")
			{
				OrgHeader ultimateConsignee2 = Factory.NewWithValidTestData<OrgHeader>();
				ultimateConsignee2.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.CBPAssignedNumber, "121234567BN", GlbCompany.CurrentCompany.Country);
			}

			if (ens10.CBPF4811ReferenceNumber == "121234567SS")
			{
				OrgHeader ultimateConsignee2 = Factory.NewWithValidTestData<OrgHeader>();
				ultimateConsignee2.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.SocialSecurityNumber, "121234567SS", GlbCompany.CurrentCompany.Country);
			}

			Factory.Save();
		}

		protected override Type GetTypeOfMessageBlock() => typeof(ENS10);

		protected override string[] GetFieldNameToExcludeForTesting()
		{
			return new string[]
			{
				"OtherGovernmentAgencyOGACode", //This field is not used currently and for future use
				"ElectronicInvoiceIndicator",// Not relevant for BIRD
			};
		}

		protected override MessageBuilders.EntryHeaderMessageBuilder<ABIInputBlockControlGenerator> GetMessageBuilder(JobDeclaration declaration, IBIRDHeaderRecord headerRecord)
		{
			return new MessageBuilders.EntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
		}
	}
}
