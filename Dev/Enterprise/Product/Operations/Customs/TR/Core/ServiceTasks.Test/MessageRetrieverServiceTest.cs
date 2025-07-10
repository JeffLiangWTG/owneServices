using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ServiceTasks;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.TR.Messaging.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.TR.ServiceTasks.Testing
{
	[TestedType(typeof(MessageRetrieverService))]
	public class MessageRetrieverServiceTest : ServiceTaskTestCase<MessageRetrieverService>
	{
		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttributes = GetHostedServiceAttributes();
			AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
			var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Code", "TRR", hostedServiceAttribute.Code);
				AssertEquals("Description", "TR Customs Interchange Processor", hostedServiceAttribute.Description);
				AssertEquals("Category", "TRC", hostedServiceAttribute.Category);
				AssertEquals("MinimumPeriod", "60Seconds", hostedServiceAttribute.MinimumPeriod);
				AssertEquals("RequiresCompanyInCountry", Core.Constants.CountryCodes.Turkey, hostedServiceAttribute.RequiresCompanyInCountry);
				AssertEquals("CanRunInAnyBranch", true, hostedServiceAttribute.CanRunInAnyBranch);
			});
		}

		public void TestHostedServiceRequirement()
		{
			var methodInfo = typeof(MessageRetrieverService).GetMethod(nameof(MessageRetrieverService.IsRequired));
			Assert("HostedServiceRequirement is applied", Attribute.IsDefined(methodInfo, typeof(HostedServiceRequirementAttribute)));

			CertificateRequirementChecker.ResetForTesting();
			AssertEquals("There is no Certificate configured in Turkey.", MessageRetrieverService.IsRequired());
			GlbExternalPasswordHelperTest.SetupGlbExternalPassword_TRK(Factory);
			CertificateRequirementChecker.ResetForTesting();
			AssertEquals(string.Empty, MessageRetrieverService.IsRequired());
		}

		public void TestRunTask()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey))
			{
				SetupETradeData();
				SetupManifestData();
				SetupSPTSData();
				var task = new MessageRetrieverService();
				InitialiseTaskSchedule(task);
				RunTaskSchedule(task);
				AssertManifestResults();
				AssertETradeData();
				AssertSPTSResults();
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIInterchangeSchema.Constants.TableName,
						"TR Customs Message Retriever Service",
						EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
						EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
						EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
						EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + ApplicationCodeList.Codes.TRCustoms),
				};
			}
		}

		void SetupETradeData()
		{
			eTradeHeader = Factory.New<Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader>();
			eTradeHeader.AMA_JobReference = "2020/00084/1";
			eTradeHeader.AMA_RN_NKCountry = Core.Constants.CountryCodes.Turkey;

			eTradeInterchange = CreateInterchange(TRMessageTypes.Codes.TRE);
			eTradeQueryRegistrationInterchange = CreateInterchange(TRMessageTypes.Codes.TRQ);
			eTradeQueryForInspectionLineInterchange = CreateInterchange(TRMessageTypes.Codes.TRL);
			eTradeQueryForInspectionClerkInterchange = CreateInterchange(TRMessageTypes.Codes.TRI);
			eTradeExportRegistrationNoInterchange = CreateInterchange(TRMessageTypes.Codes.TRS);
			eTradeImportDischargeListInterchange = CreateInterchange(TRMessageTypes.Codes.TRD);
		}

		void SetupManifestData()
		{
			manifestHeader = Factory.New<Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader>();
			manifestHeader.AMA_JobReference = "ULU-2019/00002379";
			manifestHeader.AMA_RN_NKCountry = Core.Constants.CountryCodes.Turkey;

			manifestInterchange = CreateInterchange(TRMessageTypes.Codes.TRO);
			Factory.Save();
		}

		void SetupSPTSData()
		{
			sPTSHeader = Factory.New<Integration.Customs.TR.ICusInBondSPTSHeader>();
			sPTSHeader.BH_JobReference = "SPTS0000001";

			sPTSInterchange = CreateInterchange(TRMessageTypes.Codes.TSP);
			Factory.Save();
		}

		void AssertManifestResults()
		{
			var query = new ZQuery();
			query.AddToFilter(EDIMessageSchema.EM_MessageType, TRMessageTypes.Codes.TRO);

			var messagesCreated = Factory.Load<EDIMessage>(query);
			AssertEquals("NumberOfInterchanges", 1, messagesCreated.Length);

			var message = messagesCreated[0];
			AssertMessage("Manifest", message, manifestInterchange, manifestHeader.PK);

			manifestInterchange.Reload();

			CombineAssertions(() =>
			{
				AssertEquals(EDIMessageSchema.Constants.EM_EI, manifestInterchange.PK, message.EM_EI);
				AssertEquals(EDIInterchangeSchema.Constants.EI_Status, EDIInterchange.Status.Received, manifestInterchange.EI_Status);
			});
		}

		void AssertSPTSResults()
		{
			var query = new ZQuery();
			query.AddToFilter(EDIMessageSchema.EM_MessageType, TRMessageTypes.Codes.TSP);

			var messagesCreated = Factory.Load<EDIMessage>(query);
			AssertEquals("NumberOfInterchanges", 1, messagesCreated.Length);

			var message = messagesCreated[0];
			AssertMessage("SPTS", message, sPTSInterchange, sPTSHeader.PK);

			sPTSInterchange.Reload();

			CombineAssertions(() =>
			{
				AssertEquals(EDIMessageSchema.Constants.EM_EI, sPTSInterchange.PK, message.EM_EI);
				AssertEquals(EDIInterchangeSchema.Constants.EI_Status, EDIInterchange.Status.Received, sPTSInterchange.EI_Status);
			});
		}

		void AssertETradeData()
		{
			var query = new ZQuery();
			query.AddToFilter(EDIMessageSchema.EM_MessageType, TRMessageTypes.Codes.TRE);
			var messagesCreated = Factory.Load<EDIMessage>(query);

			query = new ZQuery();
			query.AddToFilter(EDIMessageSchema.EM_MessageType, TRMessageTypes.Codes.TRQ);
			var messagesCreated2 = Factory.Load<EDIMessage>(query);

			query = new ZQuery();
			query.AddToFilter(EDIMessageSchema.EM_MessageType, TRMessageTypes.Codes.TRL);
			var messagesCreated3 = Factory.Load<EDIMessage>(query);

			query = new ZQuery();
			query.AddToFilter(EDIMessageSchema.EM_MessageType, TRMessageTypes.Codes.TRI);
			var messagesCreated4 = Factory.Load<EDIMessage>(query);

			query = new ZQuery();
			query.AddToFilter(EDIMessageSchema.EM_MessageType, TRMessageTypes.Codes.TRS);
			var messagesCreated5 = Factory.Load<EDIMessage>(query);

			query = new ZQuery();
			query.AddToFilter(EDIMessageSchema.EM_MessageType, TRMessageTypes.Codes.TRD);
			var messagesCreated6 = Factory.Load<EDIMessage>(query);

			CombineAssertions(() =>
			{
				AssertEquals("NumberOfInterchanges for type TRE", 1, messagesCreated.Length);
				AssertEquals("NumberOfInterchanges for type TRQ", 1, messagesCreated2.Length);
				AssertEquals("NumberOfInterchanges for type TRL", 1, messagesCreated3.Length);
				AssertEquals("NumberOfInterchanges for type TRI", 1, messagesCreated4.Length);
				AssertEquals("NumberOfInterchanges for type TRS", 1, messagesCreated5.Length);
				AssertEquals("NumberOfInterchanges for type TRD", 1, messagesCreated6.Length);
			});

			var message = messagesCreated[0];
			var message2 = messagesCreated2[0];
			var message3 = messagesCreated3[0];
			var message4 = messagesCreated4[0];
			var message5 = messagesCreated5[0];
			var message6 = messagesCreated6[0];

			CombineAssertions(() =>
			{
				AssertMessage("TRE", message, eTradeInterchange, eTradeHeader.PK);
				AssertMessage("TRQ", message2, eTradeQueryRegistrationInterchange, eTradeHeader.PK);
				AssertMessage("TRL", message3, eTradeQueryForInspectionLineInterchange, eTradeHeader.PK);
				AssertMessage("TRI", message4, eTradeQueryForInspectionClerkInterchange, eTradeHeader.PK);
				AssertMessage("TRS", message5, eTradeExportRegistrationNoInterchange, eTradeHeader.PK);
				AssertMessage("TRD", message6, eTradeImportDischargeListInterchange, eTradeHeader.PK);
			});

			eTradeInterchange.Reload();
			eTradeQueryRegistrationInterchange.Reload();
			eTradeQueryForInspectionLineInterchange.Reload();
			eTradeQueryForInspectionClerkInterchange.Reload();
			eTradeExportRegistrationNoInterchange.Reload();
			eTradeImportDischargeListInterchange.Reload();

			CombineAssertions(() =>
			{
				AssertEquals(EDIMessageSchema.Constants.EM_EI, eTradeInterchange.PK, message.EM_EI);
				AssertEquals(EDIInterchangeSchema.Constants.EI_Status, EDIInterchange.Status.Received, eTradeInterchange.EI_Status);
				AssertEquals(EDIMessageSchema.Constants.EM_EI, eTradeQueryRegistrationInterchange.PK, message2.EM_EI);
				AssertEquals(EDIInterchangeSchema.Constants.EI_Status, EDIInterchange.Status.Received, eTradeQueryRegistrationInterchange.EI_Status);
				AssertEquals(EDIMessageSchema.Constants.EM_EI, eTradeQueryForInspectionLineInterchange.PK, message3.EM_EI);
				AssertEquals(EDIInterchangeSchema.Constants.EI_Status, EDIInterchange.Status.Received, eTradeQueryForInspectionLineInterchange.EI_Status);
				AssertEquals(EDIMessageSchema.Constants.EM_EI, eTradeQueryForInspectionClerkInterchange.PK, message4.EM_EI);
				AssertEquals(EDIInterchangeSchema.Constants.EI_Status, EDIInterchange.Status.Received, eTradeQueryForInspectionClerkInterchange.EI_Status);
				AssertEquals(EDIMessageSchema.Constants.EM_EI, eTradeExportRegistrationNoInterchange.PK, message5.EM_EI);
				AssertEquals(EDIInterchangeSchema.Constants.EI_Status, EDIInterchange.Status.Received, eTradeExportRegistrationNoInterchange.EI_Status);
				AssertEquals(EDIMessageSchema.Constants.EM_EI, eTradeImportDischargeListInterchange.PK, message6.EM_EI);
				AssertEquals(EDIInterchangeSchema.Constants.EI_Status, EDIInterchange.Status.Received, eTradeImportDischargeListInterchange.EI_Status);
			});
		}

		void AssertMessage(string title, EDIMessage message, EDIInterchange interchange, ZGuid headerPK)
		{
			AssertEquals(title, message.EM_ApplicationCode, ApplicationCodeList.Codes.TRCustoms);
			AssertEquals(title, message.EM_MessageType, interchange.EI_InterchangeType);
			AssertEquals(title, message.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Receive);
			AssertEquals(title, message.EM_Status, EDIMessageStatusList.Codes.Queued);
			AssertEquals(title, message.EM_MessageText, interchange.EI_BodyText);
			AssertEquals(title, message.EM_GB, interchange.EI_GB);
			AssertEquals(title, message.EM_GE, GlbDepartment.CurrentDepartment.PK);
			AssertEquals(title, message.EM_LinkTable, "");
			AssertEquals(title, message.EM_LinkUniqueID, ZGuid.Empty);
			AssertEquals(title, message.EM_MessageNum, interchange.EI_InterchangeNum.Right(EDIMessage.Schema.EM_MessageNumMaxLength));
			AssertEquals(title, message.EM_EI, interchange.PK);
		}

		EDIInterchange CreateInterchange(ZString interchangeType)
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_IsActive = true;
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.TRCustoms;
			interchange.EI_InterchangeType = interchangeType;
			interchange.EI_From = "TROCustoms";
			interchange.EI_To = "eHub";
			interchange.EI_BodyText = TRMessageTestHelper.GetBodyText(interchangeType);
			interchange.EI_HeaderText = "EIheader text";
			interchange.EI_GB = GlbBranch.CurrentBranch.PK;
			return interchange;
		}

		Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader manifestHeader;
		Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader eTradeHeader;
		Integration.Customs.TR.ICusInBondSPTSHeader sPTSHeader;
		EDIInterchange eTradeInterchange;
		EDIInterchange eTradeQueryRegistrationInterchange;
		EDIInterchange eTradeQueryForInspectionLineInterchange;
		EDIInterchange eTradeQueryForInspectionClerkInterchange;
		EDIInterchange eTradeExportRegistrationNoInterchange;
		EDIInterchange manifestInterchange;
		EDIInterchange eTradeImportDischargeListInterchange;
		EDIInterchange sPTSInterchange;
	}
}
