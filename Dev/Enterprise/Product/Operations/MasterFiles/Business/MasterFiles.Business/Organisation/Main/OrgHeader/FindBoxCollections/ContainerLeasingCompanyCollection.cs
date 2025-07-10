using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("This Business Object Collection is used in Glow.")]
	public class ContainerLeasingCompanyCollection : OrganisationsFindBoxCollection, IValidateForController
	{
		public ContainerLeasingCompanyCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public ContainerLeasingCompanyCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public ContainerLeasingCompanyCollection(BusinessObjectFactory factory, OrganisationDefaults defaults) : base(factory, defaults)
		{
		}

		public ContainerLeasingCompanyCollection(BusinessObjectFactory factory, ZQuery filter, OrganisationDefaults defaults) : base(factory, filter, defaults)
		{
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var containerLeasingCompany = new ZQuery(OrgHeaderSchema.OH_IsContainerLeasingCompany, true);
			var miscServ = new ZQuery(OrgHeaderSchema.OH_IsMiscFreightServices, true);
			var filter = new ZQuery(miscServ, JoinCondition.And, containerLeasingCompany);

			var query = new ZQuery(filter);
			query.AddToFilter(base.CreateAdditionalFilter());

			return query;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			OrgHeader orgHeader = (OrgHeader)child;
			orgHeader.OH_IsMiscFreightServices = orgHeader.SecurityProvider.HasNewDetailsOrgTypeFlagSV;
			orgHeader.OH_IsContainerLeasingCompany = true;
		}

		protected override void SetFilterBusinessObjectDefaults()
		{
			//TestCase in OrganisationFilterBusinessObject
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Secondary Type", "Property", OrganisationSecondaryTypes.ContainerLeasingCompany));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property9", ZBool.True));
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
			if (((OrgHeader)selectedBusinessObject).OH_IsActive)
			{
				errors.Add(Res.GetString("F57BAD53-67E2-4C30-A312-2B5302F3B04D", "An Organization selected from here must have an Organization type of Services and must also have Container Leasing Company selected."));
			}
		}

		#region IValidateForController Members

		public override void ValidateEntityOnSaving(IBusiness entity)
		{
			base.ValidateEntityOnSaving(entity);
			OrgHeader organisation = (OrgHeader)entity;
			organisation.SetOrgTypeAsExpectedAndValidate(organisation.OH_IsMiscFreightServicesInfo, organisation.OH_IsContainerLeasingCompanyInfo);
		}

		#endregion
	}
}
