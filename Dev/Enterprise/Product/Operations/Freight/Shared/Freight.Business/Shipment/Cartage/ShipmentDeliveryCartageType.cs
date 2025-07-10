using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Business
{
	public class ShipmentDeliveryCartageType : ShipmentCartageType
	{
		public ShipmentDeliveryCartageType(CommonShipment shipment) : base(shipment) { }

		public override MultilingualString Description
		{
			get { return ResString.GetMultilingualString("c7686712-0847-4369-9f69-fd3d4e347be4", "Delivery"); }
		}

		public override OrgAddress LocalTransportProviderAddress
		{
			get { return ShipmentParent.DocsAndCartage.DeliveryCartageCoAddr; }
		}

		public override ZPropertyInfo CartageAddressInfo
		{
			get { return ShipmentParent.DocsAndCartage.JP_OA_DeliveryCartageCoAddrInfo; }
		}

		public override ZString CartageJobType
		{
			get
			{
				if (ShipmentParent.IsDomestic())
				{
					if (ShipmentParent.Consols.Count == 0 || ShipmentParent.IsLoose || ShipmentParent.IsRoad || ShipmentParent.IsAir || ShipmentParent.IsCourier)
					{
						return Constants.CartageJobType.NEW_DomesticLooseDelivery;
					}
					else
					{
						return Constants.CartageJobType.NEW_DomesticContainerizedDelivery;
					}
				}
				else
				{
					if (ShipmentParent.IsAir)
					{
						return Constants.CartageJobType.NEW_AirImport;
					}
					else if (ShipmentParent.Consols.Count == 0 || ShipmentParent.IsLoose || ShipmentParent.IsRoad || ShipmentParent.IsCourier)
					{
						return Constants.CartageJobType.NEW_LCLImport;
					}
					else if (ShipmentParent.IsContainerised)
					{
						if (GetCartageAddress(LocalCartageJobOrgTypeList.Codes.CFS).IsEmpty)
						{
							if (GetCartageAddress(LocalCartageJobOrgTypeList.Codes.CYD).IsEmpty)
							{
								return Constants.CartageJobType.NEW_FCLCTOtoCNE;
							}
							else
							{
								return Constants.CartageJobType.NEW_FCLImportToCNE;
							}
						}
						else
						{
							if (GetCartageAddress(LocalCartageJobOrgTypeList.Codes.CYD).IsEmpty)
							{
								return Constants.CartageJobType.NEW_FCLCTOtoCFS;
							}
							else
							{
								return Constants.CartageJobType.NEW_FCLImportUnpack;
							}
						}
					}
				}
				return "";
			}
		}

		public override IReadOnlyCollection<ICartageContainer> CartageContainers
		{
			get
			{
				return (ICartageContainer[])ShipmentParent.ArrivalContainers.ToArray(typeof(ICartageContainer));
			}
		}

		public override IReadOnlyCollection<ICartageLooseCargo> CartageLooseCargo
		{
			get { return (ICartageLooseCargo[])ShipmentParent.OuterPackLines.ToArray(typeof(ICartageLooseCargo)); }
		}

		public override JobDocAddress GetCartageAddress(ZString orgType)
		{
			switch (orgType)
			{
				case LocalCartageJobOrgTypeList.Codes.CFS:
					return ShipmentParent.GetArrivalCFSDocAddress;
				case LocalCartageJobOrgTypeList.Codes.CNE:
					return ShipmentParent.GetConsigneeDeliveryDocAddress;
				case LocalCartageJobOrgTypeList.Codes.CNR:
					return ShipmentParent.GetConsignorPickupDocAddress;
				case LocalCartageJobOrgTypeList.Codes.CTO:
					return ShipmentParent.GetArrivalCTODocAddress;
				case LocalCartageJobOrgTypeList.Codes.CYD:
					return ShipmentParent.GetArrivalContainerYardDocAddress;
				default:
					return null;
			}
		}

		protected override ZPropertyInfo[] GetCartageAddressInfosToMonitor()
		{
			return new ZPropertyInfo[] { ShipmentParent.JS_OA_ImportReleaseDepotInfo, };
		}

		public override void CartageAdvised(BusinessObjectFactory factory)
		{
			CommonShipment shipmentInFactory = (CommonShipment)GetCartageParentInFactory(factory);
			if (shipmentInFactory != null)
			{
				shipmentInFactory.DocsAndCartage.JP_DeliveryCartageAdvisedInfo.Value = ZDateTime.Now;
			}
		}

		public override void PickupCompleted(AutoJobContainer container, DocAddressType addressType, ZDateTime timeOut)
		{
		}

		public override void DeliveryCompleted(AutoJobContainer container, DocAddressType addressType, ZDateTime timeOut)
		{
			CommonContainer commonContainer = (CommonContainer)container;
			if (addressType == DocAddressType.LocalCartageImporter)
			{
				commonContainer.DestinationConfirm.EU_PickupDeliveryTime = timeOut;
			}
		}

		public override void PickupCompleted(DocAddressType addressType, ZDateTime timeOut)
		{
		}

		public override void DeliveryCompleted(DocAddressType addressType, ZDateTime timeOut)
		{
			if (addressType == DocAddressType.LocalCartageImporter)
			{
				AddOrSetCompletedDeliveryConfirm(ShipmentParent, ShipmentParent.DeliveryConfirms, timeOut);
			}
		}

		public override void SetTotalDemurrage(TimeSpan demurrage)
		{
			ShipmentParent.DocsAndCartage.JP_DeliveryTruckWaitTime = ZDateTime.MinSmallDateTimeValue + demurrage;
		}

		public override ZDateTime EstimatedCartagePickup
		{
			get { return ShipmentParent.DocsAndCartage.JP_EstimatedDelivery; }
		}

		public override ZDateTime EstimatedCartageDelivery
		{
			get { return ShipmentParent.DocsAndCartage.JP_DeliveryRequiredBy; }
		}

		public override ZString DropMode
		{
			get { return ShipmentParent.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded; }
		}

		public override IEnumerable<ZString> GetMatchingDirectionCodes()
		{
			return new ZString[] { CartageDirection.Import, CartageDirection.Destination };
		}
	}
}
