using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.Organisation)]
	public class ConsigneeOrForwarderCollection : OrganisationsFindBoxCollection
	{
		public ConsigneeOrForwarderCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ConsigneeOrForwarderCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public ConsigneeOrForwarderCollection(BusinessObjectFactory factory, OrganisationDefaults defaults)
			: base(factory, defaults)
		{
		}

		public ConsigneeOrForwarderCollection(BusinessObjectFactory factory, ZQuery filter, OrganisationDefaults defaults)
			: base(factory, filter, defaults)
		{
		}

		internal static string ConsigneeOrForwarderErrorMessage
		{
			get { return Res.GetString("55b1284a-d165-4c1f-8373-ac20d196a9a1", "An Organization selected from here must have an Organization type of either Consignee or Forwarder/Agent selected."); }
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			ZQuery result = base.CreateAdditionalFilter();
			if (!AllowOtherOrgTypes)
			{
				ZQuery query = new ZQuery(OrgHeaderSchema.OH_IsConsignee, true);
				query.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsForwarder, true);
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
			base.SetFilterBusinessObjectDefaults();
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property2", ZBool.True));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property5", ZBool.True));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "AndJoinCondition", ZBool.False));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "OrJoinCondition", ZBool.True));
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
			OrgHeader org = (OrgHeader)selectedBusinessObject;
			if (!(org.OH_IsConsignee || org.OH_IsForwarder) && !AllowOtherOrgTypes)
			{
				errors.Add(ConsigneeOrForwarderErrorMessage);
			}
		}

		#region IValidateForController Members

		public override void ValidateEntityOnSaving(IBusiness entity)
		{
			base.ValidateEntityOnSaving(entity);
			OrgHeader org = (OrgHeader)entity;
			if (!AllowOtherOrgTypes)
			{
				org.SetAtleastOneOrgTypeAsExpectedAndValidate(org.OH_IsConsigneeInfo, org.OH_IsForwarderInfo);
			}
			else
			{
				org.Validation.ValidateOH_IsConsignee();
				org.Validation.ValidateOH_IsForwarder();
			}
		}

		#endregion
	}
}
