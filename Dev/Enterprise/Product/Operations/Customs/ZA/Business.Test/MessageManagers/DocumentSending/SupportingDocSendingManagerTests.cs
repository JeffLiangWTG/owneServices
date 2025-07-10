using System.Data;
using System.IO;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ZA.Business.MessageManagers.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.MessageManagers.DocumentSending.Testing
{
	sealed class SupportingDocSendingManagerTests : TestCaseWithFactory
	{
		[CargoWise.Data.Testing.UseSnapshotProtection]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSupportingDocSendingManagerConcurrencyException_CloseStream()
		{
			using (Factory.AddDisposableService())
			using (RunNonTransactioned())
			{
				var declaration = Factory.New<JobDeclaration>();
				var imageBytes = File.ReadAllBytes(BaseSourcePath + @"Enterprise\Product\Operations\Customs\ZA\Business\MessageManagers\DocumentSending\TestDocs\Blank.pdf");
				var eDoc = declaration.DocManagerInfo.AddFileOrDocument(imageBytes, "Invoice.pdf", "CIV");
				var eDoc2 = declaration.DocManagerInfo.AddFileOrDocument(imageBytes, "Worksheet.pdf", "WSH");
				var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				var caseNumber1 = entryInstruction.CaseNumbers.AddNew();
				caseNumber1.CY_Data = "1234567";
				var entryHeader = declaration.ActiveEntryHeaders.AddNew();
				entryHeader.CH_BGMReference = "12341234";
				entryHeader.CH_CEI_Instruction = entryInstruction.PK;
				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine.JI_CEI = entryInstruction.PK;
				var entryLine = entryHeader.MergedLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
				CombineAssertions("Successful Send", () =>
				{
					var agentOrg = Factory.New<OrgHeader>();
					agentOrg.OH_Code = "AGENTCODE";
					agentOrg.OH_FullName = "AgentName";
					declaration.JE_OH_AgentOverride = agentOrg.PK;
					agentOrg.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "AGT7634", Core.Constants.CountryCodes.SouthAfrica);
					agentOrg.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SouthAfricaCodeTypes.CustomsDualProfileCode, "CDPC", Core.Constants.CountryCodes.SouthAfrica);
					declaration.AgentCode = "AGT7634";
					Factory.Save();
					var sql = @"
UPDATE dbo.CusCodeData
SET
	CY_Code = 'ABC',
	CY_SystemLastEditTimeUtc = GETUTCDATE(),
	CY_SystemLastEditUser = '~BP' WHERE CY_PK = @PK";
					using (var command = Db.Connection.Command(sql))
					{
						command.AddParameter("@PK", SqlDbType.UniqueIdentifier, caseNumber1.PK.ToGuid());
						command.ExecuteNonQuery();
					}

					entryHeader.CH_BGMReference = "1234";
					var objectParent = new JobDeclarationSupportingDocSendingObjectParent(declaration);
					var sendingObject1 = objectParent.SendingObjectsCollection.AddNew();
					sendingObject1.EDoc = eDoc.UniqueKey;
					sendingObject1.LocalReferenceNumber = entryHeader.CH_BGMReference;
					sendingObject1.CaseNumber = "1234567";
					var sendingObject2 = objectParent.SendingObjectsCollection.AddNew();
					sendingObject2.EDoc = eDoc2.UniqueKey;
					sendingObject2.LocalReferenceNumber = entryHeader.CH_BGMReference;
					sendingObject2.CaseNumber = "1234567";
					var notification = new MessageNotificationCollector_ForTest();
					var sendingManager = new SupportingDocSendingManager(objectParent, notification);
					notification.NextAnswer = true;
					notification.NextAnswer = true;
					// This will also resolve the concurrency issue.
					sendingManager.SendMessages();
					AssertNoExceptionThrown("Should not get a 'Stream Closed' inner exception.", () =>
					{
						Factory.Save();
					});
				});
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSendMessages()
		{
			using (Factory.AddDisposableService())
			{
				var declaration = Factory.New<JobDeclaration>();
				var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				var caseNumber1 = instruction.CaseNumbers.AddNew();
				caseNumber1.CY_Data = "1234567";
				var entryHeader = declaration.ActiveEntryHeaders.AddNew();
				entryHeader.CH_BGMReference = "12341234";
				entryHeader.CH_CEI_Instruction = instruction.PK;
				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine.JI_CEI = instruction.PK;
				var entryLine = entryHeader.MergedLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
				var imageBytes = File.ReadAllBytes(BaseSourcePath + @"Enterprise\Product\Operations\Customs\ZA\Business\MessageManagers\DocumentSending\TestDocs\Blank.pdf");
				var eDoc = declaration.DocManagerInfo.AddFileOrDocument(imageBytes, "Invoice.pdf", "CIV");
				var eDoc2 = declaration.DocManagerInfo.AddFileOrDocument(imageBytes, "Worksheet.pdf", "WSH");
				CombineAssertions("Catch Error in case pre-send check were missed", () =>
				{
					declaration.Branch.Company.OrgProxy.SetAgentCode(declaration.Branch.Company.Country, ZString.Empty);
					declaration.Branch.Company.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SouthAfricaCodeTypes.CustomsDualProfileCode, ZString.Empty, Core.Constants.CountryCodes.SouthAfrica);
					entryHeader.CH_BGMReference = "1234";
					var objectParent = new JobDeclarationSupportingDocSendingObjectParent(declaration);
					objectParent.SendingObjectsCollection.AddNew();
					var sendingObject = objectParent.SendingObjectsCollection[0];
					sendingObject.EDoc = eDoc.UniqueKey;
					sendingObject.LocalReferenceNumber = entryHeader.CH_BGMReference;
					sendingObject.CaseNumber = "1234567";
					var notification = new MessageNotificationCollector_ForTest();
					var sendingManager = new SupportingDocSendingManager(objectParent, notification);
					sendingManager.SendMessages();
					AssertMultilineASCIIEquals("Error of Sending", @"Agent Code cannot be blank. Please enter a valid Agent Code against the Agent organization selected on the Declaration.
", notification.LastMessage);
					AssertEquals(@"Cannot send Supporting Documents", notification.LastCaption);
					var caseNumber = instruction.CaseNumbers.OfType<CaseNumber>().FirstOrDefault(x => x.CY_Data == sendingObject.CaseNumber);
					AssertEquals("Case Found", "1234567", caseNumber.CY_Data);
					AssertEquals("Case Status Not Updated if Error Saving", ZString.Empty, caseNumber.Document_Status);
				});
				CombineAssertions("Successful Send", () =>
				{
					var agentOrg = Factory.New<OrgHeader>();
					agentOrg.OH_Code = "AGENTCODE";
					agentOrg.OH_FullName = "AgentName";
					declaration.JE_OH_AgentOverride = agentOrg.PK;
					agentOrg.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "AGT7634", Core.Constants.CountryCodes.SouthAfrica);
					agentOrg.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SouthAfricaCodeTypes.CustomsDualProfileCode, "CDPC", Core.Constants.CountryCodes.SouthAfrica);
					declaration.AgentCode = "AGT7634";
					Factory.Save();
					entryHeader.CH_BGMReference = "1234";
					var objectParent = new JobDeclarationSupportingDocSendingObjectParent(declaration);
					var sendingObject1 = objectParent.SendingObjectsCollection.AddNew();
					sendingObject1.EDoc = eDoc.UniqueKey;
					sendingObject1.LocalReferenceNumber = entryHeader.CH_BGMReference;
					sendingObject1.CaseNumber = "1234567";
					var sendingObject2 = objectParent.SendingObjectsCollection.AddNew();
					sendingObject2.EDoc = eDoc2.UniqueKey;
					sendingObject2.LocalReferenceNumber = entryHeader.CH_BGMReference;
					sendingObject2.CaseNumber = "1234567";
					var notification = new MessageNotificationCollector_ForTest();
					var sendingManager = new SupportingDocSendingManager(objectParent, notification);
					notification.NextAnswer = true;
					sendingManager.SendMessages();
					AssertEquals(@"Document queued for sending: Invoice.pdf
Document queued for sending: Worksheet.pdf", notification.LastMessage);
					AssertEquals("Supporting Document Sending Result", notification.LastCaption);
					var caseNumber = instruction.CaseNumbers.OfType<CaseNumber>().FirstOrDefault(x => x.CY_Data == sendingObject1.CaseNumber);
					AssertEquals("Case Found", "1234567", caseNumber.CY_Data);
					AssertEquals("Case Status Updated", "PND", caseNumber.Document_Status);
				});
			}
		}
	}
}
