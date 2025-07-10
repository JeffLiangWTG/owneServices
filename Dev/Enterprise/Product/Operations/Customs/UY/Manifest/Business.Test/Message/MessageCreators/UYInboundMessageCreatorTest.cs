using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.UY.Manifest.Business.Testing
{
	sealed class UYInboundMessageCreatorTest : TestCaseWithFactory
	{
		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateMesssage()
		{
			var interchange = CreateInterchange(new ZGuid(), EDIInterchange.Direction.Receive, EDIInterchangeStatusList.Codes.Queued);
			var creator = new UYInboundMessageCreator();
			creator.CreateMessagesForInterchange(interchange);
			interchange.Reload();
			AssertMessage(interchange);
		}

		void AssertMessage(EDIInterchange interchange)
		{
			var message = interchange.ContainedMessages[0];
			CombineAssertions(() =>
			{
				AssertNotNull(message.EM_GE);

				AssertEquals(message.EM_GB, interchange.EI_GB);
				AssertEquals(message.EM_EI, interchange.PK);
				AssertEquals(message.EM_MessageNum, interchange.EI_InterchangeNum.Right(EDIMessage.Schema.EM_MessageNumMaxLength));
			});
		}

		UYCInterchange CreateInterchange(ZGuid sessionGUID, ZString direction, ZString status)
		{
			var interchange = Factory.New<UYCInterchange>();
			interchange.EI_Status = status;
			interchange.EI_IsActive = true;
			interchange.EI_ReceiveTransmit = direction;
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.UYCustoms;
			interchange.EI_InterchangeType = MessageTypes.Codes.UYC;
			interchange.EI_From = UYMessageConstants.InterchangeToTest;
			interchange.EI_To = "eHub";
			interchange.EI_GB = GlbBranch.CurrentBranch.PK;
			interchange.EI_SessionGUID = sessionGUID;
			interchange.EI_BodyText = DAETestingHelper.GetExpectedMessageXML(Path.Combine(BaseSourcePath, DAETestingConstants.SampleWithEnvelope));
			interchange.EI_HeaderText = DAETestingHelper.GetExpectedMessageXML(Path.Combine(BaseSourcePath, DAETestingConstants.HeaderText));

			Factory.Save();
			return interchange;
		}
	}
}
