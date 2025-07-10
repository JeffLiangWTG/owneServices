using System;
using System.Collections.Generic;
using Moq;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class EIDOEdifactMessageBuilderTest : BaseAgencyTest
	{
		public void TestContainerised1()
		{
			const string expectedMessage =
				"UNH+<<MSGNO PLACEHOLDER>>+IFCSUM:D:98B:UN:ANZ20'" +
				"BGM+640+<<MSGNO PLACEHOLDER>>+9+AB'" +
				"DTM+137:200801030650:203'" +
				"NAD+AV+PASSWD'" +
				"NAD+MR+CTLPB:160:184'" +
				"NAD+MS+SL:160:184'" +
				"TDT+20+97S+1++SL::184+++9329538'" +
				"LOC+11+AUSYD'" +
				//"DTM+132::102'" +
				"NAD+CA+SL:160:184'" +
				"NAD+SF+CTLPB:160:184'" +
				"CNI+1'" +
				"RFF+AAJ:9DOZY4B009'" +
				"RFF+BM:930136910'" +
				"EQD+CN+SLLU5413278+40GP+++5'" +
				//"UNT+16+<<MSGNO PLACEHOLDER>>'" +
				"UNT+15+<<MSGNO PLACEHOLDER>>'" +
				"";

			var containerMock = new Mock<IEIDOEquiptmentData>(MockBehavior.Strict);
			containerMock.Setup(m => m.ContainerNumber).Returns("SLLU5413278");
			containerMock.Setup(m => m.ContainerISOCode).Returns("40GP");
			containerMock.Setup(m => m.SealNumbers).Returns(new List<string>());
			containerMock.Setup(m => m.GoodsDescription).Returns("");
			containerMock.Setup(m => m.HandlingInstructions).Returns("");
			containerMock.Setup(m => m.IMDGClassCode).Returns("");
			containerMock.Setup(m => m.IMDGClass).Returns("");
			containerMock.Setup(m => m.EmptyReturn).Returns((IEIDOOrganisation)null);
			containerMock.Setup(m => m.GrossKilograms).Returns(0m);
			containerMock.Setup(m => m.IsEmpty).Returns(false);

			var messageRecipientMock = new Mock<IEIDOOrganisation>(MockBehavior.Strict);
			messageRecipientMock.Setup(m => m.AcosCode).Returns("CTLPB");
			messageRecipientMock.Setup(m => m.NameAndAddress).Returns("");

			var messageSenderMock = new Mock<IEIDOOrganisation>(MockBehavior.Strict);
			messageSenderMock.Setup(m => m.AcosCode).Returns("SL");
			messageSenderMock.Setup(m => m.NameAndAddress).Returns("");

			var issuerMock = new Mock<IEIDOOrganisation>(MockBehavior.Strict);
			issuerMock.Setup(m => m.AcosCode).Returns("SL");
			issuerMock.Setup(m => m.NameAndAddress).Returns("");

			var cargoCollectionMock = new Mock<IEIDOOrganisation>(MockBehavior.Strict);
			cargoCollectionMock.Setup(m => m.AcosCode).Returns("CTLPB");
			cargoCollectionMock.Setup(m => m.NameAndAddress).Returns("");

			var messageMock = new Mock<IEIDOMessagingData>(MockBehavior.Strict);
			messageMock.Setup(m => m.MessagePrepared).Returns(new DateTime(2008, 01, 03, 06, 50, 00));
			messageMock.Setup(m => m.MessageRecipient).Returns(messageRecipientMock.Object);
			messageMock.Setup(m => m.MessageSender).Returns(messageSenderMock.Object);
			messageMock.Setup(m => m.Issuer).Returns(issuerMock.Object);
			messageMock.Setup(m => m.CargoCollection).Returns(cargoCollectionMock.Object);
			messageMock.Setup(m => m.Password).Returns("passwd");
			messageMock.Setup(m => m.ReferenceNumber).Returns("");
			messageMock.Setup(m => m.BillOfLading).Returns("930136910");
			messageMock.Setup(m => m.PIN).Returns("9DOZY4B009");
			messageMock.Setup(m => m.CarrierACOS).Returns("SL");
			messageMock.Setup(m => m.CarrierName).Returns("");
			messageMock.Setup(m => m.VesselName).Returns("");
			messageMock.Setup(m => m.VesselLloyds).Returns("9329538");
			messageMock.Setup(m => m.DischargePort).Returns("AUSYD");
			messageMock.Setup(m => m.Voyage).Returns("97S");
			messageMock.Setup(m => m.EstimatedArrivalDate).Returns(new DateTime?());
			messageMock.Setup(m => m.MessageFunction).Returns(EIDOMessageFunction.Original);
			messageMock.Setup(m => m.Equipment).Returns(new IEIDOEquiptmentData[] { containerMock.Object });

			AssertMessageEquals("Containerised Cargo", expectedMessage, Builder.GenerateMessageText(messageMock.Object));

			containerMock.VerifyAll();
			messageRecipientMock.VerifyAll();
			messageSenderMock.VerifyAll();
			issuerMock.VerifyAll();
			cargoCollectionMock.VerifyAll();
			messageMock.VerifyAll();
		}

		public void TestContainerised2()
		{
			const string expectedMessage =
				"UNH+<<MSGNO PLACEHOLDER>>+IFCSUM:D:98B:UN:ANZ20'" +
				"BGM+640+<<MSGNO PLACEHOLDER>>+9+AB'" +
				"DTM+137:200801030943:203'" +
				"NAD+AV+SLC123'" +
				"NAD+MR+ASES1:160:184+PATRICK - ESD'" +
				"NAD+MS+SLC:160:184+SHIPPING LINE COMPANY'" +
				"TDT+20+9801+1++SLC::184:SHIPPING LINE COMPANY+++9294173:::TATIANA SCHULTE'" +
				"LOC+11+AUMEL'" +
				"DTM+132:20070122:102'" +
				"NAD+CA+SLC:160:184+SHIPPING LINE COMPANY'" +
				"NAD+SF+ASES1:160:184+PATRICK - ESD'" +
				//"CTA+IC+:CSD HELPDESK'" +
				//"COM+PAUL@SLC.COM.AU:EM'" +
				"CNI+1'" +
				"RFF+AAJ:956201'" +
				"RFF+BM:SLCUQI252852'" +
				"EQD+CN+SLXU4800838+42R0+++5'" +
				"MEA+AAE+G+KGM:10056'" +
				"SEL+4878251'" +
				//"HAN+GEN'" +
				"FTX+AAA+++KNITTED WEAR'" +
				"NAD+CR+CCCONT:160:184+PPS APPLETON DOCK DEPOT'" +
				"DTM+397:20080101:102'" +
				//"UNT+24+<<MSGNO PLACEHOLDER>>'" +
				"UNT+21+<<MSGNO PLACEHOLDER>>'" +
				"";

			var emptyReturn = new Mock<IEIDOOrganisation>(MockBehavior.Strict);
			emptyReturn.Setup(m => m.AcosCode).Returns("CCCONT");
			emptyReturn.Setup(m => m.NameAndAddress).Returns("PPS APPLETON DOCK DEPOT");

			var containerMock = new Mock<IEIDOEquiptmentData>(MockBehavior.Strict);
			containerMock.Setup(m => m.ContainerNumber).Returns("SLXU4800838");
			containerMock.Setup(m => m.ContainerISOCode).Returns("42R0");
			containerMock.Setup(m => m.SealNumbers).Returns(new List<string> { "4878251" });
			containerMock.Setup(m => m.GoodsDescription).Returns("KNITTED WEAR");
			containerMock.Setup(m => m.HandlingInstructions).Returns("");
			containerMock.Setup(m => m.IMDGClassCode).Returns("");
			containerMock.Setup(m => m.IMDGClass).Returns("");
			containerMock.Setup(m => m.EmptyReturn).Returns(emptyReturn.Object);
			containerMock.Setup(m => m.EmptyReturnBy).Returns(new DateTime(2008, 01, 01, 00, 00, 00));
			containerMock.Setup(m => m.GrossKilograms).Returns(10056m);
			containerMock.Setup(m => m.IsEmpty).Returns(false);

			var messageRecipientMock = new Mock<IEIDOOrganisation>(MockBehavior.Strict);
			messageRecipientMock.Setup(m => m.AcosCode).Returns("ASES1");
			messageRecipientMock.Setup(m => m.NameAndAddress).Returns("Patrick - ESD");

			var messageSenderMock = new Mock<IEIDOOrganisation>(MockBehavior.Strict);
			messageSenderMock.Setup(m => m.AcosCode).Returns("SLC");
			messageSenderMock.Setup(m => m.NameAndAddress).Returns("Shipping Line Company");

			var issuerMock = new Mock<IEIDOOrganisation>(MockBehavior.Strict);
			issuerMock.Setup(m => m.AcosCode).Returns("SLC");
			issuerMock.Setup(m => m.NameAndAddress).Returns("Shipping Line Company");

			var cargoCollectionMock = new Mock<IEIDOOrganisation>(MockBehavior.Strict);
			cargoCollectionMock.Setup(m => m.AcosCode).Returns("ASES1");
			cargoCollectionMock.Setup(m => m.NameAndAddress).Returns("Patrick - ESD");

			var messageMock = new Mock<IEIDOMessagingData>(MockBehavior.Strict);
			messageMock.Setup(m => m.MessagePrepared).Returns(new DateTime(2008, 01, 03, 09, 43, 00));
			messageMock.Setup(m => m.MessageRecipient).Returns(messageRecipientMock.Object);
			messageMock.Setup(m => m.MessageSender).Returns(messageSenderMock.Object);
			messageMock.Setup(m => m.Issuer).Returns(issuerMock.Object);
			messageMock.Setup(m => m.CargoCollection).Returns(cargoCollectionMock.Object);
			messageMock.Setup(m => m.Password).Returns("SLC123");
			messageMock.Setup(m => m.ReferenceNumber).Returns("");
			messageMock.Setup(m => m.BillOfLading).Returns("SLCUQI252852");
			messageMock.Setup(m => m.PIN).Returns("956201");
			messageMock.Setup(m => m.CarrierACOS).Returns("SLC");
			messageMock.Setup(m => m.CarrierName).Returns("Shipping Line Company");
			messageMock.Setup(m => m.VesselName).Returns("TATIANA SCHULTE");
			messageMock.Setup(m => m.VesselLloyds).Returns("9294173");
			messageMock.Setup(m => m.DischargePort).Returns("AUMEL");
			messageMock.Setup(m => m.Voyage).Returns("9801");
			messageMock.Setup(m => m.EstimatedArrivalDate).Returns(new DateTime(2007, 01, 22, 02, 00, 00));
			messageMock.Setup(m => m.MessageFunction).Returns(EIDOMessageFunction.Original);
			messageMock.Setup(m => m.Equipment).Returns(new IEIDOEquiptmentData[] { containerMock.Object });

			AssertMessageEquals("Containerised Cargo", expectedMessage, Builder.GenerateMessageText(messageMock.Object));

			emptyReturn.VerifyAll();
			containerMock.VerifyAll();
			messageRecipientMock.VerifyAll();
			messageSenderMock.VerifyAll();
			issuerMock.VerifyAll();
			cargoCollectionMock.VerifyAll();
			messageMock.VerifyAll();
		}

		public void TestContainerised3()
		{
			const string expectedMessage =
				"UNH+<<MSGNO PLACEHOLDER>>+IFCSUM:D:98B:UN:ANZ20'" +
				"BGM+640+<<MSGNO PLACEHOLDER>>+9+AB'" +
				"DTM+137:200801030943:203'" +
				"NAD+AV+SLC123'" +
				"NAD+MR+ASES1:160:184+PATRICK - ESD'" +
				"NAD+MS+SLC:160:184+SHIPPING LINE COMPANY'" +
				"TDT+20+9801+1++SLC::184:SHIPPING LINE COMPANY+++9294173:::TATIANA SCHULTE'" +
				"LOC+11+AUMEL'" +
				"DTM+132:20070122:102'" +
				"NAD+CA+SLC:160:184+SHIPPING LINE COMPANY'" +
				"NAD+SF+ASES1:160:184+PATRICK - ESD'" +
				//"CTA+IC+:CSD HELPDESK'" +
				//"COM+PAUL@SLC.COM.AU:EM'" +
				"CNI+1'" +
				"RFF+AAJ:28956301'" +
				"RFF+BM:SLCUQI252852'" +
				"EQD+CN+SLCU8289956+45G0+++5'" +
				"MEA+AAE+G+KGM:13108'" +
				"SEL+4878202'" +
				//"HAN+GEN'" +
				"FTX+AAA+++KNITTED WEAR'" +
				"NAD+CR++PPS APPLETON DOCK DEPOT'" +
				"DTM+397:20080101:102'" +
				//"UNT+24+<<MSGNO PLACEHOLDER>>'" +
				"UNT+21+<<MSGNO PLACEHOLDER>>'" +
				"";

			var emptyReturn = new Mock<IEIDOOrganisation>(MockBehavior.Strict);
			emptyReturn.Setup(m => m.AcosCode).Returns("");
			emptyReturn.Setup(m => m.NameAndAddress).Returns("PPS APPLETON DOCK DEPOT");

			var containerMock = new Mock<IEIDOEquiptmentData>(MockBehavior.Strict);
			containerMock.Setup(m => m.ContainerNumber).Returns("SLCU8289956");
			containerMock.Setup(m => m.ContainerISOCode).Returns("45G0");
			containerMock.Setup(m => m.SealNumbers).Returns(new List<string> { "4878202" });
			containerMock.Setup(m => m.GoodsDescription).Returns("KNITTED WEAR");
			containerMock.Setup(m => m.HandlingInstructions).Returns("");
			containerMock.Setup(m => m.IMDGClassCode).Returns("");
			containerMock.Setup(m => m.IMDGClass).Returns("");
			containerMock.Setup(m => m.EmptyReturn).Returns(emptyReturn.Object);
			containerMock.Setup(m => m.EmptyReturnBy).Returns(new DateTime(2008, 01, 01, 02, 30, 00));
			containerMock.Setup(m => m.GrossKilograms).Returns(13108m);
			containerMock.Setup(m => m.IsEmpty).Returns(false);

			var messageRecipientMock = new Mock<IEIDOOrganisation>(MockBehavior.Strict);
			messageRecipientMock.Setup(m => m.AcosCode).Returns("ASES1");
			messageRecipientMock.Setup(m => m.NameAndAddress).Returns("Patrick - ESD");

			var messageSenderMock = new Mock<IEIDOOrganisation>(MockBehavior.Strict);
			messageSenderMock.Setup(m => m.AcosCode).Returns("SLC");
			messageSenderMock.Setup(m => m.NameAndAddress).Returns("Shipping Line Company");

			var issuerMock = new Mock<IEIDOOrganisation>(MockBehavior.Strict);
			issuerMock.Setup(m => m.AcosCode).Returns("SLC");
			issuerMock.Setup(m => m.NameAndAddress).Returns("Shipping Line Company");

			var cargoCollectionMock = new Mock<IEIDOOrganisation>(MockBehavior.Strict);
			cargoCollectionMock.Setup(m => m.AcosCode).Returns("ASES1");
			cargoCollectionMock.Setup(m => m.NameAndAddress).Returns("Patrick - ESD");

			var messageMock = new Mock<IEIDOMessagingData>(MockBehavior.Strict);
			messageMock.Setup(m => m.MessagePrepared).Returns(new DateTime(2008, 01, 03, 09, 43, 00));
			messageMock.Setup(m => m.MessageRecipient).Returns(messageRecipientMock.Object);
			messageMock.Setup(m => m.MessageSender).Returns(messageSenderMock.Object);
			messageMock.Setup(m => m.Issuer).Returns(issuerMock.Object);
			messageMock.Setup(m => m.CargoCollection).Returns(cargoCollectionMock.Object);
			messageMock.Setup(m => m.Password).Returns("SLC123");
			messageMock.Setup(m => m.ReferenceNumber).Returns("");
			messageMock.Setup(m => m.BillOfLading).Returns("SLCUQI252852");
			messageMock.Setup(m => m.PIN).Returns("28956301");
			messageMock.Setup(m => m.CarrierACOS).Returns("SLC");
			messageMock.Setup(m => m.CarrierName).Returns("Shipping Line Company");
			messageMock.Setup(m => m.VesselName).Returns("TATIANA SCHULTE");
			messageMock.Setup(m => m.VesselLloyds).Returns("9294173");
			messageMock.Setup(m => m.DischargePort).Returns("AUMEL");
			messageMock.Setup(m => m.Voyage).Returns("9801");
			messageMock.Setup(m => m.EstimatedArrivalDate).Returns(new DateTime(2007, 01, 22, 02, 00, 00));
			messageMock.Setup(m => m.MessageFunction).Returns(EIDOMessageFunction.Original);
			messageMock.Setup(m => m.Equipment).Returns(new IEIDOEquiptmentData[] { containerMock.Object });

			AssertMessageEquals("Containerised Cargo", expectedMessage, Builder.GenerateMessageText(messageMock.Object));

			emptyReturn.VerifyAll();
			containerMock.VerifyAll();
			messageRecipientMock.VerifyAll();
			messageSenderMock.VerifyAll();
			issuerMock.VerifyAll();
			cargoCollectionMock.VerifyAll();
			messageMock.VerifyAll();
		}

		public void TestOverLength()
		{
			const string expectedMessage =
				"UNH+<<MSGNO PLACEHOLDER>>+IFCSUM:D:98B:UN:ANZ20'" +
				"BGM+640+<<MSGNO PLACEHOLDER>>+9+AB'" +
				"DTM+137:200801030943:203'" +
				"NAD+AV+12345678901234567890123456789012345'" +
				"NAD+MR+12345678901234567890123456789012345:160:184+12345678901234567890123456789012345:67890'" +
				"NAD+MS+12345678901234567890123456789012345:160:184+12345678901234567890123456789012345:67890'" +
				"TDT+20+12345678901234567+1++12345678901234567::184:12345678901234567890123456789012345+++123456789:::12345678901234567890123456789012345'" +
				"LOC+11+1234567890123456789012345'" +
				"DTM+132:20070122:102'" +
				"NAD+CA+12345678901234567890123456789012345:160:184+12345678901234567890123456789012345:67890'" +
				"NAD+SF+12345678901234567890123456789012345:160:184+12345678901234567890123456789012345:67890'" +
				"CNI+1'" +
				"RFF+AAJ:12345678901234567890123456789012345'" +
				"RFF+BM:SLCUQI252852'" +
				"EQD+CN+12345678901234567+1234567890+++5'" +
				"MEA+AAE+G+KGM:13108'" +
				"SEL+1234567890'" +
				"HAN+HAZ:::1234567890123456789012345678901234567890123456789012345678901234567890+1234:::12345678901234567890123456789012345'" +
				"FTX+AAA+++1234567890123456789012345678901234567890123456789012345678901234567890:1234567890'" +
				"NAD+CR+12345678901234567890123456789012345:160:184+12345678901234567890123456789012345:67890'" +
				"DTM+397:20080101:102'" +
				"UNT+22+<<MSGNO PLACEHOLDER>>'" +
				"";

			var emptyReturn = new Mock<IEIDOOrganisation>(MockBehavior.Strict);
			emptyReturn.Setup(m => m.AcosCode).Returns("1234567890123456789012345678901234567890");
			emptyReturn.Setup(m => m.NameAndAddress).Returns("1234567890123456789012345678901234567890");

			var containerMock = new Mock<IEIDOEquiptmentData>(MockBehavior.Strict);
			containerMock.Setup(m => m.ContainerNumber).Returns("12345678901234567890");
			containerMock.Setup(m => m.ContainerISOCode).Returns("12345678901234567890");
			containerMock.Setup(m => m.SealNumbers).Returns(new List<string> { "12345678901234567890" });
			containerMock.Setup(m => m.GoodsDescription).Returns("12345678901234567890123456789012345678901234567890123456789012345678901234567890");
			containerMock.Setup(m => m.HandlingInstructions).Returns("12345678901234567890123456789012345678901234567890123456789012345678901234567890");
			containerMock.Setup(m => m.IMDGClassCode).Returns("1234567890");
			containerMock.Setup(m => m.IMDGClass).Returns("1234567890123456789012345678901234567890");
			containerMock.Setup(m => m.EmptyReturn).Returns(emptyReturn.Object);
			containerMock.Setup(m => m.EmptyReturnBy).Returns(new DateTime(2008, 01, 01, 02, 30, 00));
			containerMock.Setup(m => m.GrossKilograms).Returns(13108m);
			containerMock.Setup(m => m.IsEmpty).Returns(false);

			var messageRecipientMock = new Mock<IEIDOOrganisation>(MockBehavior.Strict);
			messageRecipientMock.Setup(m => m.AcosCode).Returns("1234567890123456789012345678901234567890");
			messageRecipientMock.Setup(m => m.NameAndAddress).Returns("1234567890123456789012345678901234567890");

			var messageSenderMock = new Mock<IEIDOOrganisation>(MockBehavior.Strict);
			messageSenderMock.Setup(m => m.AcosCode).Returns("1234567890123456789012345678901234567890");
			messageSenderMock.Setup(m => m.NameAndAddress).Returns("1234567890123456789012345678901234567890");

			var issuerMock = new Mock<IEIDOOrganisation>(MockBehavior.Strict);
			issuerMock.Setup(m => m.AcosCode).Returns("1234567890123456789012345678901234567890");
			issuerMock.Setup(m => m.NameAndAddress).Returns("1234567890123456789012345678901234567890");

			var cargoCollectionMock = new Mock<IEIDOOrganisation>(MockBehavior.Strict);
			cargoCollectionMock.Setup(m => m.AcosCode).Returns("1234567890123456789012345678901234567890");
			cargoCollectionMock.Setup(m => m.NameAndAddress).Returns("1234567890123456789012345678901234567890");

			var messageMock = new Mock<IEIDOMessagingData>(MockBehavior.Strict);
			messageMock.Setup(m => m.MessagePrepared).Returns(new DateTime(2008, 01, 03, 09, 43, 00));
			messageMock.Setup(m => m.MessageRecipient).Returns(messageRecipientMock.Object);
			messageMock.Setup(m => m.MessageSender).Returns(messageSenderMock.Object);
			messageMock.Setup(m => m.Issuer).Returns(issuerMock.Object);
			messageMock.Setup(m => m.CargoCollection).Returns(cargoCollectionMock.Object);
			messageMock.Setup(m => m.Password).Returns("1234567890123456789012345678901234567890");
			messageMock.Setup(m => m.ReferenceNumber).Returns("");
			messageMock.Setup(m => m.BillOfLading).Returns("SLCUQI252852");
			messageMock.Setup(m => m.PIN).Returns("1234567890123456789012345678901234567890");
			messageMock.Setup(m => m.CarrierACOS).Returns("12345678901234567890");
			messageMock.Setup(m => m.CarrierName).Returns("1234567890123456789012345678901234567890");
			messageMock.Setup(m => m.VesselName).Returns("1234567890123456789012345678901234567890");
			messageMock.Setup(m => m.VesselLloyds).Returns("1234567890");
			messageMock.Setup(m => m.DischargePort).Returns("123456789012345678901234567890");
			messageMock.Setup(m => m.Voyage).Returns("12345678901234567890");
			messageMock.Setup(m => m.EstimatedArrivalDate).Returns(new DateTime(2007, 01, 22, 02, 00, 00));
			messageMock.Setup(m => m.MessageFunction).Returns(EIDOMessageFunction.Original);
			messageMock.Setup(m => m.Equipment).Returns(new IEIDOEquiptmentData[] { containerMock.Object });

			AssertMessageEquals("Containerised Cargo", expectedMessage, Builder.GenerateMessageText(messageMock.Object));

			emptyReturn.VerifyAll();
			containerMock.VerifyAll();
			messageRecipientMock.VerifyAll();
			messageSenderMock.VerifyAll();
			issuerMock.VerifyAll();
			cargoCollectionMock.VerifyAll();
			messageMock.VerifyAll();
		}

		#region Implementation
		EIDOEdifactMessageBuilder Builder
		{
			get { return builder ?? (builder = new EIDOEdifactMessageBuilder()); }
		}
		EIDOEdifactMessageBuilder builder;
		#endregion
	}
}
