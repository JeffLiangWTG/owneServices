using CargoWise.EntityFramework;
using Enterprise.Security;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class AccOrgTaxConfigurationTemplateControllerForTest : AccOrgTaxConfigurationTemplateController
	{
		public new SecurityCheckpoint CheckPointForNew => base.CheckPointForNew;

		public new SecurityCheckpoint CheckPointForView => base.CheckPointForView;

		public new SecurityCheckpoint CheckPointForEdit => base.CheckPointForEdit;

		public new SecurityCheckpoint CheckPointForDelete => base.CheckPointForDelete;

		public SecurityCheckpoint CheckPointForCopy(BusinessObject bizo)
		{
			return base.GetCheckPointForCopy(bizo);
		}
	}
}
