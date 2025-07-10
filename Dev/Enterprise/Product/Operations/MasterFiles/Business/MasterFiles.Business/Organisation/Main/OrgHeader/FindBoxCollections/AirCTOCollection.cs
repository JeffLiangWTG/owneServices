using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AirCTOCollection : OrganisationsFindBoxCollection, IValidateForController
	{
		public AirCTOCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public AirCTOCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public AirCTOCollection(BusinessObjectFactory factory, OrganisationDefaults defaults) : base(factory, defaults)
		{
		}

		public AirCTOCollection(BusinessObjectFactory factory, ZQuery filter, OrganisationDefaults defaults) : base(factory, filter, defaults)
		{
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			ZQuery airCTO = new ZQuery(OrgHeaderSchema.OH_IsAirCTO, true);
			ZQuery miscServ = new ZQuery(OrgHeaderSchema.OH_IsMiscFreightServices, true);
			ZQuery filter = new ZQuery(miscServ, JoinCondition.And, airCTO);

			ZQuery query = new ZQuery(filter);
			query.AddToFilter(base.CreateAdditionalFilter());
			return query;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			OrgHeader orgHeader = (OrgHeader)child;
			orgHeader.OH_IsMiscFreightServices = orgHeader.SecurityProvider.HasNewDetailsOrgTypeFlagSV;
			orgHeader.OH_IsAirCTO = true;
		}

		protected override void SetFilterBusinessObjectDefaults()
		{
			//TestCase in OrganisationFilterBusinessObject
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property9", ZBool.True));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Secondary Type", "Property", OrganisationSecondaryTypes.AirCTO));
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
			if (((OrgHeader)selectedBusinessObject).OH_IsActive)
			{
				errors.Add(Res.GetString("2705ea88-da15-4522-8060-748280003107", "An Organization selected from here must have an Organization type of Services selected."));
			}
		}

		#region IValidateForController Members

		public override void ValidateEntityOnSaving(IBusiness entity)
		{
			base.ValidateEntityOnSaving(entity);
			OrgHeader organisation = (OrgHeader)entity;
			organisation.SetOrgTypeAsExpectedAndValidate(organisation.OH_IsMiscFreightServicesInfo, organisation.OH_IsAirCTOInfo);
		}

		#endregion

	}
}
