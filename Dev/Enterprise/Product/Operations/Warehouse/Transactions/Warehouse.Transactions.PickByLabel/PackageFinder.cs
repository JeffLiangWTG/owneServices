using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.PickByLabel
{
	public class PackageFinder
	{
		#region Constructor

		public PackageFinder(BusinessObjectFactory factory, string packageID, ZGuid warehousePK, GlbStaff staff)
		{
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNull(staff, nameof(staff));
			Argument.NotNullOrEmpty(packageID, nameof(packageID));

			Factory = factory;
			queryParams = new ZSqlParameterCollection
			{
				{ "@PackageID", packageID, PkgPackageHeaderSchema.KPH_PackageID },
				{ "@WarehousePk", warehousePK, WhsDocketSchema.WD_WW_Whs },
				{ "@User", staff.GS_Code, WhsPickLineSchema.WZ_GS_NKAssignedTo },
				{ "@CaseUOMType", UOMPackTypesList.Codes.Case, RefPackTypeSchema.F3_UOMType },
				{ "@PalletUOMType", UOMPackTypesList.Codes.Pallet, RefPackTypeSchema.F3_UOMType }
			};
		}
		readonly BusinessObjectFactory Factory;
		readonly ZSqlParameterCollection queryParams;

		#endregion

		#region Properties

		ZGuid PackagePK => FirstMatch != null ? (ZGuid)FirstMatch[PkgPackageSchema.Constants.PK] : ZGuid.Empty;
		public ZGuid PickDockDoorLocationPK => FirstMatch != null ? (ZGuid)FirstMatch[WhsPickSchema.Constants.WP_WL_DockDoor] : ZGuid.Empty;

		public PkgPackage Package => package ?? (package = Factory.Load<PkgPackage>(PackagePK));
		PkgPackage package;
		public bool FoundPackage => Package != null;

		public bool IsPickedAndPutaway => FirstMatch == null && isPickedAndPutaway;

		#endregion

		#region Implementation 

		DynamicBusinessObject FirstMatch
		{
			get
			{
				if (!loaded)
				{
					var matches = new DynamicBusinessObjectCollection(Factory);
					matches.Load(Query, queryParams);
					firstMatch = matches.FirstOrDefault(p => (ZInt)p[CanPickedOrPutawayColumn] == 1);
					if (firstMatch == null)
					{
						isPickedAndPutaway = matches.Count > 0 && matches.All(p => (ZInt)p[IsPickedAndPutawayColumn] == 1);
					}
					loaded = true;
				}
				return firstMatch;
			}
		}

		bool loaded;
		bool isPickedAndPutaway;
		DynamicBusinessObject firstMatch;

		const string CanPickedOrPutawayColumn = "CanPickedOrPutaway";
		const string IsPickedAndPutawayColumn = "IsPickedAndPutaway";

		readonly string Query =
$@"
	SELECT
		KP_PK,
		ISNULL(WDA_WL_AssignedDockDoor, WP_WL_DockDoor) AS WP_WL_DockDoor,
		CASE WHEN 
					(
						WZ_WE_OriginalPickedInventoryLine IS NULL AND
						WZ_GS_NKAssignedTo IN ('', @User) AND
						WZ_PickedDateTime IS NULL
					) OR
					(
						WZ_WE_OriginalPickedInventoryLine IS NOT NULL AND
						DockDoorTransferLine.WE_GS_NKPutawayBy = @User AND
						DockDoorTransferLine.WE_FinalisedDate IS NULL
					)
				THEN 1
				ELSE 0
			END {CanPickedOrPutawayColumn},
		CASE WHEN 
					(
						WZ_WE_OriginalPickedInventoryLine IS NULL AND
						WZ_PickedDateTime IS NULL
					) OR
					(
						WZ_WE_OriginalPickedInventoryLine IS NOT NULL AND
						DockDoorTransferLine.WE_FinalisedDate IS NULL
					)
				THEN 0
				ELSE 1
			END {IsPickedAndPutawayColumn}
	FROM
		dbo.PkgPackage
		JOIN dbo.PkgPackageHeader ON KP_KPH_PackageHeader = KPH_PK
		JOIN dbo.RefPackType ON KP_F3_NKPackType = F3_Code
		JOIN dbo.PkgPackageItemDivot ON KI_KP_Package = KP_PK
		JOIN dbo.WhsPickLine ON KI_ParentID = WZ_PK
		JOIN dbo.WhsDocketLine TransactionLine ON WZ_WE_TransactionLine = TransactionLine.WE_PK
		LEFT JOIN dbo.WhsDocketLine DockDoorTransferLine ON WZ_WE_InventoryLine = DockDoorTransferLine.WE_PK AND WZ_WE_OriginalPickedInventoryLine IS NOT NULL
		JOIN dbo.WhsDocket ON TransactionLine.WE_WD = WD_PK
		JOIN dbo.WhsPick ON WD_WP = WP_PK
		LEFT JOIN dbo.WhsDockDoorAssignment ON WP_WDA_DockDoorAssignment = WDA_PK 
	WHERE
		KPH_PackageID = @PackageID AND
		(KP_ClosedTimeUtc IS NULL OR KP_IsClosed = 0) AND
		(KP_ReleasedTimeUtc IS NULL OR KP_IsReleased = 0) AND
		KP_IsReleasedViaJob = 0 AND
		F3_UOMType IN (@CaseUOMType, @PalletUOMType) AND
		(F3_UOMType != @CaseUOMType OR WP_PickCasesByLabel = 1) AND
		(F3_UOMType != @PalletUOMType OR WP_PickPalletsByLabel = 1) AND
		WD_WW_Whs = @WarehousePk
";

		#endregion
	}
}
