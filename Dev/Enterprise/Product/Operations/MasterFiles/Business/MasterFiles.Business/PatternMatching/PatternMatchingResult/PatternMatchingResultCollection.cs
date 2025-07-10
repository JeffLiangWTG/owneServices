using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class PatternMatchingResultCollection : ActiveBusinessObjectCollection<PatternMatchingResult>
	{
		public PatternMatchingResultCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public PatternMatchingResultCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}
	}
}
