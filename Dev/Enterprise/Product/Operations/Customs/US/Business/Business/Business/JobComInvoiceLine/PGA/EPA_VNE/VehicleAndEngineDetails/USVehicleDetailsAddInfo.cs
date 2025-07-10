using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.USPGAVehicleDetails)]
	public class USVehicleDetailsAddInfo : AutoUSVehicleDetailsAddInfo, Integration.Customs.US.IVehicleDetailsAddInfo
	{
		public USVehicleDetailsAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		public VehicleDetails VehicleDetails => Parent as VehicleDetails;
	}
}
