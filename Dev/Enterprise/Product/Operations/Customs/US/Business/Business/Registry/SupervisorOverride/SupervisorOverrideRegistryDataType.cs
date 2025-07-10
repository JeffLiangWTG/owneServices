using System;
using Enterprise.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.US.DataRegistry.Business
{
	[RegistryEditor("Enterprise.Customs.US.DataRegistry.GUI.SupervisorOverridesRegistryItemEditor, Enterprise.Customs.US.GUI")]
	sealed class SupervisorOverrideRegistryDataType : NonPersistentBusinessObjectRegistryDataType<SupervisorOverrideData>
	{
		protected override void ValidateCore(IRegistryItem registryItem, SupervisorOverrideData proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			IRegistryItemInternals item = registryItem as IRegistryItemInternals;
			if ((item != null) && (item.GetCurrentValueToUse(companyPK, branchPK, departmentPK) != ValueToUse.DefaultValue))
			{
				base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
			}
			else
			{
				proposedValue.ClearAllNotifications();
			}
		}
	}
}
