using System;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class PortAuthorityMessageTest : BaseAgencyTest
	{
		#region TestNewV20

		public void TestNewV20()
		{
			Mock<IPortAuthorityMessagingData> messageMock;

			SetPortAuthoritySettings(HomePort);
			JobSailing sailing = FindOrCreateSailing(Voyage, HomePort, OverseasPort);

			messageMock = NewMock(PortAuthorityMessageFunction.Original);
			PortAuthorityMessage message = PortAuthorityMessage.New(sailing.Origin, messageMock.Object, PortAuthorityVersionList.Codes.V20, PortMessageTypeList.Codes.Original, ZString.Empty);

			AssertEquals(EDIMessage.ApplicationCodes.PortAuthority, message.EM_ApplicationCode);
			AssertEquals(EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
			AssertEquals(EDIMessage.ApplicationCodes.PortAuthority, message.EM_MessageType);
			AssertEquals(PortMessageTypeList.Codes.Original, message.EM_MessageSubType);
			AssertEquals(EDIMessage.Status.Queued, message.EM_Status);
			AssertMessageEquals(V20_SimpleMessage, message.EM_MessageText);
			AssertEquals(ZString.Empty, message.EM_ApplicationReference);
			messageMock.VerifyAll();
		}

		#endregion

		#region TestNewV11

		public void TestNewV11()
		{
			Mock<IPortAuthorityMessagingData> messageMock;

			SetPortAuthoritySettings(HomePort);
			JobSailing sailing = FindOrCreateSailing(Voyage, HomePort, OverseasPort);

			messageMock = NewMock(PortAuthorityMessageFunction.Original);
			PortAuthorityMessage message = PortAuthorityMessage.New(sailing.Origin, messageMock.Object, PortAuthorityVersionList.Codes.V11, PortMessageTypeList.Codes.Original, ZString.Empty);

			AssertEquals(EDIMessage.ApplicationCodes.PortAuthority, message.EM_ApplicationCode);
			AssertEquals(EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
			AssertEquals(EDIMessage.ApplicationCodes.PortAuthority, message.EM_MessageType);
			AssertEquals(PortMessageTypeList.Codes.Original, message.EM_MessageSubType);
			AssertEquals(EDIMessage.Status.Queued, message.EM_Status);
			AssertMessageEquals(V11_SimpleMessage, message.EM_MessageText);
			AssertEquals(ZString.Empty, message.EM_ApplicationReference);
			messageMock.VerifyAll();
		}

		#endregion

		#region TestMessageSubType

		public void TestMessageSubType()
		{
			SetPortAuthoritySettings(HomePort);
			JobSailing sailing = FindOrCreateSailing(Voyage, HomePort, OverseasPort);

			PortAuthorityMessage message;

			var messageMock1 = NewMock(PortAuthorityMessageFunction.Original);
			message = PortAuthorityMessage.New(sailing.Origin, messageMock1.Object, PortAuthorityVersionList.Codes.V20, PortMessageTypeList.Codes.Original, ZString.Empty);
			AssertEquals(PortMessageTypeList.Codes.Original, message.EM_MessageSubType);

			var messageMock2 = NewMock(PortAuthorityMessageFunction.Original);
			message = PortAuthorityMessage.New(sailing.Origin, messageMock2.Object, PortAuthorityVersionList.Codes.V20, PortMessageTypeList.Codes.Replace, ZString.Empty);
			AssertEquals(PortMessageTypeList.Codes.Replace, message.EM_MessageSubType);

			var messageMock3 = NewMock(PortAuthorityMessageFunction.Original);
			message = PortAuthorityMessage.New(sailing.Origin, messageMock3.Object, PortAuthorityVersionList.Codes.V20, PortMessageTypeList.Codes.Cancellation, ZString.Empty);
			AssertEquals(PortMessageTypeList.Codes.Cancellation, message.EM_MessageSubType);

			messageMock1.VerifyAll();
			messageMock2.VerifyAll();
			messageMock3.VerifyAll();
		}

		#endregion

		#region NewMock
		Mock<IPortAuthorityMessagingData> NewMock(PortAuthorityMessageFunction messageFunction)
		{
			var messageMock = new Mock<IPortAuthorityMessagingData>();
			messageMock.Setup(m => m.MessageFunction).Returns(messageFunction);
			messageMock.Setup(m => m.MessagePrepared).Returns(new DateTime(1997, 06, 30, 11, 03, 31));
			messageMock.Setup(m => m.Voyage).Returns("03S");
			messageMock.Setup(m => m.VesselLloyds).Returns("8506098");
			messageMock.Setup(m => m.VesselName).Returns("AUSTRALIAN ENTERPRISE");
			messageMock.Setup(m => m.Load).Returns("");
			messageMock.Setup(m => m.Discharge).Returns("AUFRE");
			messageMock.Setup(m => m.Equipment).Returns(Array.Empty<IPortAuthorityEquipmentData>());
			messageMock.Setup(m => m.Consignments).Returns(Array.Empty<IPortAuthorityConsignmentData>());
			return messageMock;
		}

		#endregion

		#region Implementation

		#region SimpleMessage

		const string V20_SimpleMessage =
			"UNH+<<MSGNO PLACEHOLDER>>+IFCSUM:D:98B:UN:ANZ20'" +
			"BGM+785+<<MSGNO PLACEHOLDER>>+9+AB'" +
			"DTM+137:199706301103:203'" +
			"CNT+10:0'" +
			"CNT+16:0'" +
			"TDT+20+03S++++++8506098:::AUSTRALIAN ENTERPRISE'" +
			"LOC+11+AUFRE'" +
			"UNT+8+<<MSGNO PLACEHOLDER>>'" +
			"";

		const string V11_SimpleMessage =
			"UNH+<<MSGNO PLACEHOLDER>>+IFCSUM:D:94B:UN:AU11'" +
			"BGM+785+<<MSGNO PLACEHOLDER>>+9+AB'" +
			"DTM+137:199706301103:203'" +
			"CNT+10:0'" +
			"CNT+16:0'" +
			"TDT+20+03S++++++8506098:::AUSTRALIAN ENTERPRISE'" +
			"LOC+11+AUFRE'" +
			"UNT+8+<<MSGNO PLACEHOLDER>>'" +
			"";

		#endregion

		#region Voyage

		JobVoyage Voyage
		{
			get { return voyage ?? (voyage = Factory.New<JobVoyage>()); }
		}
		JobVoyage voyage;

		#endregion

		#endregion
	}

	[TestedType(typeof(PortAuthorityMessage))]
	internal class PortAuthorityMessageBOTest : EnterpriseBusinessObjectTestCase
	{
	}
}
