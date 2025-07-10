using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Messaging.Testing
{
	class SPTSMessageBuilderTest : TestCaseWithFactory
	{
		[TestDate(2020, 12, 31)]
		[TestTimeZoneUNLOCO("AUSYD")]
		[ExpectNoExceptions]
		public void TestExpectedXml()
		{
			TestDateAttribute.UseUNLOCO = true;

			var iSPTSBillLines = new Mock<ISPTSBillLines>();
			iSPTSBillLines.Setup(m => m.LineOrderNo).Returns(1);
			iSPTSBillLines.Setup(m => m.LineContainerNo).Returns("CLHU3357890");

			var iSPTSBillLines2 = new Mock<ISPTSBillLines>();
			iSPTSBillLines2.Setup(m => m.LineOrderNo).Returns(2);
			iSPTSBillLines2.Setup(m => m.LineContainerNo).Returns("CLHU3357888");

			var iSPTSBills = new Mock<ISPTSBills>();
			iSPTSBills.Setup(m => m.BillNumber).Returns("003898681");
			iSPTSBills.Setup(m => m.BillOrderNo).Returns(1);
			iSPTSBills.Setup(m => m.DeclarationType).Returns("T");
			iSPTSBills.Setup(m => m.DeclarationNo).Returns("18067777IM000029");
			iSPTSBills.Setup(m => m.IsSubType).Returns("EVET");
			iSPTSBills.Setup(m => m.SPTSBillLines).Returns(new[] { iSPTSBillLines.Object, iSPTSBillLines2.Object });

			var iSPTSBills2 = new Mock<ISPTSBills>();
			iSPTSBills2.Setup(m => m.BillNumber).Returns("003898699");
			iSPTSBills2.Setup(m => m.BillOrderNo).Returns(2);
			iSPTSBills2.Setup(m => m.DeclarationType).Returns("T");
			iSPTSBills2.Setup(m => m.DeclarationNo).Returns("18067777IM000099");
			iSPTSBills2.Setup(m => m.IsSubType).Returns("HAYIR");
			iSPTSBills2.Setup(m => m.SPTSBillLines).Returns((IEnumerable<ISPTSBillLines>)null);

			var iSPTSUlds = new Mock<ISPTSUlds>();
			iSPTSUlds.Setup(m => m.UldNumber).Returns("uld1");
			iSPTSUlds.Setup(m => m.UldOrderNo).Returns(1);

			var iSPTSUlds2 = new Mock<ISPTSUlds>();
			iSPTSUlds2.Setup(m => m.UldNumber).Returns("uld2");
			iSPTSUlds2.Setup(m => m.UldOrderNo).Returns(2);

			var iSPTS = new Mock<ISPTS>();
			iSPTS.Setup(m => m.DestinationPortDCode).Returns("067777");
			iSPTS.Setup(m => m.PortOfPresentationDCode).Returns("066666");
			iSPTS.Setup(m => m.BusinessRegNo).Returns("12453687521");
			iSPTS.Setup(m => m.CarrierBusinessRegNo).Returns("12453687521");
			iSPTS.Setup(m => m.TransportType).Returns("10");
			iSPTS.Setup(m => m.RegistrationNoToBeUpdated).Returns("11117777IM009999");
			iSPTS.Setup(m => m.UserID).Returns("MKM-1");
			iSPTS.Setup(m => m.VoyageNumber).Returns("099");
			iSPTS.Setup(m => m.VoyageDate).Returns(ZDateTime.Today);
			iSPTS.Setup(m => m.XmlRefId).Returns("40426313202");
			iSPTS.Setup(m => m.SPTSBills).Returns(new[] { iSPTSBills.Object, iSPTSBills2.Object });
			iSPTS.Setup(m => m.SPTSUlds).Returns(new[] { iSPTSUlds.Object, iSPTSUlds2.Object });

			var sPTSMessageBuilder = new SPTSMessageBuilder(iSPTS.Object);

			var messageText = TRMessageTestHelper.GetFileText("SPTS.SPTSOutgoingTest.xml");
			var actualMessage = sPTSMessageBuilder.GetMessageText("12345678901", "12345678", "SPT0000001");

			iSPTSBillLines.VerifyAll();
			iSPTSBillLines2.VerifyAll();
			iSPTSBills.VerifyAll();
			iSPTSBills2.VerifyAll();
			iSPTSUlds.VerifyAll();
			iSPTSUlds2.VerifyAll();
			iSPTS.VerifyAll();

			AssertContains("messageText", XmlHelper.IgnoreXmlnsAttrOrder(messageText), actualMessage);
		}
	}
}
