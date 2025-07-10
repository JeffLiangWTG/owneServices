using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DataObjects;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR
{
	public sealed class CresaWithoutUIMessageSender : DocDataObjectWithoutUIMessageSender
	{
		protected override IMessageInstructions GetMessageInstructions(BusinessObjectFactory factory) => factory.GetCachedValue(nameof(CresaMessageInstructions), () => new CresaMessageInstructions());

		protected override bool IsValidBeforeSending(BusinessObject bizObj, INotifications notifications)
		{
			if (bizObj is ForwardingShipment || IsValidTransitWarehouseJob(bizObj, notifications))
			{
				return true;
			}

			notifications.AddMessageError(Res.GetString("0df6a2d6-c6d0-4da3-9701-04355783dadc", "We cannot send CRESA message from {0}", bizObj.HumanReadableName));
			return false;
		}

		protected override DocDataObject GetDocDataObject(BusinessObject bizObj)
		{
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.English))
			{
				switch (bizObj)
				{
					case ForwardingShipment shipment:
						var builderForShipment = new CresaBuilder(shipment);
						return builderForShipment.Build();

					case WhsItemReceiveConsignment consignment:
						var builderForReceiveConsignment = new CresaBuilderForTransitReceiveConsignment(consignment);
						return builderForReceiveConsignment.Build();

					case WhsItemDispatchConsignment consignment:
						var builderForDispatchConsignment = new CresaBuilderForTransitDispatchConsignment(consignment);
						return builderForDispatchConsignment.Build();
				}
			}
			return null;
		}

		bool IsValidTransitWarehouseJob(BusinessObject bizObj, INotifications notifications)
		{
			var isValid = false;
			if (bizObj is WhsItemReceiveConsignment || bizObj is WhsItemDispatchConsignment)
			{
				isValid = TransitWarehouseHelper.CanSendCRESAMessage(bizObj);
				if (!isValid)
				{
					notifications.AddMessageError(Res.GetString("146b11c6-83ad-41ff-80a4-08966a628ed6", "Unable to send CRESA message when an existing PEN or PAN Governing Reference exists on the consignment", bizObj.HumanReadableName));
				}
			}

			return isValid;
		}

		protected override void ProcessNotifications(INotifications notifications, DocDataObject docDataObject, BusinessObject bizObj)
		{
			if ((docDataObject is Cresa cresa) && (bizObj is WhsItemReceiveConsignment || bizObj is WhsItemDispatchConsignment))
			{
				var noteParent = bizObj as IStmNoteParent;
				var errorMessages = docDataObject.NotificationsIncludingChildren.Select(m => m.Message);
				noteParent.PopulateCRESAMessageNote(cresa.NoteForTransitWarehouse, string.Join(System.Environment.NewLine, errorMessages));
			}
		}

		protected override ZString DataStoreName => ShipmentDocumentDataStoreNames.GoodsReceivedCRESA;
	}
}
