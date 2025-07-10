using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.eManifest.Business
{
	public class VehicleIdentificationNumber : CusCodeData
	{
		public VehicleIdentificationNumber(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.VehicleIdentificationNumber;
			CY_Code = CusCodeDataTypeList.Codes.VehicleIdentificationNumber;
		}

		protected override TypeLoaderCollection parentLoaders
		{
			get { return new TypeLoaderCollection(typeof(Commodity)); }
		}

		protected override Customs.Business.CusCodeDataLookups GetNewLookups()
		{
			return new CusCodeDataLookups(this);
		}

		protected override Customs.Business.CusCodeDataValidation GetNewValidation()
		{
			return new CusCodeDataValidation(this);
		}
	}
}
