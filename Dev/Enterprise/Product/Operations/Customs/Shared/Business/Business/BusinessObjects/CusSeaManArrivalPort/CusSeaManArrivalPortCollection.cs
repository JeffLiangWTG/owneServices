using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class CusSeaManArrivalPortCollection : DependentBusinessObjectCollection<CusSeaManArrivalPort, CusSeaManTranHead>
	{
		public CusSeaManArrivalPortCollection(CusSeaManTranHead parent) : base(parent)
		{
		}
	}
}
