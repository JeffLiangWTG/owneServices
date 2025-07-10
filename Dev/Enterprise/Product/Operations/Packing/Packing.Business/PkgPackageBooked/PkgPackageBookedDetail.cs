using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Integration.Packing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Packing.Business
{
	public class PkgPackageBookedDetail : AutoPkgPackageBookedDetail
	{
		public PkgPackageBookedDetail(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			var defaultUnits = (IPackageDefaultUQs)new PkgDefaultUnits();
			var row = ((IBusinessObjectInternals)this).Row;
			row[PkgPackageBookedDetailSchema.Constants.KPB_PackageQty] = 1;
			row[PkgPackageBookedDetailSchema.Constants.KPB_WeightUQ] = defaultUnits.DefaultWeightUnit;
			row[PkgPackageBookedDetailSchema.Constants.KPB_VolumeUQ] = defaultUnits.DefaultVolumeUnit;
			row[PkgPackageBookedDetailSchema.Constants.KPB_DimensionUQ] = defaultUnits.DefaultDimensionUnit;
		}
	}
}
