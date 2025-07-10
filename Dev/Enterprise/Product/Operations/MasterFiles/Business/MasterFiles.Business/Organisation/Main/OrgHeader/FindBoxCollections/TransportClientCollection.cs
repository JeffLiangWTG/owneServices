using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class TransportClientCollection : OrganisationsFindBoxCollection, IValidateForController
	{
		public TransportClientCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public TransportClientCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public TransportClientCollection(BusinessObjectFactory factory, OrganisationDefaults defaults) : base(factory, defaults)
		{
		}

		public TransportClientCollection(BusinessObjectFactory factory, ZQuery filter, OrganisationDefaults defaults) : base(factory, filter, defaults)
		{
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			ZQuery query = new ZQuery(OrgHeaderSchema.OH_IsTransportClient, true);
			query.AddToFilter(base.CreateAdditionalFilter());
			return query;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			OrgHeader orgHeader = (OrgHeader)child;
			orgHeader.OH_IsTransportClient = orgHeader.SecurityProvider.HasNewDetailsOrgTypeFlagTC;
		}

		protected override void SetFilterBusinessObjectDefaults()
		{
			//TestCase in OrganisationFilterBusinessObject
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property6", ZBool.True));
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
			if (((OrgHeader)selectedBusinessObject).OH_IsActive)
			{
				errors.Add(Res.GetString("db7b1930-46d0-498e-93ca-43a0ea7940cb", "An Organization selected from here must have an Organization type of Transport Client selected."));
			}
		}

		#region IValidateForController Members

		public override void ValidateEntityOnSaving(IBusiness entity)
		{
			base.ValidateEntityOnSaving(entity);
			OrgHeader organisation = (OrgHeader)entity;
			organisation.SetOrgTypeAsExpectedAndValidate(organisation.OH_IsTransportClientInfo);
		}

		#endregion

	}
}
