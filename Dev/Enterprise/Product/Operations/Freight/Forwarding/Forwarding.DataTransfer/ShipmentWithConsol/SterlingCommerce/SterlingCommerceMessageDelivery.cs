using System.IO;
using System.Threading;
using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.MasterFiles.Integration;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class SterlingCommerceMessageDelivery : IMessageProcessor
	{
		public SterlingCommerceMessageDelivery(MessageProcessorCommunicationModesResult modes, BusinessObject bizObjToDeliver, IValueObjectDataAdapter dataAdapter, ProcessTaskNotification action)
			: this(modes, bizObjToDeliver, bizObjToDeliver as IJobNumber, dataAdapter, action)
		{
		}

		public SterlingCommerceMessageDelivery(MessageProcessorCommunicationModesResult modes, BusinessObject bizObjToDeliver, IJobNumber jobNumberSource, IValueObjectDataAdapter dataAdapter, ProcessTaskNotification action)
		{
			this.Modes = modes;
			this.bizObjToDeliver = bizObjToDeliver;
			this.dataAdapter = dataAdapter;

			this.Action = action;

			jobNumberSource = jobNumberSource ?? bizObjToDeliver as IJobNumber;
			if (jobNumberSource != null)
			{
				jobNumber = jobNumberSource.JobNumber;
			}
		}

		MessageProcessorCommunicationModesResult Modes { get; }
		IMessageProcessorCommunicationModesResult IMessageProcessor.GetDestinations() => Modes;

		readonly BusinessObject bizObjToDeliver;
		readonly ZString jobNumber;
		readonly ProcessTaskNotification Action;

#if DEBUG
		internal
#endif
		readonly IValueObjectDataAdapter dataAdapter;

		public void Process(INotifications notifications, CancellationToken token
#if DEBUG
			= new CancellationToken()
#endif
		)
		{
			var deliveryContext = new DeliveryContext(bizObjToDeliver.Factory) { ParentInfo = EntityInfo.New(bizObjToDeliver) };
			foreach (IEDICommunicationsMode mode in Modes.Destinations)
			{
				token.ThrowIfCancellationRequested();
				MemoryStream xmlStream = SerializeXmlToStream(notifications, mode);
				var stelingStream = GetSterlingCommerceStreamFromXml(xmlStream);

				EDIMessageDelivery delivery = new EDIMessageDelivery(jobNumber);
				delivery.Deliver(deliveryContext, mode, new DeliveryStreamWrapperUXML(stelingStream, deliveryContext.ParentInfo));
			}
		}

		protected SubStreamableStream GetSterlingCommerceStreamFromXml(MemoryStream xmlStream)
		{
			XmlTextReader reader = new XmlTextReader(xmlStream);

			Xsd.XmlInterchange interchange;
			Xsd.Shipment shipment = new Xsd.Shipment();
			Xsd.Consol consol = null;

			if (dataAdapter is ForwardingConsolWithShipmentValueObjectDataAdapter)
			{
				XmlValueObjectSerializer serializer = new XmlValueObjectSerializer(typeof(Xsd.Consols));
				Xsd.ConsolCollection consols = ((Xsd.Consols)Xsd.XmlInterchange.DeserializeInterchangeAndPayload(reader, serializer, out interchange)).Consol;
				consol = consols[0];
				shipment = consol.Shipments[0];
			}
			else if (dataAdapter is ForwardingShipmentValueObjectDataAdapter)
			{
				XmlValueObjectSerializer serializer = new XmlValueObjectSerializer(typeof(Xsd.Shipments));
				Xsd.ShipmentCollection shipments = ((Xsd.Shipments)Xsd.XmlInterchange.DeserializeInterchangeAndPayload(reader, serializer, out interchange)).Shipment;
				shipment = shipments[0];
			}
			else
			{
				interchange = Xsd.XmlInterchange.ReadInterchangeOnly(reader, null);
			}

			SterlingCommerceConsolAndShipmentExporter sterling = new SterlingCommerceConsolAndShipmentExporter(new BusinessObjectFactory(), consol, shipment, interchange.InterchangeInfo);

			var result = new MemoryStream();
			try
			{
				sterling.Export(result);
			}
			catch
			{
				result.Dispose();
				throw;
			}

			return (SubStreamableStream)result;
		}

		protected MemoryStream SerializeXmlToStream(INotifications notifications, IEDICommunicationsMode mode)
		{
			XmlValueObjectSerializer serializer = new XmlValueObjectSerializer(dataAdapter.ValueObjectType);
			ZString messagePurpose = Action == null ? ZString.Empty : Action.PQ_MessagePurpose;

			var result = new MemoryStream();
			try
			{
				serializer.ExportXmlData(result, dataAdapter, new BusinessObject[] { bizObjToDeliver }, new ValueObjectExportContext(notifications), mode.EK_LocalPartyVanID, mode.EK_RelatedPartyVanID, messagePurpose);
				result.Position = 0;
			}
			catch
			{
				result.Dispose();
				throw;
			}

			return result;
		}
	}
}

