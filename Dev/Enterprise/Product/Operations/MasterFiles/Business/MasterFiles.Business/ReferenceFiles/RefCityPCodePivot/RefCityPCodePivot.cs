using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefCityPCodePivot : AutoRefCityPCodePivot
	{
		public RefCityPCodePivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			DataRow row = ((IBusinessObjectInternals)this).Row;

			row[RefCityPCodePivotSchema.Constants.R0_IsSystem] = false;
		}
	}
}
