using CargoWise.EntityFramework;

namespace Enterprise.HRM.Common
{
	public class HrlProcessingRunCollection : ActiveBusinessObjectCollection<HrlProcessingRun>
	{
		public HrlProcessingRunCollection(BusinessObjectFactory factory) : base(factory)
		{
		}
	}
}
