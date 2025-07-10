using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.Organisation)]
	public class ConsignorCollection : OrganisationsFindBoxCollection, IValidateForController
	{
		public ConsignorCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public ConsignorCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public ConsignorCollection(BusinessObjectFactory factory, OrganisationDefaults defaults) : base(factory, defaults)
		{
		}

		public ConsignorCollection(BusinessObjectFactory factory, ZQuery filter, OrganisationDefaults defaults) : base(factory, filter, defaults)
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
				ZQuery query = new ZQuery(OrgHeaderSchema.OH_IsConsignor, true);
				query.AddToFilter(base.CreateAdditionalFilter());
				return query;
			}
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			OrgHeader orgHeader = (OrgHeader)child;
			orgHeader.OH_IsConsignor = orgHeader.SecurityProvider.HasNewDetailsOrgTypeFlagSP;
		}

		protected override void SetFilterBusinessObjectDefaults()
		{
			//TestCase in OrganisationFilterBusinessObject
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property3", ZBool.True));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property3", ZBool.True, SearchType.Index));
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
			if (!((OrgHeader)selectedBusinessObject).OH_IsConsignor && !AllowOtherOrgTypes)
			{
				errors.Add(Res.GetString("4236a00d-95e7-493f-9c32-98865fa05f6d", "An Organization selected from here must have an Organization type of Consignor selected."));
			}
		}

		#region IValidateForController Members

		public override void ValidateEntityOnSaving(IBusiness entity)
		{
			base.ValidateEntityOnSaving(entity);
			OrgHeader organisation = (OrgHeader)entity;
			if (!AllowOtherOrgTypes)
			{
				organisation.SetOrgTypeAsExpectedAndValidate(organisation.OH_IsConsignorInfo);
			}
			else
			{
				organisation.Validation.ValidateOH_IsConsignor();
			}
		}

		#endregion

	}
}
