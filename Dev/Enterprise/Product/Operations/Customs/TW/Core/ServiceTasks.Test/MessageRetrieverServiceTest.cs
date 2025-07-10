using System.Collections.Generic;
using System.Linq;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Business.BatchProcessor;
using Enterprise.Customs.TW.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.TW.ServiceTasks.Testing
{
	[TestedType(typeof(MessageRetrieverService))]
	sealed class MessageRetrieverServiceTest : ServiceTaskTestCase<MessageRetrieverService>
	{
		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttributes = GetHostedServiceAttributes();
			AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
			var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Code", "TWC", hostedServiceAttribute.Code);
				AssertEquals("Description", "TW Customs Message Retriever", hostedServiceAttribute.Description);
				AssertEquals("Category", "TWC", hostedServiceAttribute.Category);
				AssertEquals("MinimumPeriod", "30Seconds", hostedServiceAttribute.MinimumPeriod);
				AssertEquals("RequiresCompanyInCountry", Core.Constants.CountryCodes.Taiwan, hostedServiceAttribute.RequiresCompanyInCountry);
				AssertEquals("CanRunInAnyBranch", true, hostedServiceAttribute.CanRunInAnyBranch);
			});
		}

		public void TestOneRealExamples()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.UCMPServiceTask, Core.Constants.CountryCodes.Taiwan, ZDateTime.Today, value: false))
			{
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				var org2Code = orgHeader.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.CBF, "00612348", Core.Constants.CountryCodes.Taiwan);
				var warehouseAddress = orgHeader.Addresses.AddNew();
				warehouseAddress.Address1 = "Address1";
				warehouseAddress.Address2 = "Address2";
				org2Code.OK_OA_PremisesAddress = orgHeader.MainAddress.PK;
				Factory.Save();

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = "IMP";
				declaration.JE_CustomsOffice = "AA";
				declaration.ImporterDocumentaryAddress.E2_OA_Address = orgHeader.MainAddress.PK;
				var entryInstruction1 = declaration.CusEntryInstruction;
				entryInstruction1.CEI_Style = "B1";
				entryInstruction1.CEI_CustomsOffice = "AA";
				entryInstruction1.CEI_DateForDuty = new ZDateTime(2019, 01, 01);
				entryInstruction1.CEI_BoxNumber = "123";

				var cusHead1 = Factory.NewWithValidTestData<CusEntryHeader>();
				cusHead1.CH_CEI_Instruction = entryInstruction1.PK;
				cusHead1.CH_JE = declaration.PK;
				var cusNum1 = Factory.NewWithValidTestData<CusEntryNumber>();
				cusNum1.CE_ParentID = cusHead1.PK;
				cusNum1.CE_Category = "CUS";
				cusNum1.CE_EntryType = "IMP";
				cusNum1.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
				cusNum1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;

				var declaration2 = Factory.New<JobDeclaration>();
				declaration2.JE_MessageType = "EXP";
				declaration2.JE_CustomsOffice = "AA";
				declaration2.ImporterDocumentaryAddress.E2_OA_Address = orgHeader.MainAddress.PK;
				var entryInstruction2 = declaration2.CusEntryInstruction;
				entryInstruction2.CEI_Style = "B1";
				entryInstruction2.CEI_CustomsOffice = "AA";
				entryInstruction2.CEI_DateForDuty = new ZDateTime(2020, 01, 01);
				entryInstruction2.CEI_BoxNumber = "123";

				var cusHead2 = Factory.NewWithValidTestData<CusEntryHeader>();
				cusHead2.CH_CEI_Instruction = entryInstruction2.PK;
				cusHead2.CH_JE = declaration2.PK;
				var cusNum2 = Factory.NewWithValidTestData<CusEntryNumber>();
				cusNum2.CE_ParentID = cusHead2.PK;
				cusNum2.CE_Category = "CUS";
				cusNum2.CE_EntryType = "EXP";
				cusNum2.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
				cusNum2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;

				cusHead1.EntryNumber = "AAB10923480001";
				cusHead2.EntryNumber = "AAB10923480001";
				Factory.Save();

				var expectedMessageTypes = new Dictionary<string, string>();
				expectedMessageTypes["NX5106"] = "ARM";
				expectedMessageTypes["N5204"] = "ERM";
				expectedMessageTypes["N5109"] = "IEM";
				expectedMessageTypes["N5116"] = "IRM";
				expectedMessageTypes["N5107"] = "RFM";
				expectedMessageTypes["N5168"] = "UHC";
				expectedMessageTypes["N5110"] = "TPC";
				expectedMessageTypes["N5111"] = "TAD";

				var xmlDocument = new XmlDocument();
				foreach (var expectedMessageType in expectedMessageTypes)
				{
					var messageType = expectedMessageType.Value;
					var number = messageType == "ERM" ? cusHead2.EntryNumber : cusHead1.EntryNumber;
					var procedure = messageType == "ERM" ? "2" : "1";
					var messageText = TWXmlTestCaseWithFactory.GetExpectedMessageXML(@"Enterprise.Customs.TW.Business.Testing.BatchProcessor.TestFile." + expectedMessageType.Key + ".xml");
					xmlDocument.LoadXml(messageText);
					var nameSpace = new XmlNamespaceManager(xmlDocument.NameTable);
					nameSpace.AddNamespace("a", xmlDocument.DocumentElement.Attributes["xmlns"]?.Value);
					var idNode = xmlDocument.SelectSingleNode("a:Response/a:Declaration/a:ID", nameSpace)
						?? xmlDocument.SelectSingleNode("a:Declaration/a:ID", nameSpace);
					if (idNode != null)
					{
						idNode.InnerText = number;
					}

					var procedureNode = xmlDocument.SelectSingleNode("a:Response/a:Declaration/a:GovernmentProcedure/a:tw_TransportTypeCode", nameSpace)
						?? xmlDocument.SelectSingleNode("a:Declaration/a:GovernmentProcedure/a:tw_TransportTypeCode", nameSpace);
					if (procedureNode != null)
					{
						procedureNode.InnerText = procedure;
					}

					var interchange = Factory.NewWithValidTestData<TWCInterchange>();
					interchange.EI_From = "TWCustoms";
					interchange.EI_To = "TEST";
					interchange.EI_ApplicationCode = "TWC";
					interchange.EI_InterchangeType = "TWC";
					interchange.EI_InterchangeNum = expectedMessageType.Key + ".ABC123456";

					interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
					interchange.EI_Status = EDIInterchange.Status.Queued;
					interchange.EI_IsActive = true;
					interchange.ForceDeprecatedNTextUsageForTesting = true;
					interchange.EI_BodyNText = xmlDocument.OuterXml;
					AssertXMLContains("<ID>" + number + "</ID>", interchange.EI_BodyText);
					Factory.Save();

					InitialiseAndRunTaskSchedule(new MessageRetrieverService());

					var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIInterchange.ApplicationCodes.TaiwanCustoms);
					query.AddToFilter(EDIMessageSchema.EM_EI, new[] { interchange.PK });
					query.AddToFilter(EDIMessageSchema.EM_MessageType, messageType);
					var messages = Factory.Load<EDIMessage>(query);
					AssertEquals("Running the TWC task should have created one '" + messageType + "' messages", 1, messages.Length);
					AssertEquals(messageType + "... And the messages should be queued and successful", TWMessage.Status.ProcessedOK, messages[0].EM_Status);
				}
			}
		}

		public void TestProcessMessages_WithUCMPEnabled()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.UCMPServiceTask, Core.Constants.CountryCodes.Taiwan, ZDateTime.Today, value: true))
			{
				var twMessage = Factory.New<TWMessage>();
				twMessage.EM_MessageType = MessageTypeList.Codes.ICD;
				twMessage.EM_MessageNum = "1";
				twMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				twMessage.EM_Status = EDIMessage.Status.Queued;
				twMessage.EM_GB = GlbBranch.CurrentBranch.PK;
				Factory.Save();

				var logger = new TestServiceLogger();
				var serviceTask = new MessageRetrieverService();
				serviceTask.ServiceLogger = logger;

				serviceTask.RunTask();

				twMessage.Reload();
				AssertEquals("twMessage.EM_Status", EDIMessage.Status.Queued, twMessage.EM_Status);
			}
		}

		public void TestProcessMessages_WithUCMPDisabled()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.UCMPServiceTask, Core.Constants.CountryCodes.Taiwan, ZDateTime.Today, value: false))
			{
				var twMessage = Factory.New<TWMessage>();
				twMessage.EM_MessageType = MessageTypeList.Codes.ICD;
				twMessage.EM_MessageNum = "1";
				twMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				twMessage.EM_Status = EDIMessage.Status.Queued;
				twMessage.EM_GB = GlbBranch.CurrentBranch.PK;
				Factory.Save();

				var logger = new TestServiceLogger();
				var serviceTask = new MessageRetrieverService();
				serviceTask.ServiceLogger = logger;

				serviceTask.RunTask();

				twMessage.Reload();
				AssertNotEquals("twMessage.EM_Status", EDIMessage.Status.Queued, twMessage.EM_Status);
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"TW Customs messages inbound",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.TaiwanCustoms),

					new TaskNudgeInformationForTest(
						EDIInterchangeSchema.Constants.TableName,
						"TW Customs interchanges inbound",
						EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
						EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
						EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
						EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.TaiwanCustoms),
				};
			}
		}
	}
}
