using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class ConsigneeOrConsignorCollection : OrganisationsFindBoxCollection, IValidateForController
	{
		public ConsigneeOrConsignorCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public ConsigneeOrConsignorCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public ConsigneeOrConsignorCollection(BusinessObjectFactory factory, OrganisationDefaults defaults) : base(factory, defaults)
		{
		}

		public ConsigneeOrConsignorCollection(BusinessObjectFactory factory, ZQuery filter, OrganisationDefaults defaults) : base(factory, filter, defaults)
		{
		}

		internal static string ConsigneeOrConsignorErrorMessage => Res.GetString("28ddf77c-0d93-436d-8813-2eb1d69a8505", "An Organization selected from here must have an Organization type of either Consignee or Consignor selected.");

		protected override ZQuery CreateAdditionalFilter()
		{
			var result = base.CreateAdditionalFilter();
			if (!AllowOtherOrgTypes)
			{
				var query = new ZQuery(OrgHeaderSchema.OH_IsConsignee, true);
				query.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsConsignor, true);
			}
			return result;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			var org = (OrgHeader)child;
			org.OH_IsConsignee = org.SecurityProvider.HasNewDetailsOrgTypeFlagCon;
			org.OH_IsConsignor = org.SecurityProvider.HasNewDetailsOrgTypeFlagCon;
		}

		protected override void SetFilterBusinessObjectDefaults()
		{
			base.SetFilterBusinessObjectDefaults();
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property2", ZBool.True));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property3", ZBool.True));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "AndJoinCondition", ZBool.False));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "OrJoinCondition", ZBool.True));
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX notifications, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(notifications, selectedBusinessObject);

			var org = (OrgHeader)selectedBusinessObject;
			if (!org.OH_IsConsignee && !org.OH_IsConsignor && !AllowOtherOrgTypes)
			{
				notifications.Add(ConsigneeOrConsignorErrorMessage);
			}
		}

		#region IValidateForController Members
		public override void ValidateEntityOnSaving(IBusiness entity)
		{
			base.ValidateEntityOnSaving(entity);

			var org = (OrgHeader)entity;
			if (!AllowOtherOrgTypes)
			{
				org.SetAtleastOneOrgTypeAsExpectedAndValidate(org.OH_IsConsigneeInfo, org.OH_IsConsignorInfo);
			}
			else
			{
				org.Validation.ValidateOH_IsConsignee();
				org.Validation.ValidateOH_IsConsignor();
			}
		}
		#endregion
	}
}
