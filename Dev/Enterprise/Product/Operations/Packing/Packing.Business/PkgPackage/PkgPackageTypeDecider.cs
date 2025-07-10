using System;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Packing.Business
{
	public class PkgPackageTypeDecider : TypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var packageJobPK = new ZGuid(row[PkgPackageSchema.KP_KJ_ParentPackageJob.Name]);

			var rowFactory = ((IBusinessObjectFactoryInternals)factory).RowFactory;
			var packageJobRow = rowFactory.LoadFromPK(PkgPackageJobSchema.Constants.TableName, packageJobPK);
			var parentTableCode = packageJobRow?[PkgPackageJobSchema.KJ_ParentTableCode.Name];

			switch (parentTableCode)
			{
				case CusPackingListSchema.Constants.Prefix:
					return CusPackageTypeDecider.Value.GetTypeForLoad(row, factory);
				default:
					return typeof(PkgPackage);
			}
		}

		readonly Lazy<TypeDecider> CusPackageTypeDecider = new Lazy<TypeDecider>(() => (TypeDecider)ObjectFactory.Get<Enterprise.Integration.Customs.Shared.ICusPackageTypeDecider>());

		public override Type GetTypeForBinding() => typeof(PkgPackage);

		public override Type GetTypeForNew() => typeof(PkgPackage);
	}
}
