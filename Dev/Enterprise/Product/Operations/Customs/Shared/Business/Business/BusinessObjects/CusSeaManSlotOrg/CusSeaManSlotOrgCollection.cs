using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class CusSeaManSlotOrgCollection : DependentBusinessObjectCollection<CusSeaManSlotOrg, CusSeaManTranHead>
	{
		public CusSeaManSlotOrgCollection(CusSeaManTranHead parent) : base(parent)
		{
		}
	}
}
