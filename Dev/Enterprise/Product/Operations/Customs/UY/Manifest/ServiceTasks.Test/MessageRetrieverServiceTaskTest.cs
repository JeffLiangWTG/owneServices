using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.UY.Manifest.Business;
using Enterprise.Customs.UY.Manifest.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.UY.Manifest.ServiceTasks.Testing
{
	[TestedType(typeof(MessageRetrieverService))]
	sealed class MessageRetrieverServiceTaskTest : ServiceTaskTestCase<MessageRetrieverService>
	{
		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttributes = GetHostedServiceAttributes();
			AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
			var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Code", "UYR", hostedServiceAttribute.Code);
				AssertEquals("Description", "UY Customs Message Retriever", hostedServiceAttribute.Description);
				AssertEquals("Category", "UYC", hostedServiceAttribute.Category);
				AssertEquals("MinimumPeriod", "60Seconds", hostedServiceAttribute.MinimumPeriod);
				AssertEquals("RequiresCompanyInCountry", Core.Constants.CountryCodes.Uruguay, hostedServiceAttribute.RequiresCompanyInCountry);
				AssertEquals("CanRunInAnyBranch", true, hostedServiceAttribute.CanRunInAnyBranch);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRunTask()
		{
			SetupManifestData();

			var task = new MessageRetrieverService();
			InitialiseTaskSchedule(task);
			RunTaskSchedule(task);

			AssertManifestResults();
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIInterchangeSchema.Constants.TableName,
						ServiceTaskApplicationCodeList.Descriptions.UYR,
						EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchangeStatusList.Codes.Queued,
						EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
						EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
						EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.UYCustoms),
				};
			}
		}

		void AssertManifestResults()
		{
			var messagesCreated = Factory.Load<EDIMessage>(new ZQuery()).OrderBy(n => n.EM_MessageNum);
			AssertEquals("NumberOfInterchanges", 2, messagesCreated.Count());

			var bodyTextWithOutSign = DAETestingHelper.GetExpectedMessageXML(Path.Combine(BaseSourcePath, DAETestingConstants.SampleWithOutEnvelope));
			var message = messagesCreated.First();
			var message2 = messagesCreated.Last();
			CombineAssertions(() =>
			{
				AssertMessage(message, manifestInterchange, UYMessageFormatting.FormatWithXMLRepresentation(bodyTextWithOutSign));
				AssertMessage(message2, manifestInterchangeErrorNotification, manifestInterchangeErrorNotification.EI_HeaderText);
			});

			manifestInterchange.Reload();
			CombineAssertions(() =>
			{
				AssertEquals(EDIMessageSchema.Constants.EM_EI, manifestInterchange.PK, message.EM_EI);
				AssertEquals(EDIInterchangeSchema.Constants.EI_Status, EDIInterchange.Status.Received, manifestInterchange.EI_Status);
			});
		}

		void AssertMessage(EDIMessage message, EDIInterchange interchange, ZString resultMessageText)
		{
			AssertEquals(message.EM_ApplicationCode, ApplicationCodeList.Codes.UYCustoms);
			AssertEquals(message.EM_MessageType, interchange.EI_InterchangeType);
			AssertEquals(message.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Receive);
			AssertEquals(message.EM_Status, EDIMessageStatusList.Codes.Queued);
			AssertEquals(message.EM_MessageText, resultMessageText);
			AssertEquals(message.EM_GB, interchange.EI_GB);
			AssertEquals(message.EM_GE, GlbDepartment.CurrentDepartment.PK);
			AssertEquals(message.EM_MessageNum, interchange.EI_InterchangeNum.Right(EDIMessage.Schema.EM_MessageNumMaxLength));
			AssertEquals(message.EM_EI, interchange.PK);
		}

		EDIInterchange CreateInterchange(ZString interchangeType, ZString header, ZString body)
		{
			var interchange = Factory.New<EDIInterchange>();

			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_IsActive = true;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.UYCustoms;
			interchange.EI_InterchangeType = interchangeType;
			interchange.EI_From = UYMessageConstants.InterchangeToTest;
			interchange.EI_To = "eHub";
			interchange.EI_HeaderText = header;
			interchange.EI_BodyText = body;
			interchange.EI_GB = GlbBranch.CurrentBranch.PK;

			return interchange;
		}

		void SetupManifestData()
		{
			manifestHeader = Factory.New<Integration.Customs.ASYCUDA.UYManifest.IAsycudaManifestHeader>();
			manifestHeader.AMA_JobReference = "MAN0000071";
			manifestHeader.AMA_RN_NKCountry = Core.Constants.CountryCodes.Uruguay;

			var bodyText = DAETestingHelper.GetExpectedMessageXML(Path.Combine(BaseSourcePath, DAETestingConstants.SampleWithEnvelope));
			var headerText = DAETestingHelper.GetExpectedMessageXML(Path.Combine(BaseSourcePath, DAETestingConstants.HeaderText));
			var headerTextError = DAETestingHelper.GetExpectedMessageTXT(Path.Combine(BaseSourcePath, DAETestingConstants.HeaderTextErrorNotification));
			var bodyTextError = DAETestingHelper.GetExpectedMessageTXT(Path.Combine(BaseSourcePath, DAETestingConstants.BodyTextErrorNotification));
			manifestInterchange = CreateInterchange(MessageTypes.Codes.UYC, headerText, bodyText);
			manifestInterchangeErrorNotification = CreateInterchange("XER", headerTextError, bodyTextError);
			Factory.Save();
		}

		Integration.Customs.ASYCUDA.UYManifest.IAsycudaManifestHeader manifestHeader;
		EDIInterchange manifestInterchange;
		EDIInterchange manifestInterchangeErrorNotification;
	}
}
