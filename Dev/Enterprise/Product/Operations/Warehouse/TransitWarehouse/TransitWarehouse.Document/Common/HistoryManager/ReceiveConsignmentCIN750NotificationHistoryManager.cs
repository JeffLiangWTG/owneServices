using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.Document
{
	public class ReceiveConsignmentCIN750NotificationHistoryManager : CIN750NotificationHistoryManager<WhsItemReceiveConsignment>
	{
		public ReceiveConsignmentCIN750NotificationHistoryManager(WhsItemReceiveConsignment rcn)
		{
			Parent = rcn;
			HistoryInfo = GetCIN750NotificationHistory(rcn);
		}

		public OutNotificationAdditionalData AdditionalDataForRCN { get; set; }

		public override (CIN750NotificationMessageTypes? Type, string ErrorMessage) GetNextMessageTypeCore(CIN750NotificationMessageTypes? messageType = null)
		{
			var errorMessage = string.Empty;
			var packages = GetRCNArrivedPackageStates(Parent);
			var arrivedPackagesQty = packages.Sum(p => p.Package.KP_PackageQty);
			var arrivedPackagesWeight = packages.Sum(p => Core.Constants.Weight.Convert(p.Package.KP_Weight, p.Package.KP_WeightUQ, Core.Constants.Weight.Kilograms)).RoundTo3Digits();

			AdditionalDataForRCN = new OutNotificationAdditionalData()
			{
				PackageQuantityReadyToOut = arrivedPackagesQty - InHistory.Sum(h => h.Quantity) + CorHistory.Sum(h => h.Quantity),
				PackageWeightReadyToOut = packages.Sum(p => Core.Constants.Weight.Convert(p.Package.KP_Weight, p.Package.KP_WeightUQ, Core.Constants.Weight.Kilograms)) - InHistory.Sum(h => h.Weight) + CorHistory.Sum(h => h.Weight)
			};

			if (InHistory.Sum(h => h.Quantity) + CorHistory.Sum(h => h.Quantity) < arrivedPackagesQty)
			{
				if (messageType == CIN750NotificationMessageTypes.CIN750CorNotification)
				{
					errorMessage = null;
				}
				messageType = CIN750NotificationMessageTypes.CIN750InNotification;
			}
			else if (InHistory.Sum(h => h.Quantity) + CorHistory.Sum(h => h.Quantity) > arrivedPackagesQty)
			{
				if (messageType == CIN750NotificationMessageTypes.CIN750InNotification)
				{
					errorMessage = GetErrorMessage(CIN750NotificationMessageTypes.CIN750InNotification, CIN750NotificationMessageTypes.CIN750CorNotification, Res.GetString("7d497963-6c4f-46b7-9368-5613f7fa79a1", "Reported In Quantity is greater than the Arrived Packages Quantity"));
				}
				messageType = CIN750NotificationMessageTypes.CIN750CorNotification;
			}
			else if (InHistory.Sum(h => h.Weight) + CorHistory.Sum(h => h.Weight) > arrivedPackagesWeight)
			{
				if (messageType == CIN750NotificationMessageTypes.CIN750InNotification)
				{
					errorMessage = GetErrorMessage(CIN750NotificationMessageTypes.CIN750InNotification, CIN750NotificationMessageTypes.CIN750CorNotification, Res.GetString("7e32d7de-2465-49c2-bc7d-df727116ad55", "Reported In Weight is greater than the Arrived Packages Weight"));
				}
				messageType = CIN750NotificationMessageTypes.CIN750CorNotification;
			}
			else if (InHistory.Sum(h => h.Weight) + CorHistory.Sum(h => h.Weight) < arrivedPackagesWeight)
			{
				if (messageType == CIN750NotificationMessageTypes.CIN750InNotification)
				{
					errorMessage = GetErrorMessage(CIN750NotificationMessageTypes.CIN750InNotification, CIN750NotificationMessageTypes.CIN750CorNotification, Res.GetString("299ab8ab-5763-4f16-ad21-9160bddc0e63", "Reported In Weight is less than the Arrived Packages Weight"));
				}
				messageType = CIN750NotificationMessageTypes.CIN750CorNotification;
			}
			else
			{
				errorMessage = GetErrorMessage(messageType, null, Res.GetString("d7de147d-7b35-44be-acf5-0b7ba084cf86", "the Packages on the Receive Consignment are reported completely"));
				messageType = null;
			}
			return (messageType, errorMessage);
		}

		List<WhsItemPackageState> GetRCNArrivedPackageStates(WhsItemReceiveConsignment rcn)
		{
			var packageStateQuery = @"
WPS_PK IN
(
	SELECT
		PkgState.WPS_PK
	FROM
		dbo.WhsItemPackageState AS PkgState
		JOIN dbo.PkgPackage AS Package ON Package.KP_PK = PkgState.WPS_KP_Package
		LEFT JOIN dbo.PkgPackageHandlingUnitDivot AS Divot ON Package.KP_PK = Divot.KPD_KP_Package
		LEFT JOIN dbo.PkgPackage AS ParentPackage ON ParentPackage.KP_PK = Divot.KPD_KP_HandlingUnit AND Divot.KPD_UnpackedTime IS NULL
		LEFT JOIN dbo.WhsItemPackageState AS ParentPkgState ON ParentPackage.KP_PK = ParentPkgState.WPS_KP_Package
	WHERE
		PkgState.WPS_WRC_TransitReceiveConsignment = @ReceiveConsignmentPK
		AND PkgState.WPS_UnloadedTime IS NOT NULL
		AND PkgState.WPS_Status != 'ADJ'
		AND PkgState.WPS_UnitType NOT IN ('HU','CNT','ULD')
		AND (Package.KP_KP_TopHandlingUnitPackage IS NULL OR Package.KP_KP_TopHandlingUnitPackage = Package.KP_PK OR ParentPkgState.WPS_WRC_TransitReceiveConsignment IS NULL OR ParentPkgState.WPS_UnloadedTime IS NULL OR ParentPkgState.WPS_UnitType IN ('HU','CNT','ULD'))
)";

			var sqlParams = new ZSqlParameterCollection
			{
				{ "@ReceiveConsignmentPK", rcn.PK, WhsItemReceiveConsignmentSchema.PK },
			};

			var query = new ZDBOnlyQuery(typeof(WhsItemPackageState));
			query.AddFilterAndZSQLParameterCollection(packageStateQuery, sqlParams);

			var rcnArrivedPackagesStates = rcn.Factory.Load<WhsItemPackageState>(query).Select(p => p.BreakDownParentPackageState ?? p).Distinct().ToList();
			return rcnArrivedPackagesStates;
		}
	}
}
