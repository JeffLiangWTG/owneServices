using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Document.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.Document
{
	public static class TransitDocumentHelper
	{
		public static ZString GetEnterpiseAndServerCode()
		{
			var result = ZString.Empty;

			if (RegistrationKey != null)
			{
				result = FormattableString.Invariant($"{RegistrationKey.EnterpriseCode}{RegistrationKey.ServerCode}"); // programmatic constant
			}

			return result;
		}

		static IProductRegistrationKey RegistrationKey => Key ?? (Key = ObjectFactory.Get<IProductRegistration>().Key);

		static IProductRegistrationKey Key { get; set; }

		public static ICodeDescription GetCustomsStatus(WhsItemReceiveConsignment rcn)
		{
			var customsStatus = new CodeDescription(new CIN750CustomsStatus());
			switch (rcn.WRC_Direction)
			{
				case TransitWarehouseConsignmentDirections.Codes.Export:
					customsStatus.Code = IsFromEU(rcn.Destination) ?
										 CIN750CustomsStatus.Codes.Domestic :
										 CIN750CustomsStatus.Codes.Export;
					break;
				case TransitWarehouseConsignmentDirections.Codes.Import:
					customsStatus.Code = IsFromEU(rcn.PreviousLoadPort) &&
										 rcn.HasCustomsReleaseNumber() ?
										 CIN750CustomsStatus.Codes.Domestic :
										 CIN750CustomsStatus.Codes.Import;
					break;
				case TransitWarehouseConsignmentDirections.Codes.Domestic:
					customsStatus.Code = CIN750CustomsStatus.Codes.Domestic;
					break;
			}
			return customsStatus;
		}

		public static ICodeDescription GetCustomsStatus(WhsItemDispatchConsignment dcn)
		{
			var customsStatus = new CodeDescription(new CIN750CustomsStatus());
			switch (dcn.WDC_Direction)
			{
				case TransitWarehouseConsignmentDirections.Codes.Export:
					if (dcn.Destination != null)
					{
						customsStatus.Code = IsFromEU(dcn.Destination) ?
											 CIN750CustomsStatus.Codes.Domestic :
											 CIN750CustomsStatus.Codes.Export;
					}
					else
					{
						var rcns = GetOutMessageRelatedRCNs(dcn);
						customsStatus.Code = rcns.Any() && rcns.All(rcn => IsFromEU(rcn.Destination)) ?
											 CIN750CustomsStatus.Codes.Domestic :
											 CIN750CustomsStatus.Codes.Export;
					}
					break;
				case TransitWarehouseConsignmentDirections.Codes.Import:
					{
						var rcns = GetOutMessageRelatedRCNs(dcn);
						customsStatus.Code = rcns.Any() && rcns.All(rcn => rcn.HasCustomsReleaseNumber()) &&
											 (IsFromEU(dcn.Destination) || rcns.All(rcn => IsFromEU(rcn.PreviousLoadPort)) || rcns.All(rcn => IsFromEU(rcn.Destination))) ?
											 CIN750CustomsStatus.Codes.Domestic :
											 CIN750CustomsStatus.Codes.Import;
					}
					break;
				case TransitWarehouseConsignmentDirections.Codes.Domestic:
					customsStatus.Code = CIN750CustomsStatus.Codes.Domestic;
					break;
			}
			return customsStatus;
		}

		static bool IsFromEU(RefUNLOCO unloco) => (unloco?.Country?.RN_EconomicGrouping ?? ZString.Empty) == EconomicGroupList.Codes.EuropeanUnion;

		static IEnumerable<WhsItemReceiveConsignment> GetOutMessageRelatedRCNs(WhsItemDispatchConsignment dcn) => dcn.PackageStates.Where(p => !p.WPS_WDH_TransitDispatchHeader.IsEmpty).Select(p => p.ReceiveConsignment).Distinct();

		public static List<WhsItemPackageState> GetRCNArrivedPackageStates(WhsItemReceiveConsignment rcn)
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
		AND (Package.KP_KP_TopHandlingUnitPackage IS NULL OR ParentPkgState.WPS_WRC_TransitReceiveConsignment IS NULL OR ParentPkgState.WPS_UnloadedTime IS NULL)
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

		public static ZString RemoveHyphen(this ZString str) => str.ReplaceIgnoringCase("-", "");
	}
}
