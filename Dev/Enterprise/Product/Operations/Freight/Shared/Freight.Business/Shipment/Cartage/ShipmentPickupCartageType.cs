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
	public class ShipmentPickupCartageType : ShipmentCartageType
	{
		public ShipmentPickupCartageType(CommonShipment shipment) : base(shipment) { }

		public override MultilingualString Description
		{
			get { return ResString.GetMultilingualString("f5ba7c2c-6a34-418c-b083-b1b1d99eb56f", "Pickup"); }
		}

		public override OrgAddress LocalTransportProviderAddress
		{
			get { return ShipmentParent.DocsAndCartage.PickupCartageCoAddr; }
		}

		public override ZPropertyInfo CartageAddressInfo
		{
			get { return ShipmentParent.DocsAndCartage.JP_OA_PickupCartageCoAddrInfo; }
		}

		public override ZString CartageJobType
		{
			get
			{
				if (ShipmentParent.IsDomestic())
				{
					if (ShipmentParent.Consols.Count == 0 || ShipmentParent.IsLoose || ShipmentParent.IsRoad || ShipmentParent.IsAir || ShipmentParent.IsCourier)
					{
						return Constants.CartageJobType.NEW_DomesticLoosePickup;
					}
					else
					{
						return Constants.CartageJobType.NEW_DomesticContainerizedPickup;
					}
				}
				else
				{
					if (ShipmentParent.IsAir)
					{
						return Constants.CartageJobType.NEW_AirExport;
					}
					else if (ShipmentParent.Consols.Count == 0 || ShipmentParent.IsLoose || ShipmentParent.IsRoad || ShipmentParent.IsCourier)
					{
						return Constants.CartageJobType.NEW_LCLExport;
					}
					else if (ShipmentParent.IsContainerised)
					{
						if (GetCartageAddress(LocalCartageJobOrgTypeList.Codes.CFS).IsEmpty)
						{
							if (GetCartageAddress(LocalCartageJobOrgTypeList.Codes.CYD).IsEmpty)
							{
								return Constants.CartageJobType.NEW_FCLSHPtoCTO;
							}
							else
							{
								return Constants.CartageJobType.NEW_FCLExportToSHP;
							}
						}
						else
						{
							if (GetCartageAddress(LocalCartageJobOrgTypeList.Codes.CYD).IsEmpty)
							{
								return Constants.CartageJobType.NEW_FCLCFStoCTO;
							}
							else
							{
								return Constants.CartageJobType.NEW_FCLExportPack;
							}
						}
					}
				}

				return "";
			}
		}

		public override JobDocAddress GetCartageAddress(ZString orgType)
		{
			switch (orgType)
			{
				case LocalCartageJobOrgTypeList.Codes.CFS:
					return ShipmentParent.GetDepartureCFSDocAddress;
				case LocalCartageJobOrgTypeList.Codes.CNE:
					return ShipmentParent.GetConsigneeDeliveryDocAddress;
				case LocalCartageJobOrgTypeList.Codes.CNR:
					return ShipmentParent.GetConsignorPickupDocAddress;
				case LocalCartageJobOrgTypeList.Codes.CTO:
					return ShipmentParent.GetDepartureCTODocAddress;
				case LocalCartageJobOrgTypeList.Codes.CYD:
					return ShipmentParent.GetDepartureContainerYardDocAddress;
				default:
					return null;
			}
		}

		public override IReadOnlyCollection<ICartageContainer> CartageContainers
		{
			get { return (ICartageContainer[])ShipmentParent.DepartureContainers.ToArray(typeof(ICartageContainer)); }
		}

		public override IReadOnlyCollection<ICartageLooseCargo> CartageLooseCargo
		{
			get { return (ICartageLooseCargo[])ShipmentParent.OuterPackLines.ToArray(typeof(ICartageLooseCargo)); }
		}

		protected override ZPropertyInfo[] GetCartageAddressInfosToMonitor()
		{
			return new ZPropertyInfo[] { ShipmentParent.JS_OA_ExportReceivingDepotInfo, };
		}

		public override void CartageAdvised(BusinessObjectFactory factory)
		{
			CommonShipment shipmentInFactory = (CommonShipment)GetCartageParentInFactory(factory);
			if (shipmentInFactory != null)
			{
				shipmentInFactory.DocsAndCartage.JP_PickupCartageAdvisedInfo.Value = ZDateTime.Now;
			}
		}

		public override void PickupCompleted(AutoJobContainer container, DocAddressType addressType, ZDateTime timeOut)
		{
			CommonContainer commonContainer = (CommonContainer)container;
			if (addressType == DocAddressType.LocalCartageExporter)
			{
				commonContainer.OriginConfirm.EU_PickupDeliveryTime = timeOut;
			}
		}

		public override void DeliveryCompleted(AutoJobContainer container, DocAddressType addressType, ZDateTime timeOut)
		{
		}

		public override void PickupCompleted(DocAddressType addressType, ZDateTime timeOut)
		{
			if (addressType == DocAddressType.LocalCartageExporter)
			{
				AddOrSetCompletedPickupConfirm(ShipmentParent, ShipmentParent.PickupConfirms, timeOut);
			}
		}

		public override void DeliveryCompleted(DocAddressType addressType, ZDateTime timeOut)
		{
		}

		public override void SetTotalDemurrage(TimeSpan demurrage)
		{
			ShipmentParent.DocsAndCartage.JP_PickupTruckWaitTime = ZDateTime.MinSmallDateTimeValue + demurrage;
		}

		public override ZDateTime EstimatedCartagePickup
		{
			get { return ShipmentParent.DocsAndCartage.JP_EstimatedPickup; }
		}

		public override ZDateTime EstimatedCartageDelivery
		{
			get { return ShipmentParent.DocsAndCartage.JP_PickupRequiredBy; }
		}

		public override ZString DropMode
		{
			get { return ShipmentParent.DocsAndCartage.JP_FCLPickupEquipmentNeeded; }
		}

		public override IEnumerable<ZString> GetMatchingDirectionCodes()
		{
			return new ZString[] { CartageDirection.Export, CartageDirection.Origin };
		}
	}
}
