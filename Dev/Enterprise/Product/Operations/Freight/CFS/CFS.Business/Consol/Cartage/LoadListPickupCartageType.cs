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
	public class LoadListPickupCartageType : LoadListCartageType
	{
		public LoadListPickupCartageType(CFSLoadListConsol consol) : base(consol) { }

		public override MultilingualString Description
		{
			get { return ResString.GetMultilingualString("281b6caa-1656-4f01-9060-e5a33d77aeb9", "Pickup"); }
		}

		public override OrgAddress LocalTransportProviderAddress
		{
			get { return LoadListParent.PickupCartageOrg != null ? LoadListParent.PickupCartageOrg.MainAddress : null; }
		}

		public override ZPropertyInfo CartageAddressInfo
		{
			get
			{
				return LoadListParent.JK_OA_DeparturePackCFSTransportAddressInfo;
			}
		}

		public override ZString CartageJobType
		{
			get
			{
				var result = Constants.CartageJobType.NEW_FCLExportPack;

				var transportMode = LoadListParent.TransportMode;
				if (transportMode == Constants.TransportModes.Sea)
				{
					result = Constants.CartageJobType.NEW_FCLCFStoCTO;
				}
				else if (transportMode == Constants.TransportModes.Air)
				{
					if (LoadListParent.Containers.Count > 0)
					{
						result = Constants.CartageJobType.NEW_AirExportULDCFStoCTO;
					}
					else
					{
						result = Constants.CartageJobType.NEW_AirExportLooseCFStoCTO;
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
					return LoadListParent.GetDepartureCFSDocAddress;
				case LocalCartageJobOrgTypeList.Codes.CNE:
					return LoadListParent.GetConsigneeDeliveryDocAddress;
				case LocalCartageJobOrgTypeList.Codes.CNR:
					return LoadListParent.GetConsignorDocAddress;
				case LocalCartageJobOrgTypeList.Codes.CTO:
					return LoadListParent.GetDepartureCTODocAddress;
				case LocalCartageJobOrgTypeList.Codes.CYD:
					return LoadListParent.GetDepartureContainerYardDocAddress;
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
			if (addressType == DocAddressType.LocalCartageExporter)
			{
				commonContainer.OriginConfirm.EU_PickupDeliveryTime = timeOut;
			}
			else if (addressType == DocAddressType.LocalCartageCFS)
			{
				commonContainer.OriginCFSDeparture.EU_PickupDeliveryTime = timeOut;
			}
		}

		public override void DeliveryCompleted(AutoJobContainer container, DocAddressType addressType, ZDateTime timeOut)
		{
			CommonContainer commonContainer = (CommonContainer)container;
			if (addressType == DocAddressType.LocalCartageCFS)
			{
				commonContainer.OriginCFSArrival.EU_PickupDeliveryTime = timeOut;
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
			return new ZString[] { CartageDirection.Export, CartageDirection.Origin };
		}
	}
}
