using System;
using CargoWise.EntityFramework;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.Business
{
	public class OpportunitySalesValueAnalysisCollection : NonPersistentBusinessObjectCollection<OpportunitySalesValueAnalysis>
	{
		public OpportunitySalesValueAnalysisCollection(OrgOpportunity opportunity)
			: base(opportunity.Factory)
		{
			this.opportunity = opportunity;
		}

		public OrgOpportunity Opportunity
		{
			get { return opportunity; }
		}
		readonly OrgOpportunity opportunity;

		public OrgHeader Org
		{
			get { return Factory.Load<OrgHeader>(opportunity.P8_OH); }
		}

		public void Refresh(bool reloadChildCollections = false)
		{
			using (SuspendListChanged())
			{
				RemoveAll();

				if (reloadChildCollections)
				{
					((SalesHeaderCollection)SalesHeaderCollection).Refresh();
				}

				foreach (SalesHeader salesHeader in SalesHeaderCollection)
				{
					Add(new OpportunitySalesValueAnalysis(salesHeader));
				}

				foreach (OrgOpportunityValue valueItem in opportunity.ValueItems)
				{
					Add(new OpportunitySalesValueAnalysis(valueItem));
				}
			}
		}

		ISalesHeaderCollection SalesHeaderCollection
		{
			get { return opportunity.ProspectiveSalesHeaderCollection; }
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException("Only allowed to add OpportunitySalesValueAnalysis with a SalesHeader or OrgOpportunityValue");
		}
	}
}
