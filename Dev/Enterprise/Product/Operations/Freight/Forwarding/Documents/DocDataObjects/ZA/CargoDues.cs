namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.ZA
{
	using System.Collections.Generic;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.DocumentVisualizer.DocDataObjects;
	using Enterprise.Freight.Forwarding.Documents.DataObjects;

	public class CargoDues : DocDataObject, IDataSourceProvider
	{
		public CargoDues(ZString sourceType, ZString sourceID, ZString documentName)
		{
			SourceType = sourceType;
			SourceID = sourceID;
			DocumentName = documentName;
		}

		#region IDataSourceProvider members

		public ZString SourceID { get; }
		public ZString SourceType { get; }
		public ZString DocumentName { get; }

		#endregion

		#region Direction

		public ZString Direction
		{
			get => direction;
			set
			{
				if (SetNonPersistentPropertyValue(DirectionInfo, ref direction, value))
				{
					Validate(DirectionInfo);
				}
			}
		}

		ZString direction;

		public ZPropertyInfo DirectionInfo => GetZPropertyInfo(nameof(Direction));

		#endregion

		public ZBool IsQuotationDocument { get; set; }
		public ZBool IsExportDocument { get; set; }

		public ZBool IsDeepSea { get; set; }
		public ZBool IsContainerised { get; set; }
		public ZBool IsTranship { get; set; }
		public ZBool IsCoastwise { get; set; }
		public ZBool IsBulk { get; set; }
		public ZBool IsBreakBulk { get; set; }
		public ZBool CancellingOrder { get; set; }

		#region Addresses

		public Address Agent
		{
			get => agent;
			set => agent = SetChild(agent, value);
		}
		Address agent;

		public Address Shipper
		{
			get => shipper;
			set => shipper = SetChild(shipper, value);
		}
		Address shipper;

		public Address Consignee
		{
			get => consignee;
			set => consignee = SetChild(consignee, value);
		}
		Address consignee;

		public Address CurrentUser
		{
			get => currentUser;
			set => currentUser = SetChild(currentUser, value);
		}
		Address currentUser;

		public Address ArrivalCTO
		{
			get => arrivalCTO;
			set => arrivalCTO = SetChild(arrivalCTO, value);
		}
		Address arrivalCTO;

		public Address DepartureCTO
		{
			get => departureCTO;
			set => departureCTO = SetChild(departureCTO, value);
		}

		Address departureCTO;

		public Address CustomsContainerTerminalOperator
		{
			get => customsContainerTerminalOperator;
			set => customsContainerTerminalOperator = SetChild(customsContainerTerminalOperator, value);
		}

		Address customsContainerTerminalOperator;

		public Address ShippingLine
		{
			get => shippingLine;
			set => shippingLine = SetChild(shippingLine, value);
		}
		Address shippingLine;

		#endregion

		#region TNPAOrderNumber

		public ZString TNPAOrderNumber
		{
			get => tnpaOrderNumber;
			set
			{
				if (SetNonPersistentPropertyValue(TNPAOrderNumberInfo, ref tnpaOrderNumber, value))
				{
					Validate(TNPAOrderNumberInfo);
				}
			}
		}

		ZString tnpaOrderNumber;

		public ZPropertyInfo TNPAOrderNumberInfo => GetZPropertyInfo(nameof(TNPAOrderNumber));

		#endregion

		#region TNPAAccountNumber

		public ZString TNPAAccountNumber
		{
			get => tnpaAccountNumber;
			set
			{
				if (SetNonPersistentPropertyValue(TNPAAccountNumberInfo, ref tnpaAccountNumber, value))
				{
					Validate(TNPAAccountNumberInfo);
				}
			}
		}

		ZString tnpaAccountNumber;

		public ZPropertyInfo TNPAAccountNumberInfo => GetZPropertyInfo(nameof(TNPAAccountNumber));

		#endregion

		#region TNPAQuotationNumer

		public ZString TNPAQuotationNumber
		{
			get => tnpaQuotationNumber;
			set
			{
				if (SetNonPersistentPropertyValue(TNPAQuotationNumberInfo, ref tnpaQuotationNumber, value))
				{
					Validate(TNPAQuotationNumberInfo);
				}
			}
		}

		ZString tnpaQuotationNumber;

		public ZPropertyInfo TNPAQuotationNumberInfo => GetZPropertyInfo(nameof(TNPAQuotationNumber));

		#endregion

		#region TNPAArrivalNumber

		public ZString TNPAArrivalNumber
		{
			get => tnpaArrivalNumber;
			set
			{
				if (SetNonPersistentPropertyValue(TNPAArrivalNumberInfo, ref tnpaArrivalNumber, value))
				{
					Validate(TNPAArrivalNumberInfo);
				}
			}
		}

		ZString tnpaArrivalNumber;

		public ZPropertyInfo TNPAArrivalNumberInfo => GetZPropertyInfo(nameof(TNPAArrivalNumber));

		#endregion

		#region IMONumber

		public ZString IMONumber
		{
			get => imoNumber;
			set
			{
				if (SetNonPersistentPropertyValue(IMONumberInfo, ref imoNumber, value))
				{
					Validate(IMONumberInfo);
				}
			}
		}

		ZString imoNumber;

		public ZPropertyInfo IMONumberInfo => GetZPropertyInfo(nameof(IMONumber));

		#endregion

		#region RadioCallSign

		public ZString RadioCallSign
		{
			get => radioCallSign;
			set
			{
				if (SetNonPersistentPropertyValue(RadioCallSignInfo, ref radioCallSign, value))
				{
					Validate(RadioCallSignInfo);
				}
			}
		}

		ZString radioCallSign;

		public ZPropertyInfo RadioCallSignInfo => GetZPropertyInfo(nameof(RadioCallSign));

		#endregion

		#region CarrierCode

		public ZString CarrierCode
		{
			get => carrierCode;
			set
			{
				if (SetNonPersistentPropertyValue(CarrierCodeInfo, ref carrierCode, value))
				{
					Validate(CarrierCodeInfo);
				}
			}
		}

		ZString carrierCode;

		public ZPropertyInfo CarrierCodeInfo => GetZPropertyInfo(nameof(CarrierCode));

		#endregion

		#region Eta

		public ZDateTime Eta
		{
			get => eta;
			set
			{
				if (SetNonPersistentPropertyValue(EtaInfo, ref eta, value))
				{
					Validate(EtaInfo);
				}
			}
		}

		ZDateTime eta;

		public ZPropertyInfo EtaInfo => GetZPropertyInfo(nameof(Eta));

		#endregion

		#region Etd

		public ZDateTime Etd
		{
			get => etd;
			set
			{
				if (SetNonPersistentPropertyValue(EtdInfo, ref etd, value))
				{
					Validate(EtdInfo);
				}
			}
		}

		ZDateTime etd;

		public ZPropertyInfo EtdInfo => GetZPropertyInfo(nameof(Etd));

		#endregion

		#region VesselAndOnCarrierInfo

		public Unloco PortOfDischarge
		{
			get => portOfDischarge;
			set => portOfDischarge = SetChild(portOfDischarge, value);
		}
		Unloco portOfDischarge;

		public Unloco PortOfLoading
		{
			get => portOfLoading;
			set => portOfLoading = SetChild(portOfLoading, value);
		}
		Unloco portOfLoading;

		public Unloco PlaceOfReceipt
		{
			get => placeOfReceipt;
			set => placeOfReceipt = SetChild(placeOfReceipt, value);
		}
		Unloco placeOfReceipt;

		public Unloco PlaceOfDelivery
		{
			get => placeOfDelivery;
			set => placeOfDelivery = SetChild(placeOfDelivery, value);
		}
		Unloco placeOfDelivery;

		public Unloco ServicePort
		{
			get => servicePort;
			set => servicePort = SetChild(servicePort, value);
		}
		Unloco servicePort;

		#endregion

		#region ContainerMode

		public CodeDescription ContainerMode
		{
			get => containerMode;
			set => containerMode = SetChild(ContainerMode, value);
		}
		CodeDescription containerMode;

		#endregion

		#region ClientRef

		public ZString ClientRef
		{
			get => clientRef;
			set
			{
				if (SetNonPersistentPropertyValue(ClientRefInfo, ref clientRef, value))
				{
					Validate(ClientRefInfo);
				}
			}
		}

		ZString clientRef;

		public ZPropertyInfo ClientRefInfo => GetZPropertyInfo(nameof(ClientRef));

		#endregion

		#region WayBillNumber

		public ZString WayBillNumber
		{
			get => wayBillNumber;
			set
			{
				if (SetNonPersistentPropertyValue(WayBillNumberInfo, ref wayBillNumber, value))
				{
					Validate(WayBillNumberInfo);
				}
			}
		}

		ZString wayBillNumber;

		public ZPropertyInfo WayBillNumberInfo => GetZPropertyInfo(nameof(WayBillNumber));

		#endregion

		#region ContainerOperator

		public ZString ContainerOperator
		{
			get => containerOperator;
			set
			{
				if (SetNonPersistentPropertyValue(ContainerOperatorInfo, ref containerOperator, value))
				{
					Validate(ContainerOperatorInfo);
				}
			}
		}

		ZString containerOperator;

		public ZPropertyInfo ContainerOperatorInfo => GetZPropertyInfo(nameof(ContainerOperator));

		#endregion

		#region Terminal

		public ZString Terminal
		{
			get => terminal;
			set
			{
				if (SetNonPersistentPropertyValue(TerminalInfo, ref terminal, value))
				{
					Validate(TerminalInfo);
				}
			}
		}

		ZString terminal;

		public ZPropertyInfo TerminalInfo => GetZPropertyInfo(nameof(Terminal));

		#endregion

		#region SubTotal
		public ZDecimal SubTotal
		{
			get => subTotal;
			set
			{
				if (SetNonPersistentPropertyValue(SubTotalInfo, ref subTotal, value))
				{
					Validate(SubTotalInfo);
				}
			}
		}

		ZDecimal subTotal;

		public ZPropertyInfo SubTotalInfo => GetZPropertyInfo(nameof(SubTotal));
		#endregion

		#region VAT
		public ZDecimal VAT
		{
			get => vat;
			set
			{
				if (SetNonPersistentPropertyValue(VATInfo, ref vat, value))
				{
					Validate(VATInfo);
				}
			}
		}

		ZDecimal vat;

		public ZPropertyInfo VATInfo => GetZPropertyInfo(nameof(VAT));
		#endregion

		#region TotalR
		public ZDecimal TotalR
		{
			get => totalR;
			set
			{
				if (SetNonPersistentPropertyValue(TotalRInfo, ref totalR, value))
				{
					Validate(TotalRInfo);
				}
			}
		}

		ZDecimal totalR;

		public ZPropertyInfo TotalRInfo => GetZPropertyInfo(nameof(TotalR));
		#endregion

		#region Shipments

		public IReadOnlyCollection<ShipmentPackingInfo> ShipmentPackingInfos
		{
			get => shipmentPackingInfos;
			set => shipmentPackingInfos = SetChildCollection(shipmentPackingInfos, value);
		}

		IReadOnlyCollection<ShipmentPackingInfo> shipmentPackingInfos;

		#endregion

		#region DuesCollectionElements

		public DuesCollectionRow DuesCollectionElement1
		{
			get => duesCollectionElement1;
			set => duesCollectionElement1 = SetChild(duesCollectionElement1, value);
		}

		DuesCollectionRow duesCollectionElement1;

		public DuesCollectionRow DuesCollectionElement2
		{
			get => duesCollectionElement2;
			set => duesCollectionElement2 = SetChild(duesCollectionElement2, value);
		}

		DuesCollectionRow duesCollectionElement2;

		public DuesCollectionRow DuesCollectionElement3
		{
			get => duesCollectionElement3;
			set => duesCollectionElement3 = SetChild(duesCollectionElement3, value);
		}

		DuesCollectionRow duesCollectionElement3;

		public DuesCollectionRow DuesCollectionElement4
		{
			get => duesCollectionElement4;
			set => duesCollectionElement4 = SetChild(duesCollectionElement4, value);
		}

		DuesCollectionRow duesCollectionElement4;

		public DuesCollectionRow DuesCollectionElement5
		{
			get => duesCollectionElement5;
			set => duesCollectionElement5 = SetChild(duesCollectionElement5, value);
		}

		DuesCollectionRow duesCollectionElement5;

		public DuesCollectionRow DuesCollectionElement6
		{
			get => duesCollectionElement6;
			set => duesCollectionElement6 = SetChild(duesCollectionElement6, value);
		}

		DuesCollectionRow duesCollectionElement6;

		public DuesCollectionRow DuesCollectionElement7
		{
			get => duesCollectionElement7;
			set => duesCollectionElement7 = SetChild(duesCollectionElement7, value);
		}

		DuesCollectionRow duesCollectionElement7;

		public DuesCollectionRow DuesCollectionElement8
		{
			get => duesCollectionElement8;
			set => duesCollectionElement8 = SetChild(duesCollectionElement8, value);
		}

		DuesCollectionRow duesCollectionElement8;

		#endregion

		#region TotalNumberOfPacks

		public ZInt TotalNumberOfPacks
		{
			get => totalNumberOfPacks;
			set
			{
				if (SetNonPersistentPropertyValue(TotalNumberOfPacksInfo, ref totalNumberOfPacks, value))
				{
					Validate(TotalNumberOfPacksInfo);
				}
			}
		}

		ZInt totalNumberOfPacks;

		public ZPropertyInfo TotalNumberOfPacksInfo => GetZPropertyInfo(nameof(TotalNumberOfPacks));

		#endregion

		#region CargoDuesWarningPlaceHolder

		public ZString CargoDuesWarningPlaceHolder
		{
			get => cargoDuesWarningPlaceHolder;
			set
			{
				if (SetNonPersistentPropertyValue(CargoDuesWarningPlaceHolderInfo, ref cargoDuesWarningPlaceHolder, value))
				{
					Validate(CargoDuesWarningPlaceHolderInfo);
				}
			}
		}

		ZString cargoDuesWarningPlaceHolder;

		public ZPropertyInfo CargoDuesWarningPlaceHolderInfo => GetZPropertyInfo(nameof(CargoDuesWarningPlaceHolder));

		#endregion

		#region CargoDuesSectionTitle

		public ZString CargoDuesSectionTitle
		{
			get => cargoDuesSectionTitle;
			set
			{
				if (SetNonPersistentPropertyValue(CargoDuesSectionTitleInfo, ref cargoDuesSectionTitle, value))
				{
					Validate(CargoDuesSectionTitleInfo);
				}
			}
		}

		ZString cargoDuesSectionTitle;

		public ZPropertyInfo CargoDuesSectionTitleInfo => GetZPropertyInfo(nameof(CargoDuesSectionTitle));

		#endregion

		#region Transports

		public Transports Transports
		{
			get => transports;
			set => transports = SetChild(transports, value);
		}
		Transports transports;

		#endregion

		#region Containers

		public IReadOnlyCollection<Container> Containers
		{
			get => containers;
			set => containers = SetChildCollection(containers, value);
		}

		IReadOnlyCollection<Container> containers;

		#endregion
	}

	public class GoodsInfo : DocDataObject
	{
		public GoodsInfo(object identifier) : base(identifier)
		{
		}

		#region MarksAndNos

		public ZString MarksAndNos
		{
			get => marksAndNos;
			set
			{
				if (SetNonPersistentPropertyValue(MarksAndNosInfo, ref marksAndNos, value))
				{
					Validate(MarksAndNosInfo);
				}
			}
		}

		ZString marksAndNos;

		public ZPropertyInfo MarksAndNosInfo => GetZPropertyInfo(nameof(MarksAndNos));

		#endregion

		#region NumberOfPacks

		public ZInt NumberOfPacks
		{
			get => numberOfPacks;
			set
			{
				if (SetNonPersistentPropertyValue(NumberOfPacksInfo, ref numberOfPacks, value))
				{
					Validate(NumberOfPacksInfo);
				}
			}
		}

		ZInt numberOfPacks;

		public ZPropertyInfo NumberOfPacksInfo => GetZPropertyInfo(nameof(NumberOfPacks));

		#endregion

		#region PackType

		public ZString PackType
		{
			get => packType;
			set
			{
				if (SetNonPersistentPropertyValue(PackTypeInfo, ref packType, value))
				{
					Validate(PackTypeInfo);
				}
			}
		}

		ZString packType;

		public ZPropertyInfo PackTypeInfo => GetZPropertyInfo(nameof(PackType));

		#endregion

		#region GoodsDescription

		public ZString GoodsDescription
		{
			get => goodsDescription;
			set
			{
				if (SetNonPersistentPropertyValue(GoodsDescriptionInfo, ref goodsDescription, value))
				{
					Validate(GoodsDescriptionInfo);
				}
			}
		}

		ZString goodsDescription;

		public ZPropertyInfo GoodsDescriptionInfo => GetZPropertyInfo(nameof(GoodsDescription));

		#endregion

		public Measurement GrossMass
		{
			get => grossMass;
			set => grossMass = SetChild(grossMass, value);
		}
		Measurement grossMass;
	}

	public class DuesCollectionRow : DocDataObject
	{
		#region Description

		public ZString Description
		{
			get => description;
			set
			{
				if (SetNonPersistentPropertyValue(DescriptionInfo, ref description, value))
				{
					Validate(DescriptionInfo);
				}
			}
		}

		ZString description;

		public ZPropertyInfo DescriptionInfo => GetZPropertyInfo(nameof(Description));

		#endregion

		#region Factor

		public ZString Factor
		{
			get => factor;
			set
			{
				if (SetNonPersistentPropertyValue(FactorInfo, ref factor, value))
				{
					Validate(FactorInfo);
				}
			}
		}

		ZString factor;

		public ZPropertyInfo FactorInfo => GetZPropertyInfo(nameof(Factor));

		#endregion

		#region Rate

		public ZString Rate
		{
			get => rate;
			set
			{
				if (SetNonPersistentPropertyValue(RateInfo, ref rate, value))
				{
					Validate(RateInfo);
				}
			}
		}

		ZString rate;

		public ZPropertyInfo RateInfo => GetZPropertyInfo(nameof(Rate));

		#endregion

		#region Amount

		public ZString Amount
		{
			get => amount;
			set
			{
				if (SetNonPersistentPropertyValue(AmountInfo, ref amount, value))
				{
					Validate(AmountInfo);
				}
			}
		}

		ZString amount;

		public ZPropertyInfo AmountInfo => GetZPropertyInfo(nameof(Amount));

		#endregion
	}
}
