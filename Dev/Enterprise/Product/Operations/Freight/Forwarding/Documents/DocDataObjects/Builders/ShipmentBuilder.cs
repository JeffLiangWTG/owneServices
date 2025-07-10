using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Registry.Business;
using CountryCodes = Enterprise.Core.Constants.CountryCodes;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class ShipmentBuilder
	{
		public ShipmentBuilder(IContext context)
		{
			this.context = context ?? throw new ArgumentNullException(nameof(context));
		}

		readonly IContext context;

		public Shipment Build(ForwardingShipment shipmentBO, Func<ForwardingShipment, IEnumerable<PackingLine>> getPackingLinesFunc, Func<ForwardingShipment, string> getCTKNumberFunc = null)
		{
			if (shipmentBO == null)
			{
				return null;
			}

			var shipment = new Shipment(shipmentBO.PK);
			shipment.Origin = Unloco.Create(context, shipmentBO.Origin);
			shipment.Destination = Unloco.Create(context, shipmentBO.Destination);
			shipment.GoodsValue = new Money
			{
				Amount = shipmentBO.JS_GoodsValue,
				Currency = new CodeDescription(shipmentBO.Lookups.RefCurrency_List) { Code = shipmentBO.JS_RX_NKGoodsValueCurr }
			};
			shipment.ShipmentID = shipmentBO.JS_UniqueConsignRef;
			shipment.HouseBillNumber = shipmentBO.JS_HouseBill;
			shipment.ITNNumber = string.Join(", ", shipmentBO.CusEntryNumbers.Cast<CusEntryNumber>().Where(x => x.CE_EntryType == Customs.Common.US.CusEntryNumberTypeList.Codes.ITN).Select(x => x.CE_EntryNum));
			shipment.DUENumber = string.Join(", ", shipmentBO.CusEntryNumbers.Cast<CusEntryNumber>().Where(x => x.CE_EntryType == CusEntryNumberTypes.Brazil.DUE).Select(x => x.CE_EntryNum));
			shipment.UCRNumber = string.Join(", ", shipmentBO.CusEntryNumbers.Cast<CusEntryNumber>().Where(x => x.CE_EntryType == CusEntryNumberTypes.Standard.UniqueConsignementReference).Select(x => x.CE_EntryNum));
			shipment.CTKNumber = getCTKNumberFunc?.Invoke(shipmentBO) ?? string.Join(", ", shipmentBO.Numbers.Cast<CusEntryNumber>().Where(x => x.CE_EntryType == CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CargoTrackingNote && ((Unloco)shipment.Destination).IsInCountry(x.CE_RN_NKCountryCode)).Select(x => x.CE_EntryNum));
			shipment.CTNNumber = string.Join(",", shipmentBO.CusEntryNumbers.Cast<CusEntryNumber>().Where(x => x.CE_EntryType == CanadaAdditionalReferenceNumberTypes.Codes.CTN).Select(x => x.CE_EntryNum));

			var originCountryCode = shipmentBO.JS_RL_NKOrigin.Left(2);
			if (Core.Constants.TransportModes.Sea.Equals(shipmentBO.JS_TransportMode) &&
				CountryCodes.IsUsaOrTerritory(originCountryCode) &&
				originCountryCode != shipmentBO.JS_RL_NKDestination.Left(2))
			{
				var statements = FreightDataRegistry.Instance.ExportStatementSettings.Value[originCountryCode]?.Statements;
				if (statements != null)
				{
					foreach (ExportStatementSetting statement in statements)
					{
						if (statement.Visibility == ExportStatementSetting.VisibilityList.UserDefined && statement.Code.Equals(shipmentBO.DocsAndCartage.JP_ExportStatement))
						{
							shipment.ExportStatement = statement.Statement;
							shipment.ExportStatementCode = statement.Code;
							break;
						}
					}
				}
			}

			shipment.ContainerPackingMode = new CodeDescription(FreightCodePairLists.JS_PackingModeList(shipmentBO.JS_TransportMode))
			{
				Code = shipmentBO.JS_PackingMode
			};
			shipment.ShipmentType = new CodeDescription(shipmentBO.Lookups.JS_ShipmentType_List)
			{
				Code = shipmentBO.JS_ShipmentType
			};

			shipment.PackingLines = getPackingLinesFunc != null
								? getPackingLinesFunc(shipmentBO).ToArray()
								: GetPackingLines(shipmentBO).ToArray();

			PopulateDates(shipment, shipmentBO);
			PopulateAddresses(shipment, shipmentBO);
			PopulateCoLoadShipments(shipment, shipmentBO, getPackingLinesFunc);
			PopulateTransportsIncludingRelatedInPortOrder(shipment, shipmentBO);

			shipment.PackCount = shipmentBO.JS_OuterPacks;
			shipment.PackType = new CodeDescription(shipmentBO.Lookups.PackTypes)
			{
				Code = shipmentBO.JS_F3_NKPackType
			};

			return shipment;
		}

		void PopulateDates(Shipment shipment, ForwardingShipment shipmentBO)
		{
			shipment.PickRequestedByDate = shipmentBO.DocsAndCartage?.JP_PickupRequiredBy ?? ZDateTime.Empty;
			shipment.DeliveryRequiredByDate = shipmentBO.DocsAndCartage?.JP_DeliveryRequiredBy ?? ZDateTime.Empty;
		}

		void PopulateAddresses(Shipment shipment, ForwardingShipment shipmentBO)
		{
			shipment.Consignee = AddressBuilder.Create(context, shipmentBO.ConsigneeDocumentaryAddress);
			shipment.Consignor = AddressBuilder.Create(context, shipmentBO.ConsignorDocumentaryAddress);
			shipment.PickupFrom = AddressBuilder.Create(context, shipmentBO.ConsignorPickupAddress);
			shipment.PickupCFS = AddressBuilder.Create(context, shipmentBO.ExportReceivingDepot);
			shipment.DeliveryTo = AddressBuilder.Create(context, shipmentBO.ConsigneeDeliveryAddress);
			shipment.DeliveryCFS = AddressBuilder.Create(context, shipmentBO.ImportReleaseDepot);
			shipment.NotifyParty = AddressBuilder.Create(context, shipmentBO.NotifyPartyDocumentaryAddress);
			shipment.NotifyParty2 = AddressBuilder.Create(context, shipmentBO.NotifyParty2DocumentaryAddress);
			shipment.NotifyParty3 = AddressBuilder.Create(context, shipmentBO.NotifyParty3DocumentaryAddress);
		}

		IEnumerable<PackingLine> GetPackingLines(ForwardingShipment shipmentBO)
		{
			var packLineBuilder = new PackingLineBuilder();
			return new List<PackingLine>(shipmentBO.OuterPackLines.Cast<PackLine>().Select(x => packLineBuilder.Build(x)));
		}

		void PopulateCoLoadShipments(Shipment shipment, ForwardingShipment shipmentBO, Func<ForwardingShipment, IEnumerable<PackingLine>> getPackingLinesFunc)
		{
			var shipmentBuilder = new ShipmentBuilder(context);
			var subShipmentDOs = new List<Shipment>(shipmentBO.CoLoadShipments.Cast<ForwardingShipment>().Select(x => shipmentBuilder.Build(x, getPackingLinesFunc)));

			shipment.Shipments = subShipmentDOs;
		}

		void PopulateTransportsIncludingRelatedInPortOrder(Shipment shipment, ForwardingShipment shipmentBO)
		{
			var transportsIncludingRelated = shipmentBO
						.TransportsIncludingRelated
						.OfType<Freight.Business.Transport>()
						.ToArray();
			MovementLegComparer.SortMovementLegsByPorts(transportsIncludingRelated);

			shipment.Transports = Transports.Create(context, transportsIncludingRelated);
		}
	}
}
