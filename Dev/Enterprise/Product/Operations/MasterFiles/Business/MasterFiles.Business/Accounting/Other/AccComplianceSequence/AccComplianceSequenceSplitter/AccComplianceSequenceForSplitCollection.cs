using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class AccComplianceSequenceForSplitCollection : ActiveBusinessObjectCollection<AccComplianceSequenceForSplit>
	{
		public AccComplianceSequenceForSplitCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public AccComplianceSequenceForSplitCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override bool AllowNew => false;
	}
}
