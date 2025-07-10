using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Messaging;
using Enterprise.Customs.TW.Messaging.MessageBuilders;

namespace Enterprise.Customs.TW.Manifest.Business
{
	public class N5101HMessageSendingObject : BaseMessageSendingObject, IN5101HDeclaration
	{
		readonly MessageSendingObjectCollection sendingObjects;
		readonly MessageSendingObjectParent sendingObjectParent;

		public N5101HMessageSendingObject(AsycudaManifestHeader header, MessageSendingObjectCollection sendingObjects, MessageSendingObjectParent parent) : base(header.Factory)
		{
			Header = Argument.NotNull(header, nameof(header));
			this.sendingObjects = sendingObjects;
			this.sendingObjectParent = parent;
		}

		[BusinessObjectTestExclude]
		public ZString MessageType => MessageTypeList.Codes.FHM;

		public IEnumerable<AsycudaBill> Bills => sendingObjects.Cast<MessageSendingObject>().Select(x => x.Bill);

		public AsycudaManifestHeader Header { get; }

		public ZString FunctionalReferenceID => MessageConstants.FunctionalReferenceIDPlaceHolder;

		public ZString FunctionCode => sendingObjects.Cast<MessageSendingObject>().FirstOrDefault().Action;

		public ZString StatusCode => sendingObjectParent.IsFinalManifest ? YesNoList.Codes.Yes : YesNoList.Codes.No;

		public ITransportMeans BorderTransportMeans => new N5101HBorderTransportMeans(Header);

		public ZString CarrierId => Header.AMA_CarrierCode;

		public IEnumerable<IN5101HConsignment> Consignments => sendingObjects.Cast<MessageSendingObject>().Select(x => new N5101HConsignment(x.Bill, sendingObjects.Count));

		public ZString DeconsolidatorId => Header.AMA_DeconsolidateVAT;

		public IN5101HGoodsShipment GoodsShipment => new N5101HGoodsShipment(new N5101HGoodsShipmentConsignment(Header.MasterBill), Header.AMA_CustomsOffice);

		public ZDate UnloadingLocationArrivalDateTime => Header.AMA_E_ARV.Date;

		public ZString SerializeToMessageString()
		{
			return new N5101HMessageBuilder().PopulateXml(this);
		}
	}
}
