using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.PortMessaging.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Forwarding.PortMessaging.DataTransfer
{
	class PortMessagingShipmentDataObjectForConsolWriter : PortMessagingShipmentDataObjectWriter
	{
		public PortMessagingShipmentDataObjectForConsolWriter(IDataWritingManager manager, bool checkSubShipments, bool checkForParent, PortMessagingManager.MessageType messageType, string purpose)
			: base(manager, checkSubShipments, checkForParent, messageType, purpose)
		{
		}

		protected override void PopulateDataObject(ForwardingShipment shipmentBO, Shipment shipmentData)
		{
			base.PopulateDataObject(shipmentBO, shipmentData);

			var nonFCLCoLoadShipmentsSZBNumbers = shipmentBO.CoLoadShipments.Cast<ForwardingShipment>()
				.Where(coLoadShipment => coLoadShipment.JS_PackingMode != Core.Constants.ContainerModes.FCL && !coLoadShipment.JS_SZB.IsEmpty)
				.Select(coLoadShipment => coLoadShipment.JS_SZB);

			if (nonFCLCoLoadShipmentsSZBNumbers.Any())
			{
				shipmentData.SetAdditionalReferenceCollection(() =>
				{
					var additionalReferenceCollection = shipmentData.AdditionalReferenceCollection ?? new DataObjectList<AdditionalReference>();

					nonFCLCoLoadShipmentsSZBNumbers.ForEach(szbNumber =>
					{
						var additionalReference = new AdditionalReference
						{
							Type = new EntryType
							{
								Code = GermanyAdditionalReferenceNumberTypes.Codes.SZBNumber,
								Description = GermanyAdditionalReferenceNumberTypes.Descriptions.SZBNumber
							},
							ReferenceNumber = szbNumber
						};

						additionalReferenceCollection.Add(additionalReference);
					});

					return additionalReferenceCollection;
				});
			}
		}

		protected override IEnumerable<ForwardingPackLine> GetAllPackingLinesIncludeCoLoad(ForwardingShipment shipmentBO)
		{
			var shipmentType = shipmentBO?.JS_ShipmentType ?? ZString.Empty;

			if (shipmentBO?.OuterPackLines != null)
			{
				foreach (ForwardingPackLine packingLine in shipmentBO.OuterPackLines)
				{
					yield return packingLine;
				}
			}

			if (shipmentType == Core.Constants.ShipmentTypes.BuyersConsolLead && shipmentBO?.CoLoadShipments != null)
			{
				foreach (ForwardingShipment coLoadShipment in shipmentBO.CoLoadShipments)
				{
					foreach (var packingLine in GetAllPackingLinesIncludeCoLoad(coLoadShipment))
					{
						yield return packingLine;
					}
				}
			}
		}
	}
}
