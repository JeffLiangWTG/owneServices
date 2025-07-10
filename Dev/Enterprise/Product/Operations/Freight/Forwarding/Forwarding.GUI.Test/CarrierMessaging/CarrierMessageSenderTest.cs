using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	class CarrierMessageSenderTest : TestCaseWithFactory
	{
		public void TestSend()
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_Code = "MACARONI";

			var communicationsMode = carrier.EDICommunicationsModes.AddNew();
			communicationsMode.EK_Module = JobInvoicingConsumerTypes.Consol.Code;
			communicationsMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
			communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
			communicationsMode.EK_Destination = "PANDORA";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			Factory.Save();

			ZFormModaliser.ShowDialogsInTest = true;

			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(form =>
			{
				var exportForm = form as ManualDataExportProgressForm;

				if (exportForm != null)
				{
					exportForm.Shown += (s, e) =>
					{
						var sendButton = (ZButton)exportForm.Controls.Find("sendButton", true).First();
						sendButton.PerformClick();
					};
				}
			});

			var validation = new Mock<ICarrierMessagingValidation>();

			validation.Setup(m => m.Validate(It.IsAny<INotifications>()));
			var sender = new CarrierMessageSender();
			sender.Send("Pinging Pandora", consol, validation.Object);

			var query = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, consol.PK);
			query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, "UDM");
			query.AddToFilter(EDIMessageSchema.EM_MessageSubType, "XUS");

			var universalsMessages = Factory.Load<EDIMessage>(query);

			AssertEquals("universal xml has been created", 1, universalsMessages.Length);
			validation.Verify(m => m.Validate(It.IsAny<INotifications>()), Times.Once);
		}
	}
}
