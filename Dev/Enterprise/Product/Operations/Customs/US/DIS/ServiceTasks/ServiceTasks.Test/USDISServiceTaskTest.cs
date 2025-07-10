using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.DIS.Business;
using Enterprise.Customs.US.DIS.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.US.DIS.ServiceTasks.Testing
{
	[TestedType(typeof(USDISServiceTask))]
	sealed class USDISServiceTaskTest : ServiceTaskTestCase<USDISServiceTask>
	{
		public void TestCanRunInAnyBranch()
		{
			var hostedServiceAttribute = GetHostedServiceAttributes().Single();
			Assert("CanRunInAnyBranch", hostedServiceAttribute.CanRunInAnyBranch);
		}

		public void TestHostedServiceMinimumPeriod()
		{
			AssertEquals("30Seconds", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		public void TestRunTasksOfOutgoingMessageProcessor()
		{
			var declaration = new TestHelper(Factory).GetJobDeclaration();
			var docManagerSupport = (IDocManagerSupport)declaration;
			var storageMain = docManagerSupport.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(declaration, "TST");
			var eDocs = storageMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, "ABC.pdf", "ABC", false);
			var requiredDoc = ((IDocsAndCartageParent)declaration).RequiredDocumentsProvider.RequiredDocuments.AddNew();
			var message = Factory.New<EDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageText = TestHelper.BondSubmissionXml;
			message.EM_MessageType = MessageTypeList.Codes.Submission;
			message.EM_LinkedObject = requiredDoc;
			var attachment = message.MessageAttachments.AddNew();
			attachment.EG_StorageDocsGuid = eDocs.UniqueKey;
			Factory.Save();
			docManagerSupport.DocManagerInfo.MasterFactory.Save();
			var serviceTask = new USDISServiceTask();
			InitialiseTaskSchedule(serviceTask);
			RunTaskSchedule(serviceTask);
			message.Reload();
			AssertNotNull("interchange is created now", message.Interchange);
		}

		public void TestRunTasksForIncomingInterchanges()
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_BodyText = TestHelper.DocumentValidationResponseFailedXml;
			interchange.EI_From = "USC";
			interchange.EI_To = "ABC";
			Factory.Save();
			var serviceTask = new USDISServiceTask();
			InitialiseTaskSchedule(serviceTask);
			RunTaskSchedule(serviceTask);
			var factory2 = new BusinessObjectFactory();
			var interchangeLoaded = factory2.Load<EDIInterchange>(interchange.PK);
			AssertEquals("Messages are spawn", 1, interchangeLoaded.ContainedMessages.Count);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"US Customs DIS messages outbound",
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.USCustomsDIS,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),

					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"US Customs DIS messages inbound",
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.USCustomsDIS,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),

					new TaskNudgeInformationForTest(
						EDIInterchangeSchema.Constants.TableName,
						"US Customs DIS interchanges inbound",
						EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.USCustomsDIS,
						EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
						EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
						EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued),
				};
			}
		}
	}
}
