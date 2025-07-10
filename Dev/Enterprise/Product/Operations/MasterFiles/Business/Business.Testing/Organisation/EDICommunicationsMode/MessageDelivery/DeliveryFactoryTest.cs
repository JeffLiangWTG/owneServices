using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Moq;

namespace Enterprise.MasterFiles.Business.MessageDelivery.Testing
{
	public sealed class DeliveryFactoryTest : TestCaseWithFactory
	{
		public void TestBuild()
		{
			mode.Setup(m => m.EK_CommunicationsTransport).Returns(EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface);
			mode.Setup(m => m.EK_Destination).Returns("http://localhost");
			var delivery = DeliveryFactory.Build(mode.Object);
			AssertEquals("Delivery type", typeof(EAdaptorDelivery), delivery.GetType());
		}

		public void TestBuild_NativeXMLConnectorWithLicense()
		{
			Enterprise.Registry.Business.eHubMessagingRegistry.Instance.HasNativeXMLConnector = true;

			mode.Setup(m => m.EK_CommunicationsTransport).Returns(EDICommunicationsModeCommunicationsTransportList.Codes.NativeXMLConnector);
			mode.Setup(m => m.EK_Destination).Returns("http://localhost");
			var delivery = DeliveryFactory.Build(mode.Object);
			AssertEquals("Delivery type", "NativeWebServiceDelivery", delivery.GetType().Name);
		}

		public void TestBuild_NativeXMLConnectorWithoutLicense()
		{
			Enterprise.Registry.Business.eHubMessagingRegistry.Instance.HasNativeXMLConnector = false;
			var dummyOrganisation = Factory.New<OrgHeader>();
			dummyOrganisation.OH_Code = "HHGTTGDP42";
			dummyOrganisation.OH_FullName = "Restaurant At The End Of The Universe";

			mode.Setup(m => m.EK_CommunicationsTransport).Returns(EDICommunicationsModeCommunicationsTransportList.Codes.NativeXMLConnector);
			mode.Setup(m => m.EK_Destination).Returns("http://localhost");
			mode.Setup(m => m.Organisation).Returns(dummyOrganisation);
			AssertExceptionThrown(typeof(NotAllowedException)
				, NativeXMLConnectorPermissionFailureMessage
				, delegate
				{ DeliveryFactory.Build(mode.Object); });
		}

		public const string NativeXMLConnectorPermissionFailureMessage = "Cannot Send Via Native XML Connector as it is not enabled on this system. Please check the EDI Communications Modes configured on the Organization [HHGTTGDP42 - Restaurant At The End Of The Universe].";

		public void TestBuild_SendFTPWithLicense()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);

			mode.Setup(m => m.EK_CommunicationsTransport).Returns(EDICommunicationsModeCommunicationsTransportList.Codes.FTP);
			mode.Setup(m => m.EK_Destination).Returns("http://localhost");

			var delivery = DeliveryFactory.Build(mode.Object);
			AssertEquals("Delivery type", "FtpDelivery", delivery.GetType().Name);
		}

		public void TestBuild_SendFTPWithoutLicense()
		{
			var empty = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Empty };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, empty);
			var dummyOrganisation = Factory.New<OrgHeader>();
			dummyOrganisation.OH_Code = "HHGTTGDP42";
			dummyOrganisation.OH_FullName = "Restaurant At The End Of The Universe";

			mode.Setup(m => m.EK_CommunicationsTransport).Returns(EDICommunicationsModeCommunicationsTransportList.Codes.FTP);
			mode.Setup(m => m.EK_Destination).Returns("http://localhost");
			mode.Setup(m => m.Organisation).Returns(dummyOrganisation);
			AssertExceptionThrown(typeof(NotAllowedException)
				, FTPInterfaceConnectorPermissionFailureMessage
				, delegate
				{ DeliveryFactory.Build(mode.Object); });
		}

		const string FTPInterfaceConnectorPermissionFailureMessage = "Cannot Send Via FTP as this system does not have an Interface Connector License. Please check the EDI Communications Modes configured on the Organization [HHGTTGDP42 - Restaurant At The End Of The Universe].";

		public void TestBuild_SendFileWithLicense()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);

			mode.Setup(m => m.EK_CommunicationsTransport).Returns(EDICommunicationsModeCommunicationsTransportList.Codes.SaveToFile);
			mode.Setup(m => m.EK_Destination).Returns("http://localhost");

			var delivery = DeliveryFactory.Build(mode.Object);
			AssertEquals("Delivery type", "FileDelivery", delivery.GetType().Name);
		}

		public void TestBuild_SendFileWithoutLicense()
		{
			var empty = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Empty };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, empty);
			var dummyOrganisation = Factory.New<OrgHeader>();
			dummyOrganisation.OH_Code = "HHGTTGDP42";
			dummyOrganisation.OH_FullName = "Restaurant At The End Of The Universe";

			mode.Setup(m => m.EK_CommunicationsTransport).Returns(EDICommunicationsModeCommunicationsTransportList.Codes.SaveToFile);
			mode.Setup(m => m.EK_Destination).Returns("http://localhost");
			mode.Setup(m => m.Organisation).Returns(dummyOrganisation);
			AssertExceptionThrown(typeof(NotAllowedException)
				, SaveToFileInterfaceConnectorPermissionFailureMessage
				, delegate
				{ DeliveryFactory.Build(mode.Object); });
		}

		const string SaveToFileInterfaceConnectorPermissionFailureMessage = "Cannot Save to File as this system does not have an Interface Connector License. Please check the EDI Communications Modes configured on the Organization [HHGTTGDP42 - Restaurant At The End Of The Universe].";

		public void TestBuild_Email()
		{
			mode.Setup(m => m.EK_CommunicationsTransport).Returns(EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText);
			mode.Setup(m => m.EK_Destination).Returns("slartibartfast@hhgttg.org");
			var specificDelivery = DeliveryFactory.Build(mode.Object);
			AssertEquals("Delivery type", typeof(EmailDelivery), specificDelivery.GetType());
		}

		protected override void SetUp()
		{
			base.SetUp();
			mode = new Mock<IEDICommunicationsMode>();
		}

		Mock<IEDICommunicationsMode> mode;
	}
}
