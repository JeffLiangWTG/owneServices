using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Packing.DataTransfer.Universal
{
	public class PkgPackageHeaderHelper : DataObjectReader
	{
		PkgPackageHeaderHelper(IXmlImportLogger logger)
			: base(logger)
		{
		}

		public static void SetPackageQuanityAndID_IfValid(UniversalObjectFactory factory, IXmlImportLogger logger, PkgPackage package, ZLong? qty, ZString? packageId, bool importInnersAsNonTrackableItem = false)
		{
			var helper = new PkgPackageHeaderHelper(logger);
			helper.SetValue(package, PkgPackageSchema.KP_PackageQty, qty);
			if (package.PackageJob?.ParentJob as IPackingParentSupportsImportingBookedDimensions != null)
			{
				helper.SetValue(package.BookedDimensions, PkgPackageBookedDetailSchema.KPB_PackageQty, qty);
			}
			if (!importInnersAsNonTrackableItem)
			{
				helper.SetPackageID_IfQuanityValid(factory, package, packageId);
			}
		}

		public static void SetPackageQuanityAndID_IfValid(UniversalObjectFactory factory, IXmlImportLogger logger, PkgPackage package, ZInt? qty, ZString? packageId, bool importInnersAsNonTrackableItem = false)
		{
			var helper = new PkgPackageHeaderHelper(logger);
			helper.SetValue(package, PkgPackageSchema.KP_PackageQty, qty);
			if (package.PackageJob?.ParentJob as IPackingParentSupportsImportingBookedDimensions != null)
			{
				helper.SetValue(package.BookedDimensions, PkgPackageBookedDetailSchema.KPB_PackageQty, qty);
			}
			if (!importInnersAsNonTrackableItem)
			{
				helper.SetPackageID_IfQuanityValid(factory, package, packageId);
			}
		}

		void SetPackageID_IfQuanityValid(UniversalObjectFactory factory, PkgPackage package, ZString? packageId)
		{
			if (packageId.HasValue && !packageId.Value.IsEmpty)
			{
				if (package.KP_PackageQty == 1)
				{
					var existingPackageIdPK = package.GetValue(PkgPackageSchema.KP_KPH_PackageHeader);
					// no need to update package id Row if it exists (!existingPackageIdPK.IsEmpty) because if it exists we would have matched on Package ID Value.
					if (existingPackageIdPK.IsEmpty)
					{
						var packageID = factory.RowFactory.NewRowWithPK(PkgPackageHeaderSchema.Instance);
						var packageIDPK = packageID.GetValue(PkgPackageHeaderSchema.PK);
						SetValue(packageID, PkgPackageHeaderSchema.KPH_PackageID, packageId);
						SetValue(packageID, PkgPackageHeaderSchema.KPH_SystemCreateTimeUtc, ZDateTime.UtcNow);
						SetValue(packageID, PkgPackageHeaderSchema.KPH_SystemLastEditTimeUtc, ZDateTime.UtcNow);
						SetValue(packageID, PkgPackageHeaderSchema.KPH_SystemCreateUser, GlbStaff.CurrentUser.GS_Code);
						SetValue(packageID, PkgPackageHeaderSchema.KPH_SystemLastEditUser, GlbStaff.CurrentUser.GS_Code);
						SetValue(package, PkgPackageSchema.KP_KPH_PackageHeader, packageIDPK);
					}
				}
				else
				{
					SetValue(package, PkgPackageSchema.KP_KPH_PackageHeader, ZGuid.Empty);
					logger.Log(LogType.Warning, Res.GetString("PkgPackageHeaderHelper_CreatePackageIdForPackage_PackageIdNotPopulated", "Could not populate Package ID '{0}' because Package Quantity '{1}' is not 1.", packageId.Value, package.KP_PackageQty));
				}
			}
			else
			{
				SetValue(package, PkgPackageSchema.KP_KPH_PackageHeader, ZGuid.Empty);
			}
		}
	}
}
