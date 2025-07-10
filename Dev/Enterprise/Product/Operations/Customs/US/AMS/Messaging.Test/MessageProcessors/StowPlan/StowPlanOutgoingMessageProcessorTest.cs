using System.Threading;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Edifact;
using Enterprise.Edifact.D95B.Segments;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Messaging.Business.Testing
{
	class StowPlanOutgoingMessageProcessorTest : TestCaseWithFactory
	{
		[TestDate(2012, 10, 25)]
		public void TestOutgoingMessagesPackageIntoInterchanges()
		{
			var orgProxy = Factory.New<OrgHeader>();
			orgProxy.OH_Code = "1";
			orgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "ZZSD", Core.Constants.CountryCodes.UnitedStates);
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.GC_OH_OrgProxy = orgProxy.PK;
			Factory.Save();
			var bgm = new BGMSegment();
			bgm.DocumentMessageNumber = EDIMessage.MessageNumberPlaceHolder;
			var stowPlanMessage = Factory.New<StowPlanMessage>();
			stowPlanMessage.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Transmit;
			stowPlanMessage.EM_Status = Enterprise.Messaging.Business.EDIMessage.Status.Queued;
			stowPlanMessage.EM_MessageText = bgm.ToString(new UNOACharacterSet());
			stowPlanMessage.EM_GB = GlbBranch.CurrentBranch.PK;
			stowPlanMessage.EM_MessageType = "XXX";
			AssertNull(stowPlanMessage.Interchange);
			Factory.Save();
			var processor = new StowPlanOutgoingMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(CancellationToken.None);
			AssertMessage(stowPlanMessage, "1");
		}

		void AssertMessage(EDIMessage message, ZString interchangeNum)
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
