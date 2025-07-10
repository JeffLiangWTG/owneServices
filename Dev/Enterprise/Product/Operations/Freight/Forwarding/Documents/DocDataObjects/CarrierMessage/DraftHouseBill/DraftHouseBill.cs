using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DataObjects;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class DraftHouseBill : DocDataObject, IDataSourceProvider, IHouseBillOverrideProvider, ICustomFieldProviderProxy
	{
		public DraftHouseBill(ZString sourceType, ZString sourceID, CustomBusinessObject customBusinessObject = null)
		{
			this.sourceType = sourceType;
			this.sourceID = sourceID;
			this.customBusinessObject = customBusinessObject;
		}

		#region IDataSourceProvider members

		ZString IDataSourceProvider.SourceID => sourceID;
		readonly ZString sourceID;

		ZString IDataSourceProvider.SourceType => sourceType;
		readonly ZString sourceType;

		#endregion

		#region IHouseBillOverrideProvider members

		string IHouseBillOverrideProvider.GoodsDetailsTextOverride => HasGoodsDetailsTextOverride
			? (string)GoodsDetailsTextOverride
			: null;

		string IHouseBillOverrideProvider.ChargesTextOverride => HasChargesTextOverride
			? (string)ChargesTextOverride
			: null;

		string IHouseBillOverrideProvider.FollowOnTextOverride => HasFollowOnTextOverride
			? (string)FollowOnTextOverride
			: null;

		#region GoodsDetailsTextOverride

		public ZString GoodsDetailsTextOverride
		{
			get => goodsDetailsTextOverride;
			set
			{
				if (SetNonPersistentPropertyValue(GoodsDetailsTextOverrideInfo, ref goodsDetailsTextOverride, value))
				{
				}
			}
		}

		ZString goodsDetailsTextOverride;

		public ZPropertyInfo GoodsDetailsTextOverrideInfo => GetZPropertyInfo(nameof(GoodsDetailsTextOverride));

		#endregion

		#region HasGoodsDetailsTextOverride

		public ZBool HasGoodsDetailsTextOverride
		{
			get => hasGoodsDetailsTextOverride;
			set
			{
				if (SetNonPersistentPropertyValue(HasGoodsDetailsTextOverrideInfo, ref hasGoodsDetailsTextOverride, value))
				{
				}
			}
		}

		ZBool hasGoodsDetailsTextOverride;

		public ZPropertyInfo HasGoodsDetailsTextOverrideInfo => GetZPropertyInfo(nameof(HasGoodsDetailsTextOverride));

		#endregion

		#region ChargesTextOverride

		public ZString ChargesTextOverride
		{
			get => chargesTextOverride;
			set
			{
				if (SetNonPersistentPropertyValue(ChargesTextOverrideInfo, ref chargesTextOverride, value))
				{
				}
			}
		}

		ZString chargesTextOverride;

		public ZPropertyInfo ChargesTextOverrideInfo => GetZPropertyInfo(nameof(ChargesTextOverride));

		#endregion

		#region HasChargesTextOverride

		public ZBool HasChargesTextOverride
		{
			get => hasChargesTextOverride;
			set
			{
				if (SetNonPersistentPropertyValue(HasChargesTextOverrideInfo, ref hasChargesTextOverride, value))
				{
				}
			}
		}

		ZBool hasChargesTextOverride;

		public ZPropertyInfo HasChargesTextOverrideInfo => GetZPropertyInfo(nameof(HasChargesTextOverride));

		#endregion

		#region FollowOnTextOverride

		public ZString FollowOnTextOverride
		{
			get => followOnTextOverride;
			set
			{
				if (SetNonPersistentPropertyValue(FollowOnTextOverrideInfo, ref followOnTextOverride, value))
				{
				}
			}
		}

		ZString followOnTextOverride;

		public ZPropertyInfo FollowOnTextOverrideInfo => GetZPropertyInfo(nameof(FollowOnTextOverride));

		#endregion

		#region HasFollowOnTextOverride

		public ZBool HasFollowOnTextOverride
		{
			get => hasFollowOnTextOverride;
			set
			{
				if (SetNonPersistentPropertyValue(HasFollowOnTextOverrideInfo, ref hasFollowOnTextOverride, value))
				{
				}
			}
		}

		ZBool hasFollowOnTextOverride;

		public ZPropertyInfo HasFollowOnTextOverrideInfo => GetZPropertyInfo(nameof(HasFollowOnTextOverride));

		#endregion

		#endregion

		#region ICustomFieldProviderProxy members

		CustomBusinessObject ICustomFieldProviderProxy.CustomBusinessObject => customBusinessObject;
		readonly CustomBusinessObject customBusinessObject;

		#endregion

		#region IsDraft

		public ZBool IsDraft
		{
			get => isDraft;
			set
			{
				if (SetNonPersistentPropertyValue(IsDraftInfo, ref isDraft, value))
				{
				}
			}
		}

		ZBool isDraft;

		public ZPropertyInfo IsDraftInfo => GetZPropertyInfo(nameof(IsDraft));

		#endregion

		#region CarrierAgent

		public ZString CarrierAgent
		{
			get => carrierAgent;
			set
			{
				if (SetNonPersistentPropertyValue(CarrierAgentInfo, ref carrierAgent, value))
				{
				}
			}
		}
		ZString carrierAgent;

		public ZPropertyInfo CarrierAgentInfo => GetZPropertyInfo(nameof(CarrierAgent));

		#endregion

		#region CarrierBookingReference

		public ZString CarrierBookingReference
		{
			get => carrierBookingReference;
			set
			{
				if (SetNonPersistentPropertyValue(CarrierBookingReferenceInfo, ref carrierBookingReference, value))
				{
				}
			}
		}

		ZString carrierBookingReference;

		public ZPropertyInfo CarrierBookingReferenceInfo => GetZPropertyInfo(nameof(CarrierBookingReference));

		#endregion

		#region HouseBillNumber

		public ZString HouseBillNumber
		{
			get => houseBillNumber;
			set
			{
				if (SetNonPersistentPropertyValue(HouseBillNumberInfo, ref houseBillNumber, value))
				{
				}
			}
		}

		ZString houseBillNumber;

		public ZPropertyInfo HouseBillNumberInfo => GetZPropertyInfo(nameof(HouseBillNumber));

		#endregion

		#region ExportReference

		public ZString ExportReference
		{
			get => exportReference;
			set
			{
				if (SetNonPersistentPropertyValue(ExportReferenceInfo, ref exportReference, value))
				{
				}
			}
		}

		ZString exportReference;

		public ZPropertyInfo ExportReferenceInfo => GetZPropertyInfo(nameof(ExportReference));

		#endregion

		#region ShippersReference

		public ZString ShippersReference
		{
			get => shippersReference;
			set
			{
				if (SetNonPersistentPropertyValue(ShippersReferenceInfo, ref shippersReference, value))
				{
				}
			}
		}

		ZString shippersReference;

		public ZPropertyInfo ShippersReferenceInfo => GetZPropertyInfo(nameof(ShippersReference));

		#endregion

		#region FreightForwarderReference

		public ZString FreightForwarderReference
		{
			get => freightForwarderReference;
			set
			{
				if (SetNonPersistentPropertyValue(FreightForwarderReferenceInfo, ref freightForwarderReference, value))
				{
				}
			}
		}

		ZString freightForwarderReference;

		public ZPropertyInfo FreightForwarderReferenceInfo => GetZPropertyInfo(nameof(FreightForwarderReference));

		#endregion

		#region ContainerMode

		public ICodeDescription ContainerMode
		{
			get => containerMode;
			set => containerMode = SetChild(containerMode, value);
		}
		ICodeDescription containerMode;

		#endregion

		#region DeliveryMode

		public ZString DeliveryMode
		{
			get => deliveryMode;
			set
			{
				if (SetNonPersistentPropertyValue(DeliveryModeInfo, ref deliveryMode, value))
				{
				}
			}
		}

		ZString deliveryMode;

		public ZPropertyInfo DeliveryModeInfo => GetZPropertyInfo(nameof(DeliveryMode));

		#endregion

		#region CarrierContractNumber

		public ZString CarrierContractNumber
		{
			get => carrierContractNumber;
			set
			{
				if (SetNonPersistentPropertyValue(CarrierContractNumberInfo, ref carrierContractNumber, value))
				{
				}
			}
		}

		ZString carrierContractNumber;

		public ZPropertyInfo CarrierContractNumberInfo => GetZPropertyInfo(nameof(CarrierContractNumber));

		#endregion

		#region PreCarriageBy

		public ZString PreCarriageBy
		{
			get => preCarriageBy;
			set
			{
				if (SetNonPersistentPropertyValue(PreCarriageByInfo, ref preCarriageBy, value))
				{
				}
			}
		}

		ZString preCarriageBy;

		public ZPropertyInfo PreCarriageByInfo => GetZPropertyInfo(nameof(PreCarriageBy));

		#endregion

		#region VesselName

		public ZString VesselName
		{
			get => vesselName;
			set
			{
				if (SetNonPersistentPropertyValue(VesselNameInfo, ref vesselName, value))
				{
				}
			}
		}

		ZString vesselName;

		public ZPropertyInfo VesselNameInfo => GetZPropertyInfo(nameof(VesselName));

		#endregion

		#region Voyage

		public ZString Voyage
		{
			get => voyage;
			set
			{
				if (SetNonPersistentPropertyValue(VoyageInfo, ref voyage, value))
				{
				}
			}
		}

		ZString voyage;

		public ZPropertyInfo VoyageInfo => GetZPropertyInfo(nameof(Voyage));

		#endregion

		#region LloydsIMO

		public ZString LloydsIMO
		{
			get => lloydsIMO;
			set
			{
				if (SetNonPersistentPropertyValue(LloydsIMOInfo, ref lloydsIMO, value))
				{
				}
			}
		}

		ZString lloydsIMO;

		public ZPropertyInfo LloydsIMOInfo => GetZPropertyInfo(nameof(LloydsIMO));

		#endregion

		#region Shipper

		public IAddress Shipper
		{
			get => shipper;
			set => shipper = SetChild(shipper, value);
		}
		IAddress shipper;

		#endregion

		#region Consignee

		public IAddress Consignee
		{
			get => consignee;
			set => consignee = SetChild(consignee, value);
		}
		IAddress consignee;

		#endregion

		#region NotifyParty

		public IAddress NotifyParty
		{
			get => notifyParty;
			set => notifyParty = SetChild(notifyParty, value);
		}
		IAddress notifyParty;

		#endregion

		#region PlaceOfReceipt

		public IUnloco PlaceOfReceipt
		{
			get => placeOfReceipt;
			set => placeOfReceipt = SetChild(placeOfReceipt, value);
		}
		IUnloco placeOfReceipt;

		#endregion

		#region PlaceOfDelivery

		public IUnloco PlaceOfDelivery
		{
			get => placeOfDelivery;
			set => placeOfDelivery = SetChild(placeOfDelivery, value);
		}
		IUnloco placeOfDelivery;

		#endregion

		#region PortOfLoading

		public IUnloco PortOfLoading
		{
			get => portOfLoading;
			set => portOfLoading = SetChild(portOfLoading, value);
		}
		IUnloco portOfLoading;

		#endregion

		#region PortOfDischarge

		public IUnloco PortOfDischarge
		{
			get => portOfDischarge;
			set => portOfDischarge = SetChild(portOfDischarge, value);
		}
		IUnloco portOfDischarge;

		#endregion

		#region Containers

		public IReadOnlyCollection<DraftHouseBillContainer> Containers
		{
			get => containers;
			set => containers = SetChildCollection(containers, value);
		}
		IReadOnlyCollection<DraftHouseBillContainer> containers;

		#endregion

		#region FreightCharges

		public ZString FreightCharges
		{
			get => freightCharges;
			set
			{
				if (SetNonPersistentPropertyValue(FreightChargesInfo, ref freightCharges, value))
				{
				}
			}
		}

		ZString freightCharges;

		public ZPropertyInfo FreightChargesInfo => GetZPropertyInfo(nameof(FreightCharges));

		#endregion

		#region FreightPayableAt

		public IUnloco FreightPayableAt
		{
			get => freightPayableAt;
			set => freightPayableAt = SetChild(freightPayableAt, value);
		}
		IUnloco freightPayableAt;

		#endregion

		#region DeclaredValueOfGoods

		public IMoney DeclaredValueOfGoods
		{
			get => declaredValueOfGoods;
			set => declaredValueOfGoods = SetChild(declaredValueOfGoods, value);
		}
		IMoney declaredValueOfGoods;

		#endregion

		#region DateOfIssue

		public ZDateTime DateOfIssue
		{
			get => dateOfIssue;
			set
			{
				if (SetNonPersistentPropertyValue(DateOfIssueInfo, ref dateOfIssue, value))
				{
				}
			}
		}

		ZDateTime dateOfIssue;

		public ZPropertyInfo DateOfIssueInfo => GetZPropertyInfo(nameof(DateOfIssue));

		#endregion

		#region PlaceOfIssue

		public IUnloco PlaceOfIssue
		{
			get => placeOfIssue;
			set => placeOfIssue = SetChild(placeOfIssue, value);
		}
		IUnloco placeOfIssue;

		#endregion

		#region ShippedOnBoard

		public IShippedOnBoard ShippedOnBoard
		{
			get => shippedOnBoard;
			set => shippedOnBoard = SetChild(shippedOnBoard, value);
		}
		IShippedOnBoard shippedOnBoard;

		#endregion

		#region Clause

		public ZString Clause
		{
			get => clause;
			set
			{
				if (SetNonPersistentPropertyValue(ClauseInfo, ref clause, value))
				{
				}
			}
		}

		ZString clause;

		public ZPropertyInfo ClauseInfo => GetZPropertyInfo(nameof(Clause));

		#endregion
	}
}
