using System;
using Enterprise.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.US.DataRegistry.Business
{
	[RegistryEditor("Enterprise.Customs.US.DataRegistry.GUI.ACECargoReleaseTypePortMappingItemEditor, Enterprise.Customs.US.GUI")]
	sealed class ACECargoReleaseTypePortRegistryDataType : NonPersistentBusinessObjectRegistryDataType<ACECargoReleaseTypePortMapping>
	{
		protected override void ValidateCore(IRegistryItem registryItem, ACECargoReleaseTypePortMapping proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			var item = registryItem as IRegistryItemInternals;
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
