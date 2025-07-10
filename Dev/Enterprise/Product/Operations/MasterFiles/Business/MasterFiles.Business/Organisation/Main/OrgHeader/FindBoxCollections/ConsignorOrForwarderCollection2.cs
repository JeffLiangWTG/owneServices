using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.Organisation)]
	public class ConsignorOrForwarderCollection : OrganisationsFindBoxCollection
	{
		public ConsignorOrForwarderCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ConsignorOrForwarderCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public ConsignorOrForwarderCollection(BusinessObjectFactory factory, OrganisationDefaults defaults)
			: base(factory, defaults)
		{
		}

		public ConsignorOrForwarderCollection(BusinessObjectFactory factory, ZQuery filter, OrganisationDefaults defaults)
			: base(factory, filter, defaults)
		{
		}

		internal static string ConsignorOrForwarderErrorMessage
		{
			get { return Res.GetString("31cd53a2-a7f1-458d-b0cc-10c72eff3342", "An Organization selected from here must have an Organization type of either Consignor or Forwarder/Agent selected."); }
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			ZQuery result = base.CreateAdditionalFilter();
			if (!AllowOtherOrgTypes)
			{
				ZQuery query = new ZQuery(OrgHeaderSchema.OH_IsConsignor, true);
				query.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsForwarder, true);
				result.AddToFilter(query);
			}
			return result;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			OrgHeader orgHeader = (OrgHeader)child;
			orgHeader.OH_IsConsignor = orgHeader.SecurityProvider.HasNewDetailsOrgTypeFlagSP;
		}

		protected override void SetFilterBusinessObjectDefaults()
		{
			base.SetFilterBusinessObjectDefaults();
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property3", ZBool.True));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property5", ZBool.True));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "AndJoinCondition", ZBool.False));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "OrJoinCondition", ZBool.True));
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
			OrgHeader org = (OrgHeader)selectedBusinessObject;
			if (!(org.OH_IsConsignor || org.OH_IsForwarder) && !AllowOtherOrgTypes)
			{
				errors.Add(ConsignorOrForwarderErrorMessage);
			}
		}

		#region IValidateForController Members

		public override void ValidateEntityOnSaving(IBusiness entity)
		{
			base.ValidateEntityOnSaving(entity);
			OrgHeader org = (OrgHeader)entity;
			if (!AllowOtherOrgTypes)
			{
				org.SetAtleastOneOrgTypeAsExpectedAndValidate(org.OH_IsConsignorInfo, org.OH_IsForwarderInfo);
			}
			else
			{
				org.Validation.ValidateOH_IsConsignor();
				org.Validation.ValidateOH_IsForwarder();
			}
		}

		#endregion
	}
}
