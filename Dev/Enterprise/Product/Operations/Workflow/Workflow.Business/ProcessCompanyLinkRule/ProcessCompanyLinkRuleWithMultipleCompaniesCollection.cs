using CargoWise.EntityFramework;

namespace Enterprise.Workflow.Business
{
	public class ProcessCompanyLinkRuleWithMultipleCompaniesCollection : ActiveBusinessObjectCollection<ProcessCompanyLinkRule>
	{
		public ProcessCompanyLinkRuleWithMultipleCompaniesCollection(ProcessCompanyLinkRule master) : base(master.Factory, new ProcessCompanyLinkRuleWithMultipleCompaniesCollectionRelationship(master))
		{
			Add(master);
		}
	}

	public class ProcessCompanyLinkRuleWithMultipleCompaniesCollectionRelationship : AdhocCollectionRelationship
	{
		public ProcessCompanyLinkRuleWithMultipleCompaniesCollectionRelationship(ProcessCompanyLinkRule master) : base(typeof(ProcessCompanyLinkRule))
		{
			this.master = master;
		}

		readonly ProcessCompanyLinkRule master;

		protected override void RemoveFromRelationship(BusinessObject businessObject)
		{
			master.HasChanges = true;
			base.RemoveFromRelationship(businessObject);
		}

		protected override void AddToRelationship(BusinessObject businessObject)
		{
			if (businessObject != master)
			{
				master.HasChanges = true;
			}
			base.AddToRelationship(businessObject);
		}
	}
}
