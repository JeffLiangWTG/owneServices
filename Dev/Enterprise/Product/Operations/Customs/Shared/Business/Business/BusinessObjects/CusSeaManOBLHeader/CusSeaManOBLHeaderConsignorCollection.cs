using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public class CusSeaManOBLHeaderConsignorCollection : ConsignorCollection
	{
		public CusSeaManOBLHeaderConsignorCollection(BusinessObjectFactory factory, AutoCusSeaManOBLHeader parent) : base(factory)
		{
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(OrgConstants.FilterControl.UNLOCOType.OrgPort, "Property", parent.BO_RL_NKLoadPort));
		}
	}
}
