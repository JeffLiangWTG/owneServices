using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Schema;
using Res = Enterprise.Warehouse.Transit.DataTransfer.Universal.Res;

namespace Enterprise.Warehouse.Transit.DataTransfer
{
	public class PackageStateTransitLogHelper : TransitLogTableHelper<WhsItemPackageState, TransitLogColumnIDs.PackageStateColumn>
	{
		protected override void AddFetchHints(IEnumerable<WhsItemPackageState> businessObjects)
		{
			var factory = businessObjects.First().Factory;
			factory.AddFetchHint(typeof(PkgPackage), new ZQuery(PkgPackageSchema.PK, businessObjects.Select(ps => ps.WPS_KP_Package)));
		}

		protected override ZString GetValue(WhsItemPackageState packageState, TransitLogColumnIDs.PackageStateColumn columnID)
		{
			switch (columnID)
			{
				case TransitLogColumnIDs.PackageStateColumn.Package:
					return packageState?.FormattedReference ?? "";
				case TransitLogColumnIDs.PackageStateColumn.RCN:
					return packageState?.ReceiveConsignment?.FormattedReference ?? "";
				case TransitLogColumnIDs.PackageStateColumn.DCN:
					return packageState?.DispatchConsignment?.FormattedReference ?? "";
				case TransitLogColumnIDs.PackageStateColumn.ASN:
					return packageState?.ReceiveASN?.FormattedReference ?? "";
				case TransitLogColumnIDs.PackageStateColumn.RTU:
					return packageState?.ReceiveTransportationUnit?.FormattedReference ?? "";
				case TransitLogColumnIDs.PackageStateColumn.DTU:
					return packageState?.DispatchTransportationUnit?.FormattedReference ?? "";
				case TransitLogColumnIDs.PackageStateColumn.LoadList:
					return packageState?.DispatchLoadList?.FormattedReference ?? "";
				case TransitLogColumnIDs.PackageStateColumn.Status:
					return GetStatus(packageState);
				default:
					return "";
			}
		}

		protected override ZString GetHeader(TransitLogColumnIDs.PackageStateColumn columnID)
		{
			switch (columnID)
			{
				case TransitLogColumnIDs.PackageStateColumn.Package:
					return Res.GetString("e140e4fa-625f-45f0-914b-e8d51ba2d45b", "Package");
				case TransitLogColumnIDs.PackageStateColumn.RTU:
					return Res.GetString("2014c9b3-48f3-4d2e-bbe6-3a193415d736", "RTU");
				case TransitLogColumnIDs.PackageStateColumn.RCN:
					return Res.GetString("15cd2249-ed3b-4e9f-8157-1c5a3ed7b3bd", "RCN");
				case TransitLogColumnIDs.PackageStateColumn.ASN:
					return Res.GetString("21198489-673d-4315-b8d5-9459c63a671f", "ASN");
				case TransitLogColumnIDs.PackageStateColumn.DCN:
					return Res.GetString("8435e581-0232-4515-bd2e-e72799cfb572", "DCN");
				case TransitLogColumnIDs.PackageStateColumn.DTU:
					return Res.GetString("018d20f5-c549-4756-962a-b529155c5f85", "DTU");
				case TransitLogColumnIDs.PackageStateColumn.LoadList:
					return Res.GetString("2ea86704-0043-4280-99d8-4d96b5f7cdb8", "Load List");
				case TransitLogColumnIDs.PackageStateColumn.Status:
					return Res.GetString("60d55185-484b-412a-a7fa-ca31e81c3f8e", "Status");
				default:
					return "";
			}
		}

		public static ZString GetStatus(WhsItemPackageState packageState)
		{
			if (packageState != null)
			{
				var statusCodeDescriptionPairs = new TransitWarehouseStatuses();
				return statusCodeDescriptionPairs.GetDescriptionFromCode(packageState.WPS_Status);
			}

			return "";
		}
	}
}
