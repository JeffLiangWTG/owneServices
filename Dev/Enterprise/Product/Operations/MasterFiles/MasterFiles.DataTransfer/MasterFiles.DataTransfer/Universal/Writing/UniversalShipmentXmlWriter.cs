using System.IO;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.XmlIO.XmlWriting;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public class UniversalShipmentXmlWriter : IUniversalShipmentXmlWriter
	{
		public UniversalShipmentXmlWriter(ITopLevelDataObjectWriter dataWriter, BusinessObject exportedBO, string destination, string filename)
		{
			this.dataWriter = Argument.NotNull(dataWriter, "ITopLevelDataObjectWriter dataWriter");
			this.exportedBO = Argument.NotNull(exportedBO, "BusinessObject exportedBO");
			this.destination = Argument.NotNull(destination, "string destination");
			this.filename = Argument.NotNull(filename, "string filename");
		}

		protected readonly ITopLevelDataObjectWriter dataWriter;
		protected readonly BusinessObject exportedBO;
		protected readonly ZString destination;
		protected readonly ZString filename;

		public void Process(INotifications notifications, CancellationToken token
#if DEBUG
			= new CancellationToken()
#endif
		)
		{
			var exportedData = (TopLevelDataObject)dataWriter.GetDataObject(exportedBO);

			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				new XmlWriter().WriteXML(exportedData, stream);

				var actionInfo = new ActionInfo(null, exportedBO)
				{
					ActionType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML,
					TriggerActualDate = ZDateTimeOffset.Now,
				};

				new DataContextDataObjectWriter().PopulateDataObject(actionInfo, exportedData.DataContext);

				var jobNumber = JobNumberResolver.GetJobNumber(exportedBO);
				var delivery = (!jobNumber.IsEmpty) ? new EDIMessageDelivery(jobNumber) : new EDIMessageDelivery();

				var communicationMode = new SendXMLCommsMode()
				{
					EK_Destination = destination,
					EK_Filename = filename
				};

				var context = new DeliveryContext(exportedBO.Factory)
				{
					ParentInfo = EntityInfo.New(exportedBO),
					ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging,
					MessageTypeCode = EDIMessageTypeList.Codes.XDC,
					MessageSubTypeCode = EDIMessageSubTypeList.Codes.XmlUniversalShipment,
					Notifications = notifications
				};

				delivery.Deliver(context, communicationMode, new DeliveryStreamWrapperUXML(stream, context.ParentInfo));
			}
		}

		class SendXMLCommsMode : IEDICommunicationsMode
		{
			public ZGuid EK_ECC_CommunicationPartyConfig { get; set; }

			public ZString EK_CommunicationsTransport { get { return EDICommunicationsModeCommunicationsTransportList.Codes.EHubService; } }
			public ZString EK_FileFormat { get { return EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment; } }
			public ZString EK_Destination { get; set; }
			public ZDateTime EK_LastFailed { get; set; }
			public ZString EK_Filename { get; set; }

			ZString IEDICommunicationsMode.EK_LocalPartyVanID { get { return ZString.Empty; } }
			ZString IEDICommunicationsMode.EK_LoginName { get { return ZString.Empty; } }
			ZString IEDICommunicationsMode.EK_MessagePurpose { get { return ZString.Empty; } }
			ZString IEDICommunicationsMode.EK_Password { get { return ZString.Empty; } }
			ZInt IEDICommunicationsMode.EK_PortNumber { get { return ZInt.Zero; } }
			ZBool IEDICommunicationsMode.EK_PublishInternalMilestones { get { return false; } }
			ZString IEDICommunicationsMode.EK_RelatedPartyVanID { get { return ZString.Empty; } }
			ZString IEDICommunicationsMode.EK_ServerAddressSubject { get { return ZString.Empty; } }
			IOrgHeader IMessageDestinationSource.Organisation => null;
		}
	}
}
