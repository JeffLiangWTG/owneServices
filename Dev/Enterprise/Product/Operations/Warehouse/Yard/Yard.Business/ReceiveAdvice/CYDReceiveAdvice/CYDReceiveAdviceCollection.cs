using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDReceiveAdviceCollection : ActiveBusinessObjectCollection<CYDReceiveAdvice>
	{
		public CYDReceiveAdviceCollection(BusinessObjectFactory factory) : base(factory) { }
	}
}
