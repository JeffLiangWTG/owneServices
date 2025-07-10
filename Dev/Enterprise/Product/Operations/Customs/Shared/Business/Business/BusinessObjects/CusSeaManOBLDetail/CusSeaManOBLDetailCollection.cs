using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class CusSeaManOBLDetailCollection : DependentBusinessObjectCollection<CusSeaManOBLDetail, CusSeaManOBLHeader>
	{
		public CusSeaManOBLDetailCollection(CusSeaManOBLHeader parent) : base(parent)
		{
		}
	}
}
