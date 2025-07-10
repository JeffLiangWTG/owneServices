using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.MessagingProcess.Testing;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.Customs.ZA.Business.MessageBuilders.CUSCAR;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.Testing
{
	sealed class CUSCARMessageBuilderTest : TestCaseWithFactory
	{
		[TestDate(2023, 10, 16)]
		public void TestICustomsMessageGenerator()
		{
			var bizObj = Factory.New<DummyBizObjWithMessages>();

			var cuscarHeader = new Mock<ICusCarHeader>();
			_ = cuscarHeader.Setup(x => x.ManifestDocumentType).Returns(ManifestDocumentType.COH);
			_ = cuscarHeader.Setup(x => x.Messages).Returns(bizObj.Messages);

			var builder = new CUSCARMessageBuilder(cuscarHeader.Object, MessageSubTypeCodes.Codes.Original, ZString.Empty);
			var generator = builder as Customs.Business.MessagingProcess.ICustomsMessageGenerator;

			AssertNotNull("Builder could not be cast to ICustomsMessageGenerator", generator);

			var msg = generator.GenerateMessage();

			AssertNotNull("Message created", msg);
			AssertContains("UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:16A:UN:RCG001", msg.EM_MessageText);
		}

		public void TestPopulateEQDSegment()
		{
			var bizObj = Factory.New<DummyBizObjWithMessages>();

			var mockContainer1 = new Mock<IZACusCarContainer>();
			_ = mockContainer1.Setup(x => x.ContainerNumber).Returns("CNT0000001");
			_ = mockContainer1.Setup(x => x.ContainerTypeISO).Returns("10GP");
			_ = mockContainer1.Setup(x => x.GrossMassInKilos).Returns(10m);
			_ = mockContainer1.Setup(x => x.GetLandedPurpose()).Returns("AR1");

			var mockContainer2 = new Mock<IZACusCarContainer>();
			_ = mockContainer2.Setup(x => x.ContainerNumber).Returns("CNT0000002");
			_ = mockContainer2.Setup(x => x.ContainerTypeISO).Returns("20GP");
			_ = mockContainer2.Setup(x => x.GrossMassInKilos).Returns(20m);
			_ = mockContainer2.Setup(x => x.GetLandedPurpose()).Returns("3");

			var cuscarHeader = new Mock<ICusCarHeader>();
			_ = cuscarHeader.Setup(x => x.ManifestDocumentType).Returns(ManifestDocumentType.ALH);
			_ = cuscarHeader.Setup(x => x.Messages).Returns(bizObj.Messages);
			_ = cuscarHeader.Setup(x => x.GetContainersByBillIssuer("BILL1")).Returns(new[] { mockContainer1.Object, mockContainer2.Object });

			var builder = new CUSCARMessageBuilder(cuscarHeader.Object, MessageSubTypeCodes.Codes.Original, "BILL1");
			var generator = builder as Customs.Business.MessagingProcess.ICustomsMessageGenerator;

			var msg = generator.GenerateMessage();

			AssertNotNull("Message created", msg);
			AssertContains("EQD+CN+CNT0000001+10GP:102++AR1+0", msg.EM_MessageText);
			AssertContains("EQD+CN+CNT0000002+20GP:102++3+0", msg.EM_MessageText);
		}
	}
}
