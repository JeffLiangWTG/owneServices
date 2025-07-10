using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Business
{
	public class JobComInvLineRefsCollection<T> : DependentBusinessObjectCollection<T, BusinessObject> where T : JobComInvLineRefs
	{
		public JobComInvLineRefsCollection(BusinessObject parent, ZString referenceType)
			: base(parent)
		{
			this.JG_ReferenceType = referenceType;
		}

		public readonly ZString JG_ReferenceType;

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery result = base.CreateRelationshipFilter();
			result.AddToFilter(JobComInvLineRefsSchema.JG_ReferenceType, JG_ReferenceType);
			return result;
		}

		protected override void SetCollectionRelationships(BusinessObject dependent)
		{
			base.SetCollectionRelationships(dependent);
			JobComInvLineRefs child = (JobComInvLineRefs)dependent;
			child.JG_ReferenceType = JG_ReferenceType;
		}
	}
}
