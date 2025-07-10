using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.eManifest.Business
{
	public class VehicleIdentificationNumberCollection : CusCodeDataCollection<VehicleIdentificationNumber>
	{
		public VehicleIdentificationNumberCollection(BusinessObject master)
			: base(master, CusCodeDataTypeList.Codes.VehicleIdentificationNumber)
		{
		}
	}
}
