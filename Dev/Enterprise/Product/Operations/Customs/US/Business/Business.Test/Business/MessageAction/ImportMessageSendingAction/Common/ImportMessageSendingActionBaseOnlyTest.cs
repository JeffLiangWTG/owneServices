using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ImportMessageSendingActionBaseOnlyTest : TestCaseWithFactory
	{
		public void TestIsElectronicInvoice()
		{
			var mock = new Mock<ImportMessageSendingAction>(new object[] { Declaration, ImportMessageStatusList.MessageType.EntrySummary, Actions });
			AssertEquals("IsElectronicInvoice", false, mock.Object.IsElectronicInvoice);
			mock = new Mock<ImportMessageSendingAction>(new object[] { Declaration, ImportMessageStatusList.MessageType.ElectronicInvoice, Actions });
			AssertEquals("IsElectronicInvoice", true, mock.Object.IsElectronicInvoice);
		}

		public void TestIsEntrySummary()
		{
			var mock = new Mock<ImportMessageSendingAction>(new object[] { Declaration, ImportMessageStatusList.MessageType.EntrySummary, Actions });
			AssertEquals("IsEntrySummary", true, mock.Object.IsEntrySummary);
			mock = new Mock<ImportMessageSendingAction>(new object[] { Declaration, ImportMessageStatusList.MessageType.ElectronicInvoice, Actions });
			AssertEquals("IsEntrySummary", false, mock.Object.IsEntrySummary);
		}

		public void TestIsCargoRelease()
		{
			var mock = new Mock<ImportMessageSendingAction>(new object[] { Declaration, ImportMessageStatusList.MessageType.CargoRelease, Actions });
			AssertEquals("IsCargoRelease", true, mock.Object.IsCargoRelease);
			mock = new Mock<ImportMessageSendingAction>(new object[] { Declaration, ImportMessageStatusList.MessageType.ElectronicInvoice, Actions });
			AssertEquals("IsCargoRelease", false, mock.Object.IsCargoRelease);
		}

		public void TestIsSimplifiedEntry()
		{
			var mock = new Mock<ImportMessageSendingAction>(new object[] { Declaration, ImportMessageStatusList.MessageType.ACECargoRelease, Actions });
			AssertEquals("IsSimplifiedEntry", true, mock.Object.IsACECargoRelease);
			mock = new Mock<ImportMessageSendingAction>(new object[] { Declaration, ImportMessageStatusList.MessageType.ElectronicInvoice, Actions });
			AssertEquals("IsSimplifiedEntry", false, mock.Object.IsACECargoRelease);
		}

		public void TestUS_MessageDescription()
		{
			var mock = new Mock<ImportMessageSendingAction>(new object[] { Declaration, ImportMessageStatusList.MessageType.EntrySummary, Actions });
			AssertEquals("US_MessageDescription", ZString.Empty, mock.Object.US_MessageDescription);
		}

		public void TestUS_MessageContents()
		{
			var mock = new Mock<ImportMessageSendingAction>(new object[] { Declaration, ImportMessageStatusList.MessageType.EntrySummary, Actions });
			mock.CallBase = true;
			AssertEquals("US_MessageContents", ImportMessageSendingAction.MessageContentPreviewNotSupportedYet, mock.Object.US_MessageContents);
		}

		public void TestUS_CertifyCargoReleaseInfoReadOnly()
		{
			var mock = new Mock<ImportMessageSendingAction>(Declaration, ImportMessageStatusList.MessageType.EntrySummary, Actions)
			{
				CallBase = true
			};
			mock.Setup(m => m.US_SendMessage).Returns(true);
			AssertEquals("US_CertifyCargoReleaseInfo.ReadOnly", true, mock.Object.US_CertifyCargoReleaseInfo.ReadOnly);
			mock.Setup(m => m.US_SendMessage).Returns(false);
			AssertEquals("US_CertifyCargoReleaseInfo.ReadOnly", true, mock.Object.US_CertifyCargoReleaseInfo.ReadOnly);
			mock.Setup(m => m.US_SendMessage).Returns(true);
			mock.Protected().Setup<bool>("GetUS_CertifyCargoReleaseInfoReadOnly").Returns(true);
			AssertEquals("US_CertifyCargoReleaseInfo.ReadOnly", true, mock.Object.US_CertifyCargoReleaseInfo.ReadOnly);
			mock.Protected().Setup<bool>("GetUS_CertifyCargoReleaseInfoReadOnly").Returns(false);
			mock.Protected().Setup<bool>("GetUS_CertifyCargoReleaseInfoReadOnly").Returns(false);
			AssertEquals("US_CertifyCargoReleaseInfo.ReadOnly", false, mock.Object.US_CertifyCargoReleaseInfo.ReadOnly);
			mock.Setup(m => m.US_SendMessage).Returns(false);
			mock.Invocations.Clear();
			AssertEquals("US_CertifyCargoReleaseInfo.ReadOnly", true, mock.Object.US_CertifyCargoReleaseInfo.ReadOnly);
			AssertEquals("US_CertifyCargoReleaseInfo.ReadOnly", true, mock.Object.US_CertifyCargoReleaseInfo.ReadOnly);
			mock.Protected().Verify("GetUS_CertifyCargoReleaseInfoReadOnly", Times.Never());
		}

		public void TestUS_AcknowledgeAndSignInfoReadOnly()
		{
			var mock = new Mock<ImportMessageSendingAction>(new object[] { Declaration, ImportMessageStatusList.MessageType.EntrySummary, Actions });
			mock.CallBase = true;
			ImportMessageSendingAction action = mock.Object;
			action.US_SendMessage = true;
			AssertEquals("US_AcknowledgeAndSignInfo.ReadOnly", false, action.US_AcknowledgeAndSignInfo.ReadOnly);
			action.US_SendMessage = false;
			AssertEquals("US_AcknowledgeAndSignInfo.ReadOnly", true, action.US_AcknowledgeAndSignInfo.ReadOnly);
		}

		public void TestUS_Declarant()
		{
			var mock = new Mock<ImportMessageSendingAction>(new object[] { Declaration, ImportMessageStatusList.MessageType.EntrySummary, Actions });
			ImportMessageSendingAction action = mock.Object;
			GlbStaff.CurrentUser.GS_FullName = "MR BOB";
			AssertEquals("US_Declarant", "MR BOB", action.US_Declarant);
			GlbStaff.CurrentUser.GS_FullName = "MR SMITH";
			AssertEquals("US_Declarant", "MR SMITH", action.US_Declarant);
		}

		public void TestUS_TitleOfDeclarantInfoReadOnly()
		{
			var mock = new Mock<ImportMessageSendingAction>(new object[] { Declaration, ImportMessageStatusList.MessageType.EntrySummary, Actions });
			mock.CallBase = true;
			ImportMessageSendingAction action = mock.Object;
			action.US_SendMessage = true;
			AssertEquals("US_TitleOfDeclarantInfo.ReadOnly", true, action.US_TitleOfDeclarantInfo.ReadOnly);
			action.US_SendMessage = false;
			AssertEquals("US_TitleOfDeclarantInfo.ReadOnly", true, action.US_TitleOfDeclarantInfo.ReadOnly);
			action.US_SendMessage = true;
			mock.Protected().Setup<bool>("GetUS_TitleOfDeclarantInfoReadOnly").Returns(true);
			AssertEquals("US_TitleOfDeclarantInfo.ReadOnly", true, action.US_TitleOfDeclarantInfo.ReadOnly);
			mock.Protected().Setup<bool>("GetUS_TitleOfDeclarantInfoReadOnly").Returns(false);
			AssertEquals("US_TitleOfDeclarantInfo.ReadOnly", false, action.US_TitleOfDeclarantInfo.ReadOnly);
			action.US_SendMessage = false;
			mock.Protected().Verify<bool>("GetUS_CertifyCargoReleaseInfoReadOnly", Times.Never());
			AssertEquals("US_TitleOfDeclarantInfo.ReadOnly", true, action.US_TitleOfDeclarantInfo.ReadOnly);
		}

		public void TestUS_DateOfDeclarationInfoReadOnly()
		{
			var mock = new Mock<ImportMessageSendingAction>(new object[] { Declaration, ImportMessageStatusList.MessageType.EntrySummary, Actions });
			mock.CallBase = true;
			ImportMessageSendingAction action = mock.Object;
			action.US_SendMessage = true;
			AssertEquals("US_DateOfDeclarationInfo.ReadOnly", true, action.US_DateOfDeclarationInfo.ReadOnly);
			action.US_SendMessage = false;
			AssertEquals("US_DateOfDeclarationInfo.ReadOnly", true, action.US_DateOfDeclarationInfo.ReadOnly);
			action.US_SendMessage = true;
			mock.Protected().Setup<bool>("GetUS_DateOfDeclarationInfoReadOnly").Returns(true);
			AssertEquals("US_DateOfDeclarationInfo.ReadOnly", true, action.US_DateOfDeclarationInfo.ReadOnly);
			mock.Protected().Setup<bool>("GetUS_DateOfDeclarationInfoReadOnly").Returns(false);
			AssertEquals("US_DateOfDeclarationInfo.ReadOnly", false, action.US_DateOfDeclarationInfo.ReadOnly);
			action.US_SendMessage = false;
			mock.Protected().Verify<bool>("GetUS_CertifyCargoReleaseInfoReadOnly", Times.Never());
			AssertEquals("US_DateOfDeclarationInfo.ReadOnly", true, action.US_DateOfDeclarationInfo.ReadOnly);
		}

		public void TestUS_IsCustomsRequestedInfoReadOnly()
		{
			var mock = new Mock<ImportMessageSendingAction>(new object[] { Declaration, ImportMessageStatusList.MessageType.EntrySummary, Actions });
			mock.CallBase = true;
			ImportMessageSendingAction action = mock.Object;
			action.US_SendMessage = true;
			AssertEquals("US_IsCustomsRequestedInfo.ReadOnly", true, action.US_IsCustomsRequestedInfo.ReadOnly);
			action.US_SendMessage = false;
			AssertEquals("US_IsCustomsRequestedInfo.ReadOnly", true, action.US_IsCustomsRequestedInfo.ReadOnly);
			action.US_SendMessage = true;
			mock.Protected().Setup<bool>("GetUS_IsCustomsRequestedInfoReadOnly").Returns(true);
			AssertEquals("US_IsCustomsRequestedInfo.ReadOnly", true, action.US_IsCustomsRequestedInfo.ReadOnly);
			mock.Protected().Setup<bool>("GetUS_IsCustomsRequestedInfoReadOnly").Returns(false);
			AssertEquals("US_IsCustomsRequestedInfo.ReadOnly", false, action.US_IsCustomsRequestedInfo.ReadOnly);
			action.US_SendMessage = false;
			mock.Protected().Verify<bool>("GetUS_CertifyCargoReleaseInfoReadOnly", Times.Never());
			AssertEquals("US_IsCustomsRequestedInfo.ReadOnly", true, action.US_IsCustomsRequestedInfo.ReadOnly);
		}

		public void TestUS_SE_ContactFields()
		{
			GlbStaff.CurrentUser.GS_FullName = "Timothy Kensington-Double-Barrelled-Shotgun";
			GlbStaff.CurrentUser.GS_WorkPhone = "+18005550100";
			GlbStaff.CurrentUser.GS_IsSystemAccount = false;
			var mock = new Mock<ImportMessageSendingAction>(new object[] { Declaration, ImportMessageStatusList.MessageType.ACECargoRelease, Actions });
			mock.CallBase = true;
			ImportMessageSendingAction action = mock.Object;
			AssertEquals("Contact Name truncated due to max length", "Timothy Kensington-Double-Barrelled-Shot", action.US_SE_ContactName);
			AssertEquals("Contact Phone Number", "8005550100", action.US_SE_ContactPhone);
			GlbBranch branch = Factory.Load<GlbBranch>(Env.CurrentBranchPK);
			branch.GB_Phone = "+1 738 294 5000";
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "amy xiang";
			staff.GS_WorkPhone = "+86 158 5050 3354";
			staff.GS_GB_HomeBranch = branch.PK;
			USCustomsDataRegistry.Instance.CargoReleaseFTZContact.SetValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, staff.PK.ToGuid());
			mock = new Mock<ImportMessageSendingAction>(new object[] { Declaration, ImportMessageStatusList.MessageType.ACECargoRelease, Actions });
			mock.CallBase = true;
			action = mock.Object;
			AssertEquals("amy xiang", action.US_SE_ContactName);
			AssertEquals("15850503354", action.US_SE_ContactPhone);
			staff.GS_WorkPhone = ZString.Empty;
			mock = new Mock<ImportMessageSendingAction>(new object[] { Declaration, ImportMessageStatusList.MessageType.ACECargoRelease, Actions });
			mock.CallBase = true;
			action = mock.Object;
			AssertEquals("amy xiang", action.US_SE_ContactName);
			AssertEquals("7382945000", action.US_SE_ContactPhone);
		}

		public void TestValidateModes()
		{
			var entrySummaryActionMock = new Mock<ImportMessageSendingAction>(Declaration, ImportMessageStatusList.MessageType.EntrySummary, Actions);
			entrySummaryActionMock.CallBase = true;
			ImportMessageSendingAction entrySummaryAction = entrySummaryActionMock.Object;
			AssertNotNull(entrySummaryAction);
			ImportMessageSendingAction inBondAction = new Mock<ImportMessageSendingAction>(Declaration, ImportMessageStatusList.MessageType.InBondDeparture, Actions).Object;
			AssertNotNull(inBondAction);
			AssertEquals("ValidationMode calculated", ValidationModes.EntrySummary, entrySummaryAction.ValidateMode);
			entrySummaryAction.US_CertifyCargoRelease = true;
			AssertEquals("ValidationMode calculated", ValidationModes.EntrySummary | ValidationModes.CargoRelease, entrySummaryAction.ValidateMode);
			entrySummaryActionMock.Protected().Setup<bool>("IsElectronicInvoicing").Returns(true);
			AssertEquals("ValidationMode calculated", ValidationModes.EntrySummary | ValidationModes.CargoRelease, entrySummaryAction.ValidateMode);
			ImportMessageSendingAction crlAction = new Mock<ImportMessageSendingAction>(Declaration, ImportMessageStatusList.MessageType.BorderCargoRelease, Actions).Object;
			AssertEquals("ValidationMode calculated", ValidationModes.EntrySummary | ValidationModes.CargoRelease, entrySummaryAction.ValidateMode);
		}

		public void TestPSCReasonAndExplanationEnabled()
		{
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			Declaration.US_PSC = true;
			var eNSEntry = Declaration.CustomsEntryHeaders.AddNew();
			eNSEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			eNSEntry.EntryNumber = "11111";
			var action = new EntryHeaderMessageSendingAction(eNSEntry, ImportMessageStatusList.MessageType.EntrySummary, Actions);
			AssertEquals("PSCReason and Explanation enabled", true, action.IsPSCReasonAndExplanationEnabled);
			Declaration.US_PSC = false;
			AssertEquals("PSCReason and Explanation disabled", false, action.IsPSCReasonAndExplanationEnabled);
			Declaration.US_PSC = true;
			var cRLEntry = Declaration.CustomsEntryHeaders.AddNew();
			cRLEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.CargoRelease;
			cRLEntry.CH_CH_PrimeEntry = eNSEntry.PK;
			action = new EntryHeaderMessageSendingAction(cRLEntry, ImportMessageStatusList.MessageType.CargoRelease, Actions);
			AssertEquals("PSCReason and Explanation disabled", false, action.IsPSCReasonAndExplanationEnabled);
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			Declaration.US_PSC = true;
			AssertEquals("PSCReason and Explanation disabled", false, action.IsPSCReasonAndExplanationEnabled);
		}

		public void TestIsSE13DataRelevant()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XXX";
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Replacement);
			var action = actions[0];
			action.US_CertifyCargoRelease = false;
			Assert(!action.IsSE13DataRelevant);
			action.US_CertifyCargoRelease = true;
			Assert(action.IsSE13DataRelevant);
			actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			action = actions[0];
			Assert(action.IsSE13DataRelevant);
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration2.US_EntryFilerCode = "XXX";
			declaration2.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration2.Invoices.AddNew();
			declaration2.InvoiceLines.AddNew();
			declaration2.US_EnableENS = false;
			declaration2.US_EnableCRL = true;
			declaration2.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			actions = new ImportMessageSendingActionCollection(declaration2, ImportMessageSendingMessageType.Original);
			action = actions[0];
			Assert(action.IsSE13DataRelevant);
			declaration2.ActiveEntryHeaders.SimplifiedEntry.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
			actions = new ImportMessageSendingActionCollection(declaration2, ImportMessageSendingMessageType.Replacement);
			action = actions[0];
			Assert(action.IsSE13DataRelevant);
			actions = new ImportMessageSendingActionCollection(declaration2, ImportMessageSendingMessageType.Deletion);
			action = actions[0];
			Assert(action.IsSE13DataRelevant);
		}

		public void TestShouldPSCReasonAndExplanationBeSaved()
		{
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			Declaration.US_PSC = true;
			var eNSEntry = Declaration.CustomsEntryHeaders.AddNew();
			eNSEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			eNSEntry.EntryNumber = "11111";
			var action = new EntryHeaderMessageSendingAction(eNSEntry, ImportMessageStatusList.MessageType.EntrySummary, Actions);
			action.US_SendMessage = false;
			AssertEquals("PSC Reason and Explanation should not be saved", false, action.ShouldPSCReasonAndExplanationBeSaved);
		}

		public void TestIsTemporaryImportationBond()
		{
			var mock = new Mock<ImportMessageSendingAction>(new object[] { Declaration, ImportMessageStatusList.MessageType.TemporaryImportationBond, Actions });
			AssertEquals("IsTemporaryImportationBond", true, mock.Object.IsTemporaryImportationBond);
			mock = new Mock<ImportMessageSendingAction>(new object[] { Declaration, ImportMessageStatusList.MessageType.ElectronicInvoice, Actions });
			AssertEquals("IsEntrySummary", false, mock.Object.IsEntrySummary);
		}

		ImportMessageSendingActionCollection actions;
		ImportMessageSendingActionCollection Actions => actions ?? (actions = new ImportMessageSendingActionCollection(Declaration, ImportMessageSendingMessageType.Original));

		JobDeclaration declaration;
		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				}

				return declaration;
			}
		}
	}
}
