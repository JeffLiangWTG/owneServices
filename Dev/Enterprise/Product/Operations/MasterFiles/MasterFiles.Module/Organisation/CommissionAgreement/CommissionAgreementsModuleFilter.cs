using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module.Organisation.CommissionAgreement
{
	//acts similarly to TasksModuleFilter or ExceptionsModuleFilter ("Tasks" and "Exceptions" filter strips for all modules which support Workflow) but applies to commission agreements
	public class CommissionAgreementsModuleFilter : ModuleGuidForeignCollectionFilter
	{
		protected CommissionAgreementsModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		public CommissionAgreementsModuleFilter(ZString description, SchemaGuidColumn primaryKeyColumn, SchemaGuidColumn foreignKeyColumn, IBusinessObjectCollection list, Type parentBusinessObjectType)
			: base(description, ModuleIDs.OrgCommissionAgreement, primaryKeyColumn, foreignKeyColumn, list, parentBusinessObjectType)
		{
		}

		public CommissionAgreementsModuleFilter(ZString description, SchemaGuidColumn primaryKeyColumn, SchemaGuidColumn foreignKeyColumn, GetList listDelegate, Type parentBusinessObjectType)
			: base(description, ModuleIDs.OrgCommissionAgreement, primaryKeyColumn, foreignKeyColumn, listDelegate, parentBusinessObjectType)
		{
		}

		public new ModuleGuidFilterValidation Validation
		{
			get { return (CommissionAgreementsModuleFilterValidation)base.Validation; }
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new CommissionAgreementsModuleFilterValidation(this);
		}
	}

	#region class Validation

	public class CommissionAgreementsModuleFilterValidation : ModuleGuidFilterValidation
	{
		public CommissionAgreementsModuleFilterValidation(CommissionAgreementsModuleFilter parent)
			: base(parent)
		{
		}

		protected override void CheckSelectedFiltersDescription()
		{
			base.CheckSelectedFiltersDescription();

			// Env.Security.CommissionAgreementView relates to Manage / Sales & Marketing / Opportunity Management / Opportunity Commission Agreement / View
			if (!Env.Security.CommissionAgreementView.IsAllowed)
			{
				GetParent().SelectedFiltersDescriptionInfo.AddError(Env.Security.CommissionAgreementView.ErrorMessageForNotAllowed);
			}
		}
	}
	#endregion
}
