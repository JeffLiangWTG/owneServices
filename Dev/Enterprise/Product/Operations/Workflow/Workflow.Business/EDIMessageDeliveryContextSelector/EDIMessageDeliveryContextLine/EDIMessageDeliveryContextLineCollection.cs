using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Workflow.Business
{
	public class EDIMessageDeliveryContextLineCollection : ActiveBusinessObjectCollection<EDIMessageDeliveryContextLine>
	{
		public EDIMessageDeliveryContextLineCollection(EDIMessageDeliveryContextSelector parent)
			: base(parent.Factory)
		{
			Parent = parent;
		}

		readonly EDIMessageDeliveryContextSelector Parent;

		protected override void SetRelationshipDefaultsForElementCore(EDIMessageDeliveryContextLine newElement, bool throwIfRelationshipNotSupported)
		{
			base.SetRelationshipDefaultsForElementCore(newElement, throwIfRelationshipNotSupported);
			newElement.ECL_ECS_MessageDeliveryContextSelector = Parent.PK;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery filter = base.CreateRelationshipFilter();
			filter.AddToFilter(EDIMessageDeliveryContextLineSchema.ECL_ECS_MessageDeliveryContextSelector, Parent.PK);
			return filter;
		}
	}
}
