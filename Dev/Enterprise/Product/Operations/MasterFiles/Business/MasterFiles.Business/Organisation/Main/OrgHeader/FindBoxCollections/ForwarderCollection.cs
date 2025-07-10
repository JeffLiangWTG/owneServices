using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.Organisation)]
	public class ForwarderCollection : OrganisationsFindBoxCollection, IValidateForController
	{
		public ForwarderCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public ForwarderCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public ForwarderCollection(BusinessObjectFactory factory, OrganisationDefaults defaults) : base(factory, defaults)
		{
		}

		public ForwarderCollection(BusinessObjectFactory factory, ZQuery filter, OrganisationDefaults defaults) : base(factory, filter, defaults)
		{
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			if (AllowOtherOrgTypes)
			{
				return base.CreateAdditionalFilter();
			}
			else
			{
				ZQuery query = new ZQuery(OrgHeaderSchema.OH_IsForwarder, true);
				query.AddToFilter(base.CreateAdditionalFilter());
				return query;
			}
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			OrgHeader orgHeader = (OrgHeader)child;
			orgHeader.OH_IsForwarder = orgHeader.SecurityProvider.HasNewDetailsOrgTypeFlagFA;
		}

		protected override void SetFilterBusinessObjectDefaults()
		{
			//TestCase in OrganisationFilterBusinessObject
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property5", ZBool.True));
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
			if (!((OrgHeader)selectedBusinessObject).OH_IsForwarder && !AllowOtherOrgTypes)
			{
				errors.Add(Res.GetString("691b1a5b-03fc-4974-861b-ff8b3e1972a2", "An Organization selected from here must have an Organization type of Forwarder selected."));
			}
		}

		#region IValidateForController Members

		public override void ValidateEntityOnSaving(IBusiness entity)
		{
			base.ValidateEntityOnSaving(entity);
			OrgHeader organisation = (OrgHeader)entity;
			if (!AllowOtherOrgTypes)
			{
				organisation.SetOrgTypeAsExpectedAndValidate(organisation.OH_IsForwarderInfo);
			}
			else
			{
				organisation.Validation.ValidateOH_IsForwarder();
			}
		}

		#endregion

	}
}
