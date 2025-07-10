using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit
{
	public class OutturnProviderForDCN : IOutturnProvider
	{
		public OutturnProviderForDCN(PkgPackage package)
		{
			this.package = package;
			var directPackageState = package.Factory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, package.PK)).SingleOrDefault();
			packageState = directPackageState == null && package.ParentPackage != null ? package.Factory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, package.KP_KP_ParentPackage)).SingleOrDefault() : directPackageState;
			HasUnloadedInners = packageState != null && packageState.GetChildPackages().Any(p => !p.WPS_UnloadedTime.IsEmpty);
		}
		readonly WhsItemPackageState packageState;
		readonly PkgPackage package;
		readonly bool HasUnloadedInners;

		int IOutturnProvider.OutturnQty
		{
			get
			{
				var qty = 0;
				if (packageState != null)
				{
					if (!packageState.WPS_UnloadedTime.IsEmpty || !packageState.WPS_LoadedTime.IsEmpty)
					{
						qty = packageState.Package.KP_PackageQty;
					}
					else if (packageState.WPS_UnloadedTime.IsEmpty && packageState.WPS_LoadedTime.IsEmpty)
					{
						if (packageState.WPS_UnitType == PackageStateUnitType.Codes.Overpack)
						{
							qty = HasUnloadedInners ? 1 : 0;
						}
					}
				}

				return qty;
			}
		}

		ZDateTime? IOutturnProvider.UnloadDate
		{
			get
			{
				ZDateTime? unloadDate = null;
				if (packageState != null)
				{
					if (!packageState.WPS_UnloadedTime.IsEmpty)
					{
						unloadDate = packageState.WPS_UnloadedTime.ToLocalZDateTime();
					}

					if (packageState.WPS_UnitType == PackageStateUnitType.Codes.Overpack && HasUnloadedInners)
					{
						unloadDate = packageState.WPS_SystemCreateTimeUtc.ToLocalBranchTime();
					}
				}

				return unloadDate;
			}
		}

		ZDateTime? IOutturnProvider.LoadDate
		{
			get
			{
				if (packageState == null || packageState.WPS_LoadedTime == ZDateTimeOffset.Empty)
				{
					return null;
				}
				else
				{
					return packageState.WPS_LoadedTime.ToLocalZDateTime();
				}
			}
		}

		int IOutturnProvider.OutturnDamagedQty => package.KP_IsDamaged ? package.KP_PackageQty : ZInt.Zero;

		int IOutturnProvider.OutturnPillagedQty => package.KP_IsPillaged ? package.KP_PackageQty : ZInt.Zero;

		decimal IOutturnProvider.OutturnedHeight => packageState?.DispatchTransportationUnit != null ? package.KP_Height : ZDecimal.Zero;

		decimal IOutturnProvider.OutturnedLength => packageState?.DispatchTransportationUnit != null ? package.KP_Length : ZDecimal.Zero;

		decimal IOutturnProvider.OutturnedVolume => packageState?.DispatchTransportationUnit != null ? package.KP_Volume : ZDecimal.Zero;

		decimal IOutturnProvider.OutturnedWeight => packageState?.DispatchTransportationUnit != null ? package.KP_Weight : ZDecimal.Zero;

		decimal IOutturnProvider.OutturnedWidth => packageState?.DispatchTransportationUnit != null ? package.KP_Width : ZDecimal.Zero;

		string IOutturnProvider.ActualTransportJobID => packageState?.DispatchTransportationUnit != null
													? packageState.DispatchTransportationUnit.WDH_ReferenceNumber
													: ZString.Empty;

		string IOutturnProvider.ActualTransportJobTypeCode => TWAdditionalReferenceTypesForUniversalXML.Codes.TransitWarehouseDispatchTransportationUnit;

		string IOutturnProvider.ActualTransportJobTypeDescription => TWAdditionalReferenceTypesForUniversalXML.Descriptions.TransitWarehouseDispatchTransportationUnit;

		string IOutturnProvider.ExpectedTransportJobID => packageState?.DispatchLoadList != null
											? packageState.DispatchLoadList.WDL_JobID
											: ZString.Empty;

		string IOutturnProvider.ExpectedTransportJobTypeCode => TWAdditionalReferenceTypesForUniversalXML.Codes.TransitWarehouseDispatchLoadList;

		string IOutturnProvider.ExpectedTransportJobTypeDescription => TWAdditionalReferenceTypesForUniversalXML.Descriptions.TransitWarehouseDispatchLoadList;

		bool IOutturnProvider.IsHighRisk => packageState?.WPS_IsHighRisk ?? false;

		ZString IOutturnProvider.OverriddenAviationSecurityInspectionType => packageState?.OverriddenAviationSecurityInspectionType ?? ZString.Empty;
	}
}
