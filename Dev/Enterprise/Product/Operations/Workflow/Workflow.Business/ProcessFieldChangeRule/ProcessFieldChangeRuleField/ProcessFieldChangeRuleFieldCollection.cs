using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Workflow.Business
{
	public class ProcessFieldChangeRuleFieldCollection : ActiveBusinessObjectCollection<ProcessFieldChangeRuleField>
	{
		public ProcessFieldChangeRuleFieldCollection(ProcessFieldChangeRule parent) : base(parent.Factory)
		{
			this.Parent = parent;
		}

		readonly ProcessFieldChangeRule Parent;

		protected override void SetRelationshipDefaultsForElementCore(ProcessFieldChangeRuleField newElement, bool throwIfRelationshipNotSupported)
		{
			base.SetRelationshipDefaultsForElementCore(newElement, throwIfRelationshipNotSupported);
			newElement.PFL_PFR = Parent.PK;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery filter = base.CreateRelationshipFilter();
			filter.AddToFilter(ProcessFieldChangeRuleFieldSchema.PFL_PFR, Parent.PK);
			return filter;
		}
	}
}
