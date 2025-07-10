using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.Organisation)]
	public class ConsigneeCollection : OrganisationsFindBoxCollection
	{
		public ConsigneeCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public ConsigneeCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public ConsigneeCollection(BusinessObjectFactory factory, OrganisationDefaults defaults)
			: base(factory, defaults)
		{
		}

		public ConsigneeCollection(BusinessObjectFactory factory, ZQuery filter, OrganisationDefaults defaults)
			: base(factory, filter, defaults)
		{
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			ZQuery result = base.CreateAdditionalFilter();
			if (!AllowOtherOrgTypes)
			{
				ZQuery query = new ZQuery(OrgHeaderSchema.OH_IsConsignee, true);
				result.AddToFilter(query);
			}
			return result;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			OrgHeader orgHeader = (OrgHeader)child;
			orgHeader.OH_IsConsignee = orgHeader.SecurityProvider.HasNewDetailsOrgTypeFlagCon;
		}

		protected override void SetFilterBusinessObjectDefaults()
		{
			//TestCase in OrganisationFilterBusinessObject 
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property2", ZBool.True));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property2", ZBool.True, SearchType.Index));
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
			if (!((OrgHeader)selectedBusinessObject).OH_IsConsignee && !AllowOtherOrgTypes)
			{
				errors.Add(Res.GetString("73b5550e-5bc9-4c66-b7e5-2b9bae121ebc", "An Organization selected from here must have an Organization type of Consignee selected."));
			}
		}

		#region IValidateForController Members

		public override void ValidateEntityOnSaving(IBusiness entity)
		{
			base.ValidateEntityOnSaving(entity);
			OrgHeader organisation = (OrgHeader)entity;
			if (!AllowOtherOrgTypes)
			{
				organisation.SetOrgTypeAsExpectedAndValidate(organisation.OH_IsConsigneeInfo);
			}
			else
			{
				organisation.Validation.ValidateOH_IsConsignee();
			}
		}

		#endregion

	}
}
