using CargoWise.EntityFramework;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class CommonWorkSheetCollection : ActiveBusinessObjectCollection<CommonWorkSheet>
	{
		public CommonWorkSheetCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
