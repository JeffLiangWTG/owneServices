using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.CFS.Business
{
	public class LoadListDeliveryCartageType : LoadListCartageType
	{
		public LoadListDeliveryCartageType(CFSLoadListConsol consol) : base(consol) { }

		public override MultilingualString Description
		{
			get { return ResString.GetMultilingualString("4495a55c-c8ab-4207-b5e5-621d21e63737", "Delivery"); }
		}

		public override OrgAddress LocalTransportProviderAddress
		{
			get { return LoadListParent.DeliveryCartageOrg != null ? LoadListParent.DeliveryCartageOrg.MainAddress : null; }
		}

		public override ZPropertyInfo CartageAddressInfo
		{
			get { return LoadListParent.JK_OA_ArrivalUnpackCFSTransportAddressInfo; }
		}

		public override ZString CartageJobType
		{
			get
			{
				var result = Constants.CartageJobType.NEW_FCLImportUnpack;

				var transportMode = LoadListParent.TransportMode;
				if (transportMode == Constants.TransportModes.Sea)
				{
					result = Constants.CartageJobType.NEW_FCLCTOtoCFS;
				}
				else if (transportMode == Constants.TransportModes.Air)
				{
					if (LoadListParent.Containers.Count > 0)
					{
						result = Constants.CartageJobType.NEW_AirImportULDCTOtoCFS;
					}
					else
					{
						result = Constants.CartageJobType.NEW_AirImportLooseCTOtoCFS;
					}
				}

				return result;
			}
		}

		public override JobDocAddress GetCartageAddress(ZString orgType)
		{
			switch (orgType)
			{
				case LocalCartageJobOrgTypeList.Codes.CFS:
					return LoadListParent.GetArrivalCFSDocAddress;
				case LocalCartageJobOrgTypeList.Codes.CNE:
					return LoadListParent.GetConsigneeDeliveryDocAddress;
				case LocalCartageJobOrgTypeList.Codes.CNR:
					return LoadListParent.GetConsignorDocAddress;
				case LocalCartageJobOrgTypeList.Codes.CTO:
					return LoadListParent.GetArrivalCTODocAddress;
				case LocalCartageJobOrgTypeList.Codes.CYD:
					return LoadListParent.GetArrivalContainerYardDocAddress;
				default:
					return null;
			}
		}

		protected override ZPropertyInfo[] GetCartageAddressInfosToMonitor()
		{
			return Array.Empty<ZPropertyInfo>();
		}

		public override void PickupCompleted(AutoJobContainer container, DocAddressType addressType, ZDateTime timeOut)
		{
			CommonContainer commonContainer = (CommonContainer)container;
			if (addressType == DocAddressType.LocalCartageCFS)
			{
				commonContainer.DestinationCFSDeparture.EU_PickupDeliveryTime = timeOut;
			}
		}

		public override void DeliveryCompleted(AutoJobContainer container, DocAddressType addressType, ZDateTime timeOut)
		{
			CommonContainer commonContainer = (CommonContainer)container;
			if (addressType == DocAddressType.LocalCartageImporter)
			{
				commonContainer.DestinationConfirm.EU_PickupDeliveryTime = timeOut;
			}
			else if (addressType == DocAddressType.LocalCartageCFS)
			{
				commonContainer.DestinationCFSArrival.EU_PickupDeliveryTime = timeOut;
			}
		}

		public override void PickupCompleted(DocAddressType addressType, ZDateTime timeOut)
		{
		}

		public override void DeliveryCompleted(DocAddressType addressType, ZDateTime timeOut)
		{
		}

		public override void SetTotalDemurrage(TimeSpan demurrage)
		{
		}

		public override ZString DropMode
		{
			get { return ""; }
		}

		public override IEnumerable<ZString> GetMatchingDirectionCodes()
		{
			return new ZString[] { CartageDirection.Import, CartageDirection.Destination };
		}
	}
}
