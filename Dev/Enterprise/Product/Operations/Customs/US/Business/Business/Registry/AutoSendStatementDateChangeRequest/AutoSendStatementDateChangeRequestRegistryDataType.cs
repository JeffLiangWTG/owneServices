using System;
using Enterprise.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.US.DataRegistry.Business
{
	[RegistryEditor("Enterprise.Customs.US.DataRegistry.GUI.AutoSendStatementDateChangeRequestRegistryItemEditor, Enterprise.Customs.US.GUI")]
	sealed class AutoSendStatementDateChangeRequestRegistryDataType : NonPersistentBusinessObjectRegistryDataType<AutoSendStatementDateChangeRequest>
	{
		protected override void ValidateCore(IRegistryItem registryItem, AutoSendStatementDateChangeRequest proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			proposedValue.ValidateOverrideAllOrByOrganisation();
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
		}
	}
}
