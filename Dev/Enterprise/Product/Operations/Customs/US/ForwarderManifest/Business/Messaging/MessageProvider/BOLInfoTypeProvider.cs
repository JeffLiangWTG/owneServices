using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.US.MessageContracts.Interfaces;
using CargoWise.Types;

namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	public class BOLInfoTypeProvider : IBOLInfoType
	{
		readonly USExportAsycudaBill bill;
		readonly ZString action;
		readonly ZString classificationCode;
		ZInt totalQty = 0;
		ZDecimal totalWeight = 0;

		public BOLInfoTypeProvider(USExportAsycudaBill bill, ZString action, ZString classificationCode)
		{
			this.bill = Argument.NotNull(bill, "bill cannot be null");
			this.action = action;
			this.classificationCode = classificationCode;
			CalculateWeightAndQty();
		}

		void CalculateWeightAndQty()
		{
			foreach (var arvHeader in bill.Header.ArrivalHeaders)
			{
				foreach (var arvLine in arvHeader.ArrivalDetails)
				{
					totalQty += arvLine.ATL_Quantity;
					totalWeight += arvLine.ATL_Weight;
				}
			}
		}

		Collection<ILocationType> GetLocationList()
		{
			void AddLocationAndTypeToList(Collection<ILocationType> locationCollection, ZString location, ZString type)
			{
				if (!location.IsEmpty && !type.IsEmpty)
				{
					locationCollection.Add(new LocationTypeProvider(location, type));
				}
			}

			var result = new Collection<ILocationType>();

			AddLocationAndTypeToList(result, bill.ABL_CustomsLoadPort, USExportManifestLocationType.Codes.PortOfLading);
			AddLocationAndTypeToList(result, bill.ABL_CustomsDischargePort, USExportManifestLocationType.Codes.PortOfUnlading);
			AddLocationAndTypeToList(result, bill.Header.AMA_CustomsFirstArrivalPort, USExportManifestLocationType.Codes.PortOfArrival);
			AddLocationAndTypeToList(result, bill.Header.AMA_CustomsFinalDeparturePort, USExportManifestLocationType.Codes.PortOfDeparture);
			AddLocationAndTypeToList(result, bill.ABL_CustomsOriginPort, USExportManifestLocationType.Codes.PortOfOrigin);
			AddLocationAndTypeToList(result, bill.ABL_CustomsFinalDestinationPort, USExportManifestLocationType.Codes.PortOfDestination);
			AddLocationAndTypeToList(result, bill.ABL_LocationInformation, USExportManifestLocationType.Codes.PlaceOfReceipt);

			if (bill.VisitedPorts.Any())
			{
				foreach (var port in bill.VisitedPorts)
				{
					AddLocationAndTypeToList(result, port.CY_Code, USExportManifestLocationType.Codes.PortOfTransit);
				}
			}

			return result;
		}

		Collection<IPartyInfoType> GetPartyList()
		{
			void AddPartyToList(Collection<IPartyInfoType> partyCollection, ZGuid party, ZString partyType, ZString partyName, ZString addr1, ZString addr2, ZString city, ZString state, ZString postal, ZString country, ZString phone)
			{
				if (party != ZGuid.Empty || !partyName.IsEmpty)
				{
					partyCollection.Add(new PartyInfoTypeProvider(
					partyType,
					partyName,
					addr1,
					addr2,
					city,
					state,
					postal,
					country,
					phone));
				}
			}

			var result = new Collection<IPartyInfoType>();

			AddPartyToList(result, bill.ABL_OA_Consignee, USExportManifestPartyType.Codes.Consignee, bill.ABL_ConsigneeName, bill.ABL_ConsigneeStreet1, bill.ABL_ConsigneeStreet2, bill.ABL_ConsigneeCity, bill.ABL_ConsigneeState, bill.ABL_ConsigneePostcode, bill.ABL_RN_NKConsigneeCountry, bill.ABL_ConsigneePhone);
			AddPartyToList(result, bill.ABL_OA_Shipper, USExportManifestPartyType.Codes.Shipper, bill.ABL_ShipperName, bill.ABL_ShipperStreet1, bill.ABL_ShipperStreet2, bill.ABL_ShipperCity, bill.ABL_ShipperState, bill.ABL_ShipperPostcode, bill.ABL_RN_NKShipperCountry, bill.ABL_ShipperPhone);
			AddPartyToList(result, bill.ABL_OA_NotifyParty, USExportManifestPartyType.Codes.NotifyParty, bill.ABL_NotifyPartyName, bill.ABL_NotifyPartyStreet1, bill.ABL_NotifyPartyStreet2, bill.ABL_NotifyPartyCity, bill.ABL_NotifyPartyState, bill.ABL_NotifyPartyPostcode, bill.ABL_RN_NKNotifyPartyCountry, bill.ABL_NotifyPartyPhone);

			return result;
		}

		public IManifestStringType BOLIssuerCode => new ManifestStringTypeProvider(bill.ABL_BillIssuer);

		public IManifestStringType BOLNumber => new ManifestStringTypeProvider(bill.ABL_BillNumber.IsEmpty ? bill.Header.MasterBOL : bill.ABL_BillNumber);

		public IManifestStringType BOLTypeCode => new ManifestStringTypeProvider(bill.ABL_SpecialCargoCode);

		public IManifestStringType BOLClassificationCode => new ManifestStringTypeProvider(classificationCode);

		public IManifestStringType BOLActionType => new ManifestStringTypeProvider(action);

		public IManifestStringType BOLAmendmentReasonCode => new ManifestStringTypeProvider("");

		public IManifestStringType SplitShipmentIndicator => bill.ABL_ManifestQty != totalQty ? new ManifestStringTypeProvider("Y") : new ManifestStringTypeProvider("");

		public IManifestStringType Quantity => new ManifestStringTypeProvider(bill.ABL_ManifestQty.ToString());

		public IManifestStringType QuantityUnitOfMeasure => new ManifestStringTypeProvider(bill.ABL_ManifestUQ);

		public IManifestStringType BoardedQuantity => new ManifestStringTypeProvider(totalQty.ToString());

		public IManifestStringType Weight => new ManifestStringTypeProvider(bill.ABL_GrossWeight.ToZInt().ToString());

		public IManifestStringType WeightUnitOfMeasure => new ManifestStringTypeProvider(bill.ABL_GrossWeightUQ);

		public IManifestStringType BoardedWeight => new ManifestStringTypeProvider(totalWeight.ToZInt().ToString());

		public IManifestStringType Volume => new ManifestStringTypeProvider(bill.ABL_Volume.ToZInt().ToString());

		public IManifestStringType VolumeUnitOfMeasure => new ManifestStringTypeProvider(bill.ABL_VolumeUQ);

		public IManifestStringType ModeOfTransportationCode => new ManifestStringTypeProvider(bill.ABL_InlandTransportMode);

		public Collection<ILocationType> BOLLocationList => GetLocationList();

		public Collection<IReferenceType> BOLReferenceInfoList
		{
			get
			{
				var bolReferenceList = new Collection<IReferenceType>();
				bolReferenceList.Add(new ReferenceTypeProvider(Enterprise.Customs.US.Messaging.Business.ReferenceQualifierList.Codes.CSK, bill.ABL_CustomsDischargePort));

				if (!bill.ABL_UCRNumber.IsEmpty)
				{
					bolReferenceList.Add(new ReferenceTypeProvider(bill.ABL_UCRNumber, string.Empty));
				}

				foreach (CusEntryNumber itnNum in bill.AESITNNumberCollection)
				{
					bolReferenceList.Add(new ReferenceTypeProvider(Enterprise.Customs.Common.US.CusEntryNumberTypeList.Codes.ITN, itnNum.CE_EntryNum));
				}

				foreach (CusEntryNumber inBondNum in bill.InBondNumberCollection)
				{
					bolReferenceList.Add(new ReferenceTypeProvider(InBondNumReferenceType, inBondNum.CE_EntryNum));
				}

				var header = bill.Header;
				var billIssuer = header.MasterBill.ABL_BillIssuer;
				var masterBOL = header.MasterBOL;
				if (!billIssuer.IsEmpty && !masterBOL.IsEmpty)
				{
					bolReferenceList.Add(new ReferenceTypeProvider(OBReferenceType, billIssuer.Left(4) + masterBOL));
				}

				return bolReferenceList;
			}
		}
		public const string InBondNumReferenceType = "IB";
		public const string OBReferenceType = "OB";

		public Collection<IPartyInfoType> BOLPartyInfoList => GetPartyList();

		public Collection<IMovementInfoType> MovementInfoList => new Collection<IMovementInfoType>();

		public Collection<IEquipmentInfoType> EquipmentInfoList
		{
			get
			{
				var result = new Collection<IEquipmentInfoType>();

				foreach (var container in bill.ContainersOnThisBill)
				{
					result.Add(new EquipmentInfoTypeProvider(container, bill.Packs.Where(p => p.ContainerPK == container.PK).OfType<USExportAsycudaPack>()));
				}

				if (result.Count == 0 && bill.Packs.Count > 0)
				{
					result.Add(new EquipmentInfoTypeProvider(bill.Packs.OfType<USExportAsycudaPack>()));
				}

				return result;
			}
		}

		public Collection<IErrorType> ResponseMessage => new Collection<IErrorType>(System.Array.Empty<ErrorTypeProvider>());

		public Collection<IActionType> Action => new Collection<IActionType> { new ActionTypeProvider(action) };
	}
}
