using System.Threading;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AMS.Common;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Messaging.Business.Testing
{
	sealed class AMSOutgoingMessageProcessorTest : TestCaseWithFactory
	{
		[TestDate(2009, 12, 1)]
		public void TestOutgoingMessagesPackageIntoInterchanges()
		{
			var orgProxy = Factory.New<OrgHeader>();
			orgProxy.OH_Code = "1";
			orgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "ZZSD", Core.Constants.CountryCodes.UnitedStates);
			orgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "S323", Core.Constants.CountryCodes.UnitedStates);
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.GC_OH_OrgProxy = orgProxy.PK;
			Factory.Save();
			var inpm02 = new INPM02();
			inpm02.CarrierAssignedBatchNumber = AMSEDIMessage.AMSMessageNumberPlaceHolder;
			ZString messageText = inpm02.Serialise() + " B";
			var message1 = Factory.New<AMSEDIMessage>();
			message1.EM_ReceiveTransmit = AMSEDIMessage.Direction.Transmit;
			message1.EM_Status = AMSEDIMessage.Status.Queued;
			message1.EM_MessageText = messageText;
			message1.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestCreate;
			AssertNull(message1.Interchange);
			var message2 = Factory.New<AMSEDIMessage>();
			message2.EM_ReceiveTransmit = AMSEDIMessage.Direction.Transmit;
			message2.EM_Status = AMSEDIMessage.Status.Queued;
			message2.EM_MessageText = messageText;
			message2.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestCreate;
			AssertNull(message2.Interchange);
			Factory.Save();
			var processor = new AMSOutgoingMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(CancellationToken.None);
			AssertMessage(message1, "1");
			AssertMessage(message2, "2");
		}

		void AssertMessage(AMSEDIMessage message, ZString interchangeNum)
		{
			message.Reload();
			var interchange = message.Interchange;
			AssertNotNull(interchange);
			AssertEquals(message, interchange.ContainedMessages[0]);
			AssertEquals(1, interchange.ContainedMessages.Count);
			AssertEquals(message.EM_MessageType, interchange.EI_InterchangeType);
			AssertEquals(interchangeNum, interchange.EI_InterchangeNum);
		}
	}
}
