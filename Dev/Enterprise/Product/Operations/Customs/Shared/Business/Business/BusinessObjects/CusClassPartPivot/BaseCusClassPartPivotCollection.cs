using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class BaseCusClassPartPivotCollection : BusinessObjectCollection<BaseCusClassPartPivot>
	{
		public BaseCusClassPartPivotCollection(BusinessObjectFactory factory, ZQuery additionalFilter)
			: base(factory, additionalFilter)
		{
		}

		public BaseCusClassPartPivotCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
