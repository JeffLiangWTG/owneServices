using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.Organisation)]
	public class ControllingAgentCollection : OrganisationsFindBoxCollection, IValidateForController
	{
		public ControllingAgentCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ControllingAgentCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public ControllingAgentCollection(BusinessObjectFactory factory, OrganisationDefaults defaults)
			: base(factory, defaults)
		{
		}

		public ControllingAgentCollection(BusinessObjectFactory factory, ZQuery filter, OrganisationDefaults defaults)
			: base(factory, filter, defaults)
		{
		}

		public bool ControllingAgentValidationsEnabled
		{
			get { return OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.Value; }
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			if (!ControllingAgentValidationsEnabled)
			{
				return base.CreateAdditionalFilter();
			}

			var query = new ZQuery(OrgHeaderSchema.OH_IsControllingAgent, true);
			query.AddToFilter(base.CreateAdditionalFilter());
			return query;
		}

		protected override void SetFilterBusinessObjectDefaults()
		{
			FilterBusinessObjectDefault filterBODefault = new FilterBusinessObjectDefault("Organisation Types", "Property12", ZBool.True);
			FilterBusinessObjectDefaults.Add(filterBODefault);
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);

			if (ControllingAgentValidationsEnabled && !((OrgHeader)selectedBusinessObject).OH_IsControllingAgent)
			{
				errors.Add(Res.GetString("4b8eb587-5e7e-471b-9244-ddd7f517b5d4", "An Organization selected from here must have an Organization type of Controlling Agent selected."));
			}
		}

		#region IValidateForController Members

		public override void ValidateEntityOnSaving(IBusiness entity)
		{
			base.ValidateEntityOnSaving(entity);

			var organization = (OrgHeader)entity;

			if (ControllingAgentValidationsEnabled)
			{
				organization.SetOrgTypeAsExpectedAndValidate(organization.OH_IsControllingAgentInfo);
			}
			else
			{
				organization.Validation.ValidateOH_IsControllingAgent();
			}
		}

		#endregion
	}
}
