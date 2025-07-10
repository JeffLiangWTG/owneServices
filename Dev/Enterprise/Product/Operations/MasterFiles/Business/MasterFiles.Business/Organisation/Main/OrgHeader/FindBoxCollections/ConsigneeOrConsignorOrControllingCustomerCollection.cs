using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class ConsigneeOrConsignorOrControllingCustomerCollection : OrganisationsFindBoxCollection, IValidateForController
	{
		public ConsigneeOrConsignorOrControllingCustomerCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public ConsigneeOrConsignorOrControllingCustomerCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public ConsigneeOrConsignorOrControllingCustomerCollection(BusinessObjectFactory factory, OrganisationDefaults defaults) : base(factory, defaults)
		{
		}

		public ConsigneeOrConsignorOrControllingCustomerCollection(BusinessObjectFactory factory, ZQuery filter, OrganisationDefaults defaults) : base(factory, filter, defaults)
		{
		}
		public bool ControllingCustomerValidationsEnabled
		{
			get { return OrganisationsDataRegistry.Instance.EnableControllingCustomerFunctionalityAndValidations.Value; }
		}

		internal static string ConsigneeOrConsignorOrControllingCustomerErrorMessage => Res.GetString("443fd040-b787-4d9e-8daa-253666d3f804", "An Organization selected from here must have an Organization type of either Consignee, Consignor or Controlling Customer selected.");

		protected override ZQuery CreateAdditionalFilter()
		{
			var result = base.CreateAdditionalFilter();
			if (!AllowOtherOrgTypes)
			{
				var query = new ZQuery(OrgHeaderSchema.OH_IsConsignee, true);
				query.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsConsignor, true);
				query.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsControllingCustomer, true);
				result.AddToFilter(query);
			}
			return result;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			var org = (OrgHeader)child;
			org.OH_IsConsignee = org.SecurityProvider.HasNewDetailsOrgTypeFlagCon;
			org.OH_IsConsignor = org.SecurityProvider.HasNewDetailsOrgTypeFlagCon;

			if (ControllingCustomerValidationsEnabled)
			{
				org.OH_IsControllingCustomer = org.SecurityProvider.HasNewDetailsOrgTypeFlagCon;
			}
		}

		protected override void SetFilterBusinessObjectDefaults()
		{
			base.SetFilterBusinessObjectDefaults();
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property2", ZBool.True));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property3", ZBool.True));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property13", ZBool.True));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "AndJoinCondition", ZBool.False));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "OrJoinCondition", ZBool.True));
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX notifications, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(notifications, selectedBusinessObject);

			var org = (OrgHeader)selectedBusinessObject;
			if (!org.OH_IsConsignee && !org.OH_IsConsignor && !org.OH_IsControllingCustomer && !AllowOtherOrgTypes)
			{
				notifications.Add(ConsigneeOrConsignorOrControllingCustomerErrorMessage);
			}
		}

		#region IValidateForController Members
		public override void ValidateEntityOnSaving(IBusiness entity)
		{
			base.ValidateEntityOnSaving(entity);

			var org = (OrgHeader)entity;
			if (!AllowOtherOrgTypes)
			{
				if (ControllingCustomerValidationsEnabled)
				{
					org.SetAtleastOneOrgTypeAsExpectedAndValidate(org.OH_IsConsigneeInfo, org.OH_IsConsignorInfo, org.OH_IsControllingCustomerInfo);
				}
				else
				{
					org.SetAtleastOneOrgTypeAsExpectedAndValidate(org.OH_IsConsigneeInfo, org.OH_IsConsignorInfo);
				}
			}
			else
			{
				org.Validation.ValidateOH_IsConsignee();
				org.Validation.ValidateOH_IsConsignor();
				org.Validation.ValidateOH_IsControllingCustomer();
			}
		}
		#endregion
	}
}
