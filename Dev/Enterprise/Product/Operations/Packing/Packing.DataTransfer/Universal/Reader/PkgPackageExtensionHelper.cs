using CargoWise.EntityFramework;
using Enterprise.Packing.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Packing.DataTransfer.Universal
{
	public static class PkgPackageExtensionHelper
	{
		public static void CreateOrUpdatePackageExtension(this IPackingParentSupportsPackageExtensions packingParent, PkgPackage package, IXmlImportLogger logger)
		{
			var factory = packingParent.Factory;
			var query = new ZQuery(PkgPackageExtensionSchema.KPN_ParentID, packingParent.PK);
			var existingPackageExtension = packingParent.Factory.LoadTop1<PkgPackageExtension>(query);
			var packageExtension = existingPackageExtension ?? factory.New<PkgPackageExtension>();
			packageExtension.SetValue(PkgPackageExtensionSchema.KPN_KP_Package, package.PK, logger);
			packageExtension.SetValue(PkgPackageExtensionSchema.KPN_ParentID, packingParent.PK, logger);
			packageExtension.SetValue(PkgPackageExtensionSchema.KPN_ParentTableCode, packingParent.TablePrefix, logger);
		}
	}
}
