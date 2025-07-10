using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.Opportunity)]
	public class OrgOpportunityDependentCollection : DependentBusinessObjectCollection<OrgOpportunity, OrgHeader>
	{
		public OrgOpportunityDependentCollection(OrgHeader organisation)
			: base(organisation)
		{
		}

		protected override void OnLoaded()
		{
			foreach (var child in this)
			{
				child.SuspendValidation();
			}

			Master.SalesOpportunities.HasChangesChanged -= SalesOpportunities_HasChangesChanged;
			Master.SalesOpportunities.HasChangesChanged += SalesOpportunities_HasChangesChanged;
		}

		void SalesOpportunities_HasChangesChanged(object sender, EventArgs e)
		{
			var collection = (BusinessObjectCollection)sender;
			foreach (OrgOpportunity orgOpportunity in collection)
			{
				orgOpportunity.IgnoreValidationSuspended = orgOpportunity.HasChanges;
				orgOpportunity.Validation.ValidateAll();
			}
		}
	}
}
