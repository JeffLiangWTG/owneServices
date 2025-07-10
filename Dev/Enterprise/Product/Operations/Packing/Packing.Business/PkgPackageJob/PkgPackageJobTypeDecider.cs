using System;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Packing.Business
{
	public class PkgPackageJobTypeDecider : TypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var parentTableCode = row[PkgPackageJobSchema.KJ_ParentTableCode.Name].ToString();
			switch (parentTableCode)
			{
				case CusPackingListSchema.Constants.Prefix:
					return CusPackageJobTypeDecider.Value.GetTypeForLoad(row, factory);
				case DtbBookingConsolidationSchema.Constants.Prefix:
					return DtbBookingConsolidationPackageJobType.Value;
				default:
					return typeof(PkgPackageJob);
			}
		}

		public static Type GetTypeByParentTableCode(string parentTableCode)
		{
			switch (parentTableCode)
			{
				case CusPackingListSchema.Constants.Prefix:
					return CusPackageJobType.Value;
				case DtbBookingConsolidationSchema.Constants.Prefix:
					return DtbBookingConsolidationPackageJobType.Value;
				default:
					return typeof(PkgPackageJob);
			}
		}

		readonly Lazy<TypeDecider> CusPackageJobTypeDecider = new Lazy<TypeDecider>(() => (TypeDecider)ObjectFactory.Get<Enterprise.Integration.Customs.Shared.ICusPackageJobTypeDecider>());
		static readonly Lazy<Type> CusPackageJobType = new Lazy<Type>(() => ObjectFactory.GetType<Enterprise.Integration.Customs.ICusPackageJob>());

		static readonly Lazy<Type> DtbBookingConsolidationPackageJobType = new Lazy<Type>(() => ObjectFactory.GetType<Enterprise.Integration.TransportBooking.IDtbBookingConsolidationPkgPackageJob>());

		public override Type GetTypeForBinding() => typeof(PkgPackageJob);

		public override Type GetTypeForNew() => typeof(PkgPackageJob);
	}
}
