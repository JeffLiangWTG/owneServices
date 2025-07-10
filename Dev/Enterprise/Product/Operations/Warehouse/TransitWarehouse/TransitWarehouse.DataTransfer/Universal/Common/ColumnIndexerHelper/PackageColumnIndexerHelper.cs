using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.DataTransfer
{
	public static class PackageColumnIndexerHelper
	{
		public static ZString GetPackageId(UniversalObjectFactory factory, IColumnIndexer package)
		{
			if (package != null)
			{
				var header = GetHeader(factory, package);
				return header.GetValue(PkgPackageHeaderSchema.KPH_PackageID);
			}
			return "";
		}

		public static IColumnIndexer GetHeader(UniversalObjectFactory factory, IColumnIndexer package)
		{
			if (package != null)
			{
				var headerPK = package.GetValue(PkgPackageSchema.KP_KPH_PackageHeader);
				var query = new ZQuery(PkgPackageHeaderSchema.PK, headerPK);
				return DataObjectReader.GetColumnIndexerFromRow(factory.RowFactory.Load(PkgPackageHeaderSchema.Constants.TableName, query).SingleOrDefault());
			}
			return null;
		}
	}
}
