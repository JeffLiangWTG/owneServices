using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.DataTransfer.Universal.Common
{
	public static class DeleteDetachedPackageHelper
	{
		public static void DeleteDetachedPackages(UniversalObjectFactory factory)
		{
			var queryDetachedPackages = new ZQuery(PkgPackageSchema.KP_KJ_ParentPackageJob, ZGuid.Empty);
			var detachedPackages = factory.RowFactory.Load(PkgPackageSchema.Constants.TableName, queryDetachedPackages);
			var detachedPackageIDPKs = detachedPackages.Select(p => DataObjectReader.GetColumnIndexerFromRow(p).GetValue(PkgPackageSchema.KP_KPH_PackageHeader));
			DeletePackageIDs(detachedPackageIDPKs, factory);

			foreach (var detachedPackage in detachedPackages)
			{
				var row = DataObjectReader.GetColumnIndexerFromRow(detachedPackage);
				DeleteContainer(row, factory);

				factory.DeleteRowAndSetHasChanges<PkgPackage>(row, PkgPackageSchema.PK);
			}
		}

		static void DeletePackageIDs(IEnumerable<ZGuid> packageIDPKs, UniversalObjectFactory factory)
		{
			var detachedPackageIDs = factory.RowFactory.Load(PkgPackageHeaderSchema.Constants.TableName, new ZQuery(PkgPackageHeaderSchema.PK, packageIDPKs));

			foreach (var detachedPackageID in detachedPackageIDs)
			{
				factory.DeleteRowAndSetHasChanges<PkgPackageHeader>(DataObjectReader.GetColumnIndexerFromRow(detachedPackageID), PkgPackageHeaderSchema.PK);
			}
		}

		static void DeleteContainer(IColumnIndexer row, UniversalObjectFactory factory)
		{
			if (row.GetValue(PkgPackageSchema.KP_KP_ParentPackage).IsEmpty)
			{
				var container = factory.RowFactory.LoadFromNaturalKey(PkgPackageContainerSchema.Constants.TableName, PkgPackageContainerSchema.K0_KP_Package, row.GetValue(PkgPackageSchema.PK), false);
				var containerRow = DataObjectReader.GetColumnIndexerFromRow(container);
				if (containerRow != null)
				{
					factory.DeleteRowAndSetHasChanges<PkgPackageContainer>(containerRow, PkgPackageContainerSchema.PK);
				}
			}
		}
	}
}
