using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public sealed class Shipment : DocDataObject, IShipment
	{
		public Shipment(object identifier)
		: base(identifier)
		{
		}

		#region ShipmentID

		public ZString ShipmentID
		{
			get => shipmentID;
			set
			{
				if (SetNonPersistentPropertyValue(ShipmentIDInfo, ref shipmentID, value))
				{
					Validate(ShipmentIDInfo);
				}
			}
		}

		ZString shipmentID;

		public ZPropertyInfo ShipmentIDInfo => GetZPropertyInfo(nameof(ShipmentID));

		#endregion

		#region Origin

		public IUnloco Origin
		{
			get => origin;
			set => origin = SetChild(origin, value);
		}

		IUnloco origin;

		#endregion

		#region Destination

		public IUnloco Destination
		{
			get => destination;
			set => destination = SetChild(destination, value);
		}

		IUnloco destination;

		#endregion

		#region ConsignorEoriNumber

		public IRegistrationNumber ConsignorEoriNumber
		{
			get => consignorEoriNumber;
			set => consignorEoriNumber = SetChild(consignorEoriNumber, value);
		}

		IRegistrationNumber consignorEoriNumber;

		#endregion

		#region ConsignorEoriBranchSuffix

		public IRegistrationNumber ConsignorEoriBranchSuffix
		{
			get => consignorEoriBranchSuffix;
			set => consignorEoriBranchSuffix = SetChild(ConsignorEoriBranchSuffix, value);
		}

		IRegistrationNumber consignorEoriBranchSuffix;

		#endregion

		#region GoodsValue

		public IMoney GoodsValue
		{
			get => goodsValue;
			set => goodsValue = SetChild(goodsValue, value);
		}

		IMoney goodsValue;

		#endregion

		#region HouseBillNumber

		public ZString HouseBillNumber
		{
			get => houseBillNumber;
			set
			{
				if (SetNonPersistentPropertyValue(HouseBillNumberInfo, ref houseBillNumber, value))
				{
					Validate(HouseBillNumberInfo);
				}
			}
		}

		ZString houseBillNumber;

		public ZPropertyInfo HouseBillNumberInfo => GetZPropertyInfo(nameof(HouseBillNumber));

		#endregion

		#region ITNNumber

		public ZString ITNNumber
		{
			get => itnNumber;
			set
			{
				var formattedValue = string.Join(", ", value.ToString().Split(new string[] { ",", " ", ";", "/", System.Environment.NewLine, "\t" }, StringSplitOptions.RemoveEmptyEntries)).ToUpperInvariant();

				if (SetNonPersistentPropertyValue(ITNNumberInfo, ref itnNumber, formattedValue))
				{
					Validate(ITNNumberInfo);
				}
			}
		}

		ZString itnNumber;

		public ZPropertyInfo ITNNumberInfo => GetZPropertyInfo(nameof(ITNNumber));

		#endregion

		#region CTNNumber

		public ZString CTNNumber
		{
			get => ctnNumber;
			set
			{
				var formattedValue = string.Join(", ", value.ToString().Replace("-", "").Split(new string[] { ",", " ", ";", "/", System.Environment.NewLine, "\t" }, StringSplitOptions.RemoveEmptyEntries)).ToUpperInvariant();
				if (SetNonPersistentPropertyValue(CTNNumberInfo, ref ctnNumber, formattedValue))
				{
					Validate(CTNNumberInfo);
				}
			}
		}

		ZString ctnNumber;

		public ZPropertyInfo CTNNumberInfo => GetZPropertyInfo(nameof(CTNNumber));

		#endregion

		#region DUENumber

		public ZString DUENumber
		{
			get => dueNumber;
			set
			{
				if (SetNonPersistentPropertyValue(DUENumberInfo, ref dueNumber, value))
				{
					Validate(DUENumberInfo);
				}
			}
		}

		ZString dueNumber;

		public ZPropertyInfo DUENumberInfo => GetZPropertyInfo(nameof(DUENumber));

		#endregion

		#region UCRNumber

		public ZString UCRNumber
		{
			get => ucrNumber;
			set
			{
				var formattedValue = string.Join(", ", value.ToString().Split(new string[] { ",", " ", ";", "/", System.Environment.NewLine, "\t" }, StringSplitOptions.RemoveEmptyEntries)).ToUpperInvariant();

				if (SetNonPersistentPropertyValue(UCRNumberInfo, ref ucrNumber, formattedValue))
				{
					Validate(UCRNumberInfo);
				}
			}
		}

		ZString ucrNumber;

		public ZPropertyInfo UCRNumberInfo => GetZPropertyInfo(nameof(UCRNumber));

		#endregion

		#region CTKNumber

		public ZString CTKNumber
		{
			get => ctkNumber;
			set
			{
				var formattedValue = string.Join(", ", value.ToString().Split(new string[] { ",", " " }, StringSplitOptions.RemoveEmptyEntries).Distinct(StringComparer.OrdinalIgnoreCase));

				if (SetNonPersistentPropertyValue(CTKNumberInfo, ref ctkNumber, formattedValue))
				{
					Validate(CTKNumberInfo);
				}
			}
		}

		ZString ctkNumber;

		public ZPropertyInfo CTKNumberInfo => GetZPropertyInfo(nameof(CTKNumber));

		#endregion

		#region ContainerPackingMode

		public ICodeDescription ContainerPackingMode
		{
			get => containerPackingMode;
			set => containerPackingMode = SetChild(containerPackingMode, value);
		}

		ICodeDescription containerPackingMode;

		#endregion

		#region ShipmentType

		public ICodeDescription ShipmentType
		{
			get => shipmentType;
			set => shipmentType = SetChild(shipmentType, value);
		}

		ICodeDescription shipmentType;

		#endregion

		#region ShipperReference

		public ZString ShipperReference
		{
			get => shipperReference;
			set
			{
				if (SetNonPersistentPropertyValue(ShipperReferenceInfo, ref shipperReference, value))
				{
					Validate(ShipperReferenceInfo);
				}
			}
		}

		ZString shipperReference;

		public ZPropertyInfo ShipperReferenceInfo => GetZPropertyInfo(nameof(ShipperReference));

		#endregion

		#region PickRequestedByDate

		public ZDateTime PickRequestedByDate
		{
			get => pickRequestedByDate;
			set
			{
				if (SetNonPersistentPropertyValue(PickRequestedByDateInfo, ref pickRequestedByDate, value))
				{
					Validate(PickRequestedByDateInfo);
				}
			}
		}

		ZDateTime pickRequestedByDate;

		public ZPropertyInfo PickRequestedByDateInfo => GetZPropertyInfo(nameof(PickRequestedByDate));

		#endregion

		#region DeliveryRequiredByDate

		public ZDateTime DeliveryRequiredByDate
		{
			get => deliveryRequiredByDate;
			set
			{
				if (SetNonPersistentPropertyValue(DeliveryRequiredByDateInfo, ref deliveryRequiredByDate, value))
				{
					Validate(DeliveryRequiredByDateInfo);
				}
			}
		}

		ZDateTime deliveryRequiredByDate;

		public ZPropertyInfo DeliveryRequiredByDateInfo => GetZPropertyInfo(nameof(DeliveryRequiredByDate));

		#endregion

		#region Buyer

		public IAddress Buyer
		{
			get => buyer;
			set => buyer = SetChild(buyer, value);
		}

		IAddress buyer;

		#endregion

		#region Supplier

		public IAddress Supplier
		{
			get => supplier;
			set => supplier = SetChild(supplier, value);
		}

		IAddress supplier;

		#endregion

		#region Consignee

		public IAddress Consignee
		{
			get => consignee;
			set => consignee = SetChild(consignee, value);
		}

		IAddress consignee;

		#endregion

		#region Consignor

		public IAddress Consignor
		{
			get => consignor;
			set => consignor = SetChild(consignor, value);
		}

		IAddress consignor;

		#endregion

		#region PickupFrom

		public IAddress PickupFrom
		{
			get => pickupFrom;
			set => pickupFrom = SetChild(pickupFrom, value);
		}

		IAddress pickupFrom;

		#endregion

		#region PickupCFS

		public IAddress PickupCFS
		{
			get => pickupCFS;
			set => pickupCFS = SetChild(pickupCFS, value);
		}

		IAddress pickupCFS;

		#endregion

		#region DeliveryTo

		public IAddress DeliveryTo
		{
			get => deliveryTo;
			set => deliveryTo = SetChild(deliveryTo, value);
		}

		IAddress deliveryTo;

		#endregion

		#region DeliveryCFS

		public IAddress DeliveryCFS
		{
			get => deliveryCFS;
			set => deliveryCFS = SetChild(deliveryCFS, value);
		}

		IAddress deliveryCFS;

		#endregion

		#region NotifyParties

		public IAddress NotifyParty
		{
			get => notifyParty;
			set => notifyParty = SetChild(notifyParty, value);
		}

		IAddress notifyParty;

		public IAddress NotifyParty2
		{
			get => notifyParty2;
			set => notifyParty2 = SetChild(notifyParty2, value);
		}

		IAddress notifyParty2;

		public IAddress NotifyParty3
		{
			get => notifyParty3;
			set => notifyParty3 = SetChild(notifyParty3, value);
		}

		IAddress notifyParty3;

		#endregion

		#region PackingLines

		public IReadOnlyCollection<PackingLine> PackingLines
		{
			get => packingLines;
			set => packingLines = SetChildCollection(packingLines, value);
		}

		IReadOnlyCollection<PackingLine> packingLines;

		IReadOnlyCollection<IPackingLine> IShipment.PackingLines => PackingLines;

		#endregion

		#region AllPackingLinesIncludeCoLoad

		public IEnumerable<PackingLine> AllPackingLinesIncludeCoLoad => GetPackingLines(this);

		IEnumerable<PackingLine> GetPackingLines(Shipment shipment)
		{
			if (shipment.packingLines?.Any(x => x.PackingLines?.Any() ?? false) ?? false)
			{
				foreach (var groupedPackingLine in shipment.packingLines)
				{
					if (groupedPackingLine.PackingLines?.Any() ?? false)
					{
						foreach (var consolidatedPackingLine in groupedPackingLine.PackingLines)
						{
							yield return consolidatedPackingLine;
						}
					}
				}
			}
			else
			{
				var shipmentType = shipment?.ShipmentType?.Code ?? ZString.Empty;

				if (shipment?.PackingLines != null
					&& (shipmentType == Core.Constants.ShipmentTypes.BuyersConsolLead
						|| shipmentType == Core.Constants.ShipmentTypes.ShippersConsolLead
						|| shipmentType == Core.Constants.ShipmentTypes.StandardHouse
						|| shipmentType == Core.Constants.ShipmentTypes.ThirdPartyOwnershipHouse))
				{
					foreach (var packingLine in shipment.PackingLines)
					{
						yield return packingLine;
					}
				}

				if (shipment?.Shipments != null)
				{
					foreach (var coLoadShipment in shipment.Shipments)
					{
						foreach (var packingLine in GetPackingLines(coLoadShipment))
						{
							yield return packingLine;
						}
					}
				}
			}
		}

		#endregion

		#region CoLoadShipments

		public IReadOnlyCollection<Shipment> Shipments
		{
			get => shipments;
			set => shipments = SetChildCollection(shipments, value);
		}

		IReadOnlyCollection<IShipment> IShipment.Shipments => Shipments;

		IReadOnlyCollection<Shipment> shipments;

		#endregion

		#region ExportStatement

		public ZString ExportStatement
		{
			get => exportStatement;
			set
			{
				if (SetNonPersistentPropertyValue(ExportStatementInfo, ref exportStatement, value))
				{
					Validate(ExportStatementInfo);
				}
			}
		}

		ZString exportStatement;

		public ZPropertyInfo ExportStatementInfo => GetZPropertyInfo(nameof(ExportStatement));

		#endregion

		#region ExportStatementCode

		public ZString ExportStatementCode
		{
			get => exportStatementCode;
			set
			{
				if (SetNonPersistentPropertyValue(ExportStatementCodeInfo, ref exportStatementCode, value))
				{
					Validate(ExportStatementCodeInfo);
				}
			}
		}

		ZString exportStatementCode;

		public ZPropertyInfo ExportStatementCodeInfo => GetZPropertyInfo(nameof(ExportStatementCode));

		#endregion

		#region ExportStatementField1Type

		public ZString ExportStatementField1Type
		{
			get => exportStatementField1Type;
			set
			{
				if (SetNonPersistentPropertyValue(ExportStatementField1TypeInfo, ref exportStatementField1Type, value))
				{
					Validate(ExportStatementField1TypeInfo);
				}
			}
		}

		ZString exportStatementField1Type;

		public ZPropertyInfo ExportStatementField1TypeInfo => GetZPropertyInfo(nameof(ExportStatementField1Type));

		#endregion

		#region ExportStatementField1Code

		public ZString ExportStatementField1Code
		{
			get => exportStatementField1Code;
			set
			{
				if (SetNonPersistentPropertyValue(ExportStatementField1CodeInfo, ref exportStatementField1Code, value))
				{
					Validate(ExportStatementField1CodeInfo);
				}
			}
		}

		ZString exportStatementField1Code;

		public ZPropertyInfo ExportStatementField1CodeInfo => GetZPropertyInfo(nameof(ExportStatementField1Code));

		#endregion

		#region ExportStatementField2Type

		public ZString ExportStatementField2Type
		{
			get => exportStatementField2Type;
			set
			{
				if (SetNonPersistentPropertyValue(ExportStatementField2TypeInfo, ref exportStatementField2Type, value))
				{
					Validate(ExportStatementField2TypeInfo);
				}
			}
		}

		ZString exportStatementField2Type;

		public ZPropertyInfo ExportStatementField2TypeInfo => GetZPropertyInfo(nameof(ExportStatementField2Type));

		#endregion

		#region ExportStatementField2Code

		public ZString ExportStatementField2Code
		{
			get => exportStatementField2Code;
			set
			{
				if (SetNonPersistentPropertyValue(ExportStatementField2CodeInfo, ref exportStatementField2Code, value))
				{
					Validate(ExportStatementField2CodeInfo);
				}
			}
		}

		ZString exportStatementField2Code;

		public ZPropertyInfo ExportStatementField2CodeInfo => GetZPropertyInfo(nameof(ExportStatementField2Code));

		#endregion

		#region MRNNumbers

		public IReadOnlyCollection<IReferenceNumber> MRNNumbers
		{
			get => mrnNumbers;
			set => mrnNumbers = SetChild(mrnNumbers, value);
		}

		IReadOnlyCollection<IReferenceNumber> mrnNumbers;

		public ZString MRNNumbersConcatenated
		{
			get => mrnNumbersConcatenated;
			set
			{
				if (SetNonPersistentPropertyValue(MRNNumbersConcatenatedInfo, ref mrnNumbersConcatenated, value))
				{
					Validate(MRNNumbersConcatenatedInfo);
				}
			}
		}

		ZString mrnNumbersConcatenated;

		public ZPropertyInfo MRNNumbersConcatenatedInfo => GetZPropertyInfo(nameof(MRNNumbersConcatenated));

		#endregion

		#region GrossWeight

		public IMeasurement GrossWeight
		{
			get => grossWeight;
			set => grossWeight = SetChild(grossWeight, value);
		}

		IMeasurement grossWeight;

		#endregion

		#region PackCount

		public ZInt PackCount
		{
			get => packCount;
			set
			{
				if (SetNonPersistentPropertyValue(PackCountInfo, ref packCount, value))
				{
				}
			}
		}

		ZInt packCount;

		public ZPropertyInfo PackCountInfo => GetZPropertyInfo(nameof(PackCount));

		#endregion

		#region PackType

		public ICodeDescription PackType
		{
			get => packType;
			set => packType = SetChild(packType, value);
		}
		ICodeDescription packType;

		#endregion

		#region Transports

		public ITransports Transports
		{
			get => transports;
			set => transports = (ITransports)SetChildCollection(transports, value);
		}

		ITransports transports;

		#endregion
	}
}
