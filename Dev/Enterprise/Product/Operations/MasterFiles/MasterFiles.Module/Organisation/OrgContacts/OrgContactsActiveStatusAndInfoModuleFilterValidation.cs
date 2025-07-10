using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Module
{
	public class OrgContactsActiveStatusAndInfoModuleFilterValidation : ModuleTextFilterValidation
	{
		public OrgContactsActiveStatusAndInfoModuleFilterValidation(OrgContactsActiveStatusAndInfoModuleFilter parent)
			: base(parent)
		{
			this.parent = parent;
		}

		readonly OrgContactsActiveStatusAndInfoModuleFilter parent;

		public void ValidateContactActiveStatus()
		{
			ValidateCalculatedProperty(parent.ActiveStatusInfo);
		}

		protected void CheckActiveStatus()
		{
			if (parent.ActiveStatus.IsEmpty && !parent.Property.IsEmpty)
			{
				parent.ActiveStatusInfo.AddError(Res.GetString("1ef51134-e6b5-47f4-9cda-e8cdccc5bf64", "Can't filter organization contacts without active status"));
			}
			else
			{
				ListValidation.ErrorIfInvalidCode(parent.ActiveStatusInfo, parent.ActiveStatusList);
			}
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateContactActiveStatus();
		}
	}
}
