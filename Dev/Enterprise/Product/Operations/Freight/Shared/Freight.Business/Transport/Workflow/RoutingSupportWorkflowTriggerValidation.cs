using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	public class RoutingSupportWorkflowTriggerValidation : MilestoneOrTriggerValidation
	{
		public RoutingSupportWorkflowTriggerValidation(RoutingSupportProcessTask parent)
			: base(parent)
		{
		}

		protected override void CheckReferenceCode()
		{
			RoutingSupportMilestoneValidation.CheckReferenceCode(Parent);
		}

		new RoutingSupportProcessTask Parent
		{
			get { return (RoutingSupportProcessTask)base.Parent; }
		}
	}
}
