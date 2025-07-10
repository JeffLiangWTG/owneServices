using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Module
{
	public class OrgAddressWithActiveStatusModuleFilterValidation : ModuleTextFilterValidation
	{
		public OrgAddressWithActiveStatusModuleFilterValidation(OrgAddressWithActiveStatusModuleTextFilter parent)
			: base(parent)
		{
			this.parent = parent;
		}

		readonly OrgAddressWithActiveStatusModuleTextFilter parent;

		public void ValidateAddressActiveStatus()
		{
			ValidateCalculatedProperty(parent.ActiveStatusInfo);
		}

		protected void CheckActiveStatus()
		{
			if (parent.ActiveStatus.IsEmpty && !parent.Property.IsEmpty)
			{
				parent.ActiveStatusInfo.AddError(Res.GetString("7FD14B17-2289-4D6C-9201-F35C5725947C", "Can't filter address without active status"));
			}
			else
			{
				ListValidation.ErrorIfInvalidCode(parent.ActiveStatusInfo, parent.ActiveStatusList);
			}
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateAddressActiveStatus();
		}
	}
}
