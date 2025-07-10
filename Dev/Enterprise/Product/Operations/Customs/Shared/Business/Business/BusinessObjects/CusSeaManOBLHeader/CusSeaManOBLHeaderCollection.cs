using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class CusSeaManOBLHeaderCollection : DependentBusinessObjectCollection<CusSeaManOBLHeader, CusSeaManTranHead>
	{
		public CusSeaManOBLHeaderCollection(CusSeaManTranHead parent) : base(parent)
		{
		}
	}
}
