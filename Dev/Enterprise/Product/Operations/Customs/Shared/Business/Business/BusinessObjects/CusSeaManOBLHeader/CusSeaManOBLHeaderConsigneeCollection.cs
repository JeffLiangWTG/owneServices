using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public class CusSeaManOBLHeaderConsigneeCollection : ConsigneeCollection
	{
		public CusSeaManOBLHeaderConsigneeCollection(BusinessObjectFactory factory, AutoCusSeaManOBLHeader parent) : base(factory)
		{
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(OrgConstants.FilterControl.UNLOCOType.OrgPort, "Property", parent.BO_RL_NKDischargePort));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property5", parent.BO_FreightForwarderIndicator));
		}
	}
}
