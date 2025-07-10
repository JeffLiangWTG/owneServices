using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Organisation.OpportunityManagement.OrgOpportunity;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MarketingManager.Business
{
	public class OpportunityCreationTemplateLookups : ZLookups
	{
		public OpportunityCreationTemplateLookups(OpportunityCreationTemplate parent) : base(parent)
		{
		}

		protected new OpportunityCreationTemplate Parent => (OpportunityCreationTemplate)base.Parent;

		public ReadOnlyCodeDescriptionPairList PackageTypeList
		{
			get
			{
				return Factory.GetCachedValue("OpportunityCreationTemplateLookups.PackageTypeList", () => OrganisationsDataRegistry.Instance.ProductTypeList.Value.GetActiveCodeDescriptionPairList());
			}
		}

		public ReadOnlyCodeDescriptionPairList OpportunityTypeList
		{
			get
			{
				return Factory.GetCachedValue("OpportunityCreationTemplateLookups.OpportunityTypeList", () => OrganisationsDataRegistry.Instance.OpportunitySalesTypes.Value.GetActiveCodeDescriptionPairList());
			}
		}

		public ICodeDescriptionBoolList OpportunityStatusList
		{
			get
			{
				return Factory.GetCachedValue<ICodeDescriptionBoolList>("OpportunityCreationTemplateLookups.OpportunityStatusList", () =>
				{
					var list = new OpportunityStatusCollection();
					list.AddRange(OrganisationsDataRegistry.Instance.OpportunityStatus.Value.OfType<OpportunityStatus>().Where(x => x.Enabled));
					return list;
				});
			}
		}

		public ReadOnlyCodeDescriptionPairList OpportunityStageList
		{
			get
			{
				return Factory.GetCachedValue("OpportunityCreationTemplateLookups.OpportunityStageList", () => OrganisationsDataRegistry.Instance.OpportunityStages.Value.GetActiveCodeDescriptionPairList());
			}
		}

		public ReadOnlyCodeDescriptionPairList ActiveSourcesList
		{
			get
			{
				return Factory.GetCachedValue("OpportunityCreationTemplateLookups.ActiveSources", () => OrganisationsDataRegistry.Instance.OpportunitySource.Value.GetActiveCodeDescriptionPairList());
			}
		}

		public ReadOnlyCodeDescriptionPairList ActiveSourceDetailsList => OrganisationsDataRegistry.Instance.OpportunitySource.Value.GetActiveRelatedItemList(Parent.Source);

		public ReadOnlyCodeDescriptionPairList SourceDetailsList => OrganisationsDataRegistry.Instance.OpportunitySource.Value.GetRelatedItemList(Parent.Source);

		public ReadOnlyCodeDescriptionPairList OpportunityAssignmentList => new OpportunityAssignmentList();

		public GlbStaffCollection SalesPersonList => new GlbStaffCollection(Factory);

		public ReadOnlyCodeDescriptionPairList StaffAssignmentList => ZArchitecture.Environment.DataRegistry.Instance.OrgStaffMemberAssignmentRoles;

		public ReadOnlyCodeDescriptionPairList OverallDispositionList => new OrgOpportunityOverallDispositionList();
	}
}
