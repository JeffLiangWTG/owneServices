using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR
{
	sealed class GoodsDetail : DocDataObject
	{
		public GoodsDetail(object identifier, string pcs = "")
			: base(identifier)
		{
			this.pcs = pcs;
			this.identifier = identifier.ToString();
		}

		readonly string pcs;

		readonly string identifier;
		public ZString GroupKeyByConsignment => identifier;

		#region CommodityLineRef

		public ZString CommodityLineRef
		{
			get => commodityLineRef;
			set
			{
				if (SetNonPersistentPropertyValue(CommodityLineRefInfo, ref commodityLineRef, value))
				{
					Validate(CommodityLineRefInfo);
				}
			}
		}
		ZString commodityLineRef;

		public ZPropertyInfo CommodityLineRefInfo => GetZPropertyInfo(nameof(CommodityLineRef));

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

		#region ConsignmentNumber

		public ZString ConsignmentNumber
		{
			get => consignmentNumber;
			set
			{
				if (SetNonPersistentPropertyValue(ConsignmentNumberInfo, ref consignmentNumber, value))
				{
				}
			}
		}

		ZString consignmentNumber;

		public ZPropertyInfo ConsignmentNumberInfo => GetZPropertyInfo(nameof(ConsignmentNumber));

		#endregion

		#region ShipmentNumber

		public ZString ShipmentNumber
		{
			get => shipmentNumber;
			set
			{
				if (SetNonPersistentPropertyValue(ShipmentNumberInfo, ref shipmentNumber, value))
				{
				}
			}
		}

		ZString shipmentNumber;

		public ZPropertyInfo ShipmentNumberInfo => GetZPropertyInfo(nameof(ShipmentNumber));

		#endregion

		#region ShipmentWeight

		public IMeasurement ShipmentWeight
		{
			get => shipmentWeight;
			set => shipmentWeight = SetChild(shipmentWeight, value);
		}

		IMeasurement shipmentWeight;

		#endregion

		#region PortOfOrigin

		public Unloco PortOfOrigin
		{
			get => portOfOrigin;
			set => portOfOrigin = SetChild(portOfOrigin, value);
		}

		Unloco portOfOrigin;

		#endregion

		#region HazardousCargo

		public ZBool HazardousCargo
		{
			get => hazardousCargo;
			set
			{
				if (SetNonPersistentPropertyValue(HazardousCargoInfo, ref hazardousCargo, value))
				{
					Validate(HazardousCargoInfo);
				}
			}
		}

		ZBool hazardousCargo;

		public ZPropertyInfo HazardousCargoInfo => GetZPropertyInfo(nameof(HazardousCargo));

		#endregion

		#region PackageType

		public ICodeDescription PackageType
		{
			get => packageType;
			set => packageType = SetChild(packageType, value);
		}
		ICodeDescription packageType;

		#endregion

		#region RequiresTemperatureControl

		public ZBool RequiresTemperatureControl
		{
			get => requiresTemperatureControl;
			set
			{
				if (SetNonPersistentPropertyValue(RequiresTemperatureControlInfo, ref requiresTemperatureControl, value))
				{
					Validate(RequiresTemperatureControlInfo);
				}
			}
		}

		ZBool requiresTemperatureControl;

		public ZPropertyInfo RequiresTemperatureControlInfo => GetZPropertyInfo(nameof(RequiresTemperatureControl));

		#endregion

		#region MarksAndNumbers

		public ZString MarksAndNumbers
		{
			get => marksAndNumbers;
			set
			{
				if (SetNonPersistentPropertyValue(MarksAndNumbersInfo, ref marksAndNumbers, value))
				{
					Validate(MarksAndNumbersInfo);
				}
			}
		}

		ZString marksAndNumbers;

		public ZPropertyInfo MarksAndNumbersInfo => GetZPropertyInfo(nameof(MarksAndNumbers));

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

		#region Shipper

		public Address Shipper
		{
			get => shipper;
			set => shipper = SetChild(shipper, value);
		}

		Address shipper;

		#endregion

		#region Consignee

		public Address Consignee
		{
			get => consignee;
			set => consignee = SetChild(consignee, value);
		}

		Address consignee;

		#endregion

		#region NotifyParty

		public Address NotifyParty
		{
			get => notifyParty;
			set => notifyParty = SetChild(notifyParty, value);
		}

		Address notifyParty;

		#endregion

		#region NotifyParty2

		public Address NotifyParty2
		{
			get => notifyParty2;
			set => notifyParty2 = SetChild(notifyParty2, value);
		}

		Address notifyParty2;

		#endregion

		#region TaxReference

		public ZString TaxReference
		{
			get => taxReference;
			set
			{
				if (SetNonPersistentPropertyValue(TaxReferenceInfo, ref taxReference, value))
				{
					Validate(TaxReferenceInfo);
				}
			}
		}
		ZString taxReference;

		public ZPropertyInfo TaxReferenceInfo => GetZPropertyInfo(nameof(TaxReference));

		#endregion

		#region TaxAmount

		public Money TaxAmount
		{
			get => taxAmount;
			set => taxAmount = SetChild(taxAmount, value);
		}

		Money taxAmount;

		#endregion

		#region TaxStatus

		public ZString TaxStatus
		{
			get => taxStatus;
			set
			{
				if (SetNonPersistentPropertyValue(TaxStatusInfo, ref taxStatus, value))
				{
					Validate(TaxStatusInfo);
				}
			}
		}
		ZString taxStatus;

		public ZPropertyInfo TaxStatusInfo => GetZPropertyInfo(nameof(TaxStatus));

		#endregion

		#region GoodsHandlingNotes

		public ZString GoodsHandlingNotes
		{
			get => goodsHandlingNotes;
			set
			{
				if (SetNonPersistentPropertyValue(GoodsHandlingNotesInfo, ref goodsHandlingNotes, value))
				{
					Validate(GoodsHandlingNotesInfo);
				}
			}
		}
		ZString goodsHandlingNotes;

		public ZPropertyInfo GoodsHandlingNotesInfo => GetZPropertyInfo(nameof(GoodsHandlingNotes));

		#endregion

		#region PackingLines

		public IReadOnlyCollection<BookingPackingLine> PackingLines
		{
			get => packingLines;
			set => packingLines = SetChildCollection(packingLines, value);
		}

		IReadOnlyCollection<BookingPackingLine> packingLines;

		#endregion

		#region IsTaxStatusPrepaid

		public ZBool IsTaxStatusPrepaid
		{
			get => TaxStatus == DocDataConstants.TaxStatus.Prepaid;
			set
			{
				if (value)
				{
					TaxStatus = DocDataConstants.TaxStatus.Prepaid;
				}
				else if (!value && IsTaxStatusPrepaid)
				{
					TaxStatus = ZString.Empty;
				}

				TaxStatusInfo.RefreshBinding();
				IsTaxStatusCollectInfo.RefreshBinding();
				IsTaxStatusPrepaidInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsTaxStatusPrepaidInfo => GetZPropertyInfo(nameof(IsTaxStatusPrepaid));

		#endregion

		#region IsTaxStatusCollect

		public ZBool IsTaxStatusCollect
		{
			get => TaxStatus == DocDataConstants.TaxStatus.Collect;
			set
			{
				if (value)
				{
					TaxStatus = DocDataConstants.TaxStatus.Collect;
				}
				else if (!value && IsTaxStatusCollect)
				{
					TaxStatus = ZString.Empty;
				}

				TaxStatusInfo.RefreshBinding();
				IsTaxStatusCollectInfo.RefreshBinding();
				IsTaxStatusPrepaidInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsTaxStatusCollectInfo => GetZPropertyInfo(nameof(IsTaxStatusCollect));

		#endregion

		#region CommodityReference

		public ZString CommodityReference
		{
			get => commodityReference;
			set
			{
				if (SetNonPersistentPropertyValue(CommodityReferenceInfo, ref commodityReference, value))
				{
					Validate(CommodityReferenceInfo);
				}
			}
		}
		ZString commodityReference;

		public ZPropertyInfo CommodityReferenceInfo => GetZPropertyInfo(nameof(CommodityReference));

		#endregion

		#region CustomsStatus

		public ZString CustomsStatus
		{
			get => customsStatus;
			set
			{
				if (SetNonPersistentPropertyValue(CustomsStatusInfo, ref customsStatus, value))
				{
					Validate(CustomsStatusInfo);
				}
			}
		}
		ZString customsStatus;

		public ZPropertyInfo CustomsStatusInfo => GetZPropertyInfo(nameof(CustomsStatus));

		#endregion

		#region ICVReference

		public ZString ICVReference
		{
			get => icvReference;
			set
			{
				if (SetNonPersistentPropertyValue(ICVReferenceInfo, ref icvReference, value))
				{
					Validate(ICVReferenceInfo);
				}
			}
		}
		ZString icvReference;

		public ZPropertyInfo ICVReferenceInfo => GetZPropertyInfo(nameof(ICVReference));

		#endregion

		#region DeclaredPackCount

		public ZInt DeclaredPackCount
		{
			get => declaredPackCount;
			set
			{
				if (SetNonPersistentPropertyValue(DeclaredPackCountInfo, ref declaredPackCount, value))
				{
					Validate(DeclaredPackCountInfo);
				}
			}
		}

		ZInt declaredPackCount;

		public ZPropertyInfo DeclaredPackCountInfo => GetZPropertyInfo(nameof(DeclaredPackCount));

		#endregion

		#region DeclaredPackWeight

		public IMeasurement DeclaredPackWeight
		{
			get => declaredPackWeight;
			set => declaredPackWeight = SetChild(declaredPackWeight, value);
		}

		IMeasurement declaredPackWeight;

		#endregion

		#region DeclarationReferenceNumber

		public const string DeclarationReferenceNumberUXmlName = "DeclarationAPPlusID";

		public ZString DeclarationReferenceNumber
		{
			get => declarationReferenceNumber;
			set
			{
				if (SetNonPersistentPropertyValue(DeclarationReferenceNumberInfo, ref declarationReferenceNumber, value))
				{
					Validate(DeclarationReferenceNumberInfo);
				}
			}
		}

		ZString declarationReferenceNumber;

		public ZPropertyInfo DeclarationReferenceNumberInfo => GetZPropertyInfo(nameof(DeclarationReferenceNumber));

		#endregion

		#region AppliesToAllPacks

		public bool ShowAppliesToAllPacks { get; set; }

		public ZBool AppliesToAllPacks
		{
			get => appliesToAllPacks;
			set
			{
				if (SetNonPersistentPropertyValue(AppliesToAllPacksInfo, ref appliesToAllPacks, value))
				{
					Validate(AppliesToAllPacksInfo);
				}
			}
		}

		ZBool appliesToAllPacks;

		public ZPropertyInfo AppliesToAllPacksInfo => GetZPropertyInfo(nameof(AppliesToAllPacks));

		#endregion

		#region ThirdParty

		public Address ThirdParty
		{
			get => thirdParty;
			set => thirdParty = SetChild(thirdParty, value);
		}

		Address thirdParty;

		#endregion

		#region ThirdPartySON

		public RegistrationNumber ThirdPartySON
		{
			get => thirdPartySON;
			set => thirdPartySON = SetChild(thirdPartySON, value);
		}

		RegistrationNumber thirdPartySON;

		#endregion

		#region ThirdPartyCI5

		public RegistrationNumber ThirdPartyCI5
		{
			get => thirdPartyCI5;
			set => thirdPartyCI5 = SetChild(thirdPartyCI5, value);
		}

		RegistrationNumber thirdPartyCI5;

		#endregion

		#region ThirdPartyProviderID

		public RegistrationNumber ThirdPartyProviderID
		{
			get
			{
				switch (pcs)
				{
					case FrenchPortsConstants.PCS.MGI:
						return ThirdPartyCI5;
					case FrenchPortsConstants.PCS.Soget:
						return ThirdPartySON;
					default:
						return emptyThirdPartyProviderID ?? (emptyThirdPartyProviderID = SetChild(emptyThirdPartyProviderID, new RegistrationNumber()));
				}
			}
		}

		RegistrationNumber emptyThirdPartyProviderID;

		#endregion

		public ZString ContainerNumber { get; set; }
	}
}
