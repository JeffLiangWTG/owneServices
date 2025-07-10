using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Freight.Agency.Business;

namespace Enterprise.Customs.US.AMS.Business
{
	public class CusInBondVehicleCollection : ActiveBusinessObjectCollection<CusInBondVehicleCtrl>, ISailingSynchronisationTargetCollection<AgencyShipmentContainer, CusInBondVehicleCtrl>
	{
		public CusInBondVehicleCollection(CusInBondContainer master)
			: base(master)
		{
		}
	}
}
