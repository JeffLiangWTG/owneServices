using System.IO;
using System.Xml;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.UY.Manifest.Business.Testing
{
	sealed class UYCInboundInterchangeProcessorTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateMesssage()
		{
			var header = Factory.New<Integration.Customs.ASYCUDA.UYManifest.IAsycudaManifestHeader>();
			header.AMA_JobReference = "MAN0000071";
			Factory.Save();

			var bodyText = Path.Combine(BaseSourcePath, DAETestingConstants.SampleWithEnvelope);
			var interchange = CreateInterchange(DAETestingHelper.GetExpectedMessageXML(headerText), DAETestingHelper.GetExpectedMessageXML(bodyText));
			var processor = new UYCInboundInterchangeProcessor(new string[] { ApplicationCodeList.Codes.UYCustoms });
			processor.ExecuteBatch();
			var processedInterchange = interchange;
			processedInterchange.Reload();
			var message = processedInterchange.ContainedMessages[0];
			var headerXmlDoc = new XmlDocument();
			headerXmlDoc.LoadXml(DAETestingHelper.GetExpectedMessageXML(headerText));
			var bodyTextWithOutSign = Path.Combine(BaseSourcePath, DAETestingConstants.SampleWithOutEnvelope);

			CombineAssertions(() =>
			{
				AssertEquals(message.EM_ApplicationCode, ApplicationCodeList.Codes.UYCustoms);
				AssertEquals(message.EM_MessageType, processedInterchange.EI_InterchangeType);
				AssertEquals(message.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Receive);
				AssertEquals(message.EM_Status, EDIMessageStatusList.Codes.Queued);
				AssertEquals(message.EM_MessageText, UYMessageFormatting.FormatWithXMLRepresentation(DAETestingHelper.GetExpectedMessageXML(bodyTextWithOutSign)));
				AssertEquals(message.EM_GB, processedInterchange.EI_GB);
				AssertEquals(message.EM_GE, GlbDepartment.CurrentDepartment.PK);
				AssertEquals(message.EM_MessageNum, processedInterchange.EI_InterchangeNum.Right(EDIMessage.Schema.EM_MessageNumMaxLength));
				AssertEquals(message.EM_EI, processedInterchange.PK);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateMesssageWithEmptyBody()
		{
			var interchange = CreateInterchange(DAETestingHelper.GetExpectedMessageXML(headerText), "");
			var processor = new UYCInboundInterchangeProcessor(new string[] { ApplicationCodeList.Codes.UYCustoms });
			processor.ExecuteBatch();
			var processedInterchange = interchange;
			processedInterchange.Reload();
			CombineAssertions(() =>
			{
				AssertEquals(EDIInterchange.Status.Error, processedInterchange.EI_Status);
				AssertEquals("NO UY CUSTOMS DATA", processedInterchange.Logs.MostRecentLogByEventTime(Events.ErrorReport).SL_Reference);
				AssertEquals(0, processedInterchange.ContainedMessages.Count);
			});
		}

		EDIInterchange CreateInterchange(string headerText, string bodyText)
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_IsActive = true;
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.UYCustoms;
			interchange.EI_InterchangeType = MessageTypes.Codes.UYC;
			interchange.EI_From = UYMessageConstants.InterchangeToTest;
			interchange.EI_To = "eHub";
			interchange.EI_HeaderText = headerText;
			interchange.EI_BodyText = bodyText;
			interchange.EI_GB = GlbBranch.CurrentBranch.PK;
			Factory.Save();
			return interchange;
		}

		readonly ZString headerText = Path.Combine(BaseSourcePath, DAETestingConstants.HeaderText);
	}
}
