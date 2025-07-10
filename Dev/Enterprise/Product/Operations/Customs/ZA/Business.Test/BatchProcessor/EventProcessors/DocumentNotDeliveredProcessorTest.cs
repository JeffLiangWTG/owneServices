using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ZA.Business.EventProcessors;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class DocumentNotDeliveredProcessorTest : TestCaseWithFactory
	{
		public void TestProcess()
		{
			var newStaff = Factory.New<IGlbStaff>();
			newStaff.GS_Code = "TUS";
			newStaff.GS_LoginName = "Test.User";
			newStaff.GS_FullName = "Test User";
			newStaff.GS_EmailAddress = "email2@wisetechgloabal.com";
			Factory.Save();
			var lrn = "123456";
			var caseNo = "654321";
			var processor = new DocumentNotDeliveredProcessor(new DummyLogger());
			var xml = new XmlEventValueObject_ForTest(lrn, caseNo);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B0000100X";
			var broker = Factory.New<GlbStaff>();
			broker.GS_Code = "~~";
			broker.GS_FullName = "BROKER NAME";
			broker.GS_EmailAddress = "email1@wisetechglobal.com";
			declaration.JE_GS_NKCusAgent = broker.GS_Code;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = lrn;
			var instr = declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = instr.PK;
			var caseNumber = instr.CaseNumbers.AddNew();
			caseNumber.CY_Data = caseNo;
			processor.Process(declaration, null);
			AssertEquals(CaseNumberTypeList.Codes.DocumentInspectionCases, caseNumber.CY_Code);
			var helper = new SupportingDocumentStatusHelperTest.Helper(Factory, declaration, entryHeader, caseNo);
			var fileName = "test.pdf";
			helper.CreateNewEvent(Events.DocumentSent, helper.CreateNewDSNEDIMessage(fileName));
			SupportingDocumentStatusHelper.UpdateStatus(entryHeader, caseNumber);
			AssertEquals(DocumentStatusCodes.Codes.PND, caseNumber.Document_Status);
			var ediMessage = helper.CreateNewDNDEDIMessage(fileName);
			var universalEvent = ediMessage?.GetEM_MessageTextReader()?.Parse<UniversalEvent>();
			processor.Process(declaration, universalEvent);
			AssertEquals(DocumentStatusCodes.Codes.FAL, caseNumber.Document_Status);
			helper.CreateNewEvent(Events.DocumentDelivered, ediMessage);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(emailToMatched => emailToMatched.Subject == "Supporting Document Submission Failed: Job B0000100X - 'test.pdf'");
			CombineAssertions("Email Address from Agent", () =>
			{
				AssertNotNull(email);
				AssertEquals("One recipient", 1, email.Recipients.Count);
				AssertEquals("Email Address", "email1@wisetechglobal.com", email.Recipients[0].Email);
				var bodyText = email.Body;
				Assert("Contains Job Number", bodyText.Contains("Job Number: "));
				Assert("Contains Document", bodyText.Contains("Document: "));
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			using (EnvProxy.Instance.SetTemporaryUserContext(newStaff.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				helper.CreateNewEvent(Events.DocumentSent, helper.CreateNewDSNEDIMessage(fileName));
				SupportingDocumentStatusHelper.UpdateStatus(entryHeader, caseNumber);
				AssertEquals(DocumentStatusCodes.Codes.PND, caseNumber.Document_Status);
				ediMessage = helper.CreateNewDNDEDIMessage(fileName);
				using (universalEvent = ediMessage?.GetEM_MessageTextReader()?.Parse<UniversalEvent>())
				{
					processor.Process(declaration, universalEvent);
					AssertEquals(DocumentStatusCodes.Codes.FAL, caseNumber.Document_Status);
					helper.CreateNewEvent(Events.DocumentDelivered, ediMessage);
				}
			}

			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(emailToMatched => emailToMatched.Subject == "Supporting Document Submission Failed: Job B0000100X - 'test.pdf'");
			CombineAssertions("Email Address from User", () =>
			{
				AssertNotNull(email);
				AssertEquals("One recipient", 1, email.Recipients.Count);
				AssertEquals("Email Address", "email2@wisetechgloabal.com", email.Recipients[0].Email);
				var bodyText = email.Body;
				Assert("Contains Job Number", bodyText.Contains("Job Number: "));
				Assert("Contains Document", bodyText.Contains("Document: "));
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		}
	}
}
