using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.GUI
{
	public class BaseShipmentAndBrokerageCommon
	{
		public BaseShipmentAndBrokerageCommon(BaseJobDeclaration declaration)
		{
			this.declaration = declaration;
		}
		protected BaseJobDeclaration declaration;

		public virtual SupervisorOverrides GetSupervisorOverrides()
		{
			return new SupervisorOverrides(declaration, SupervisorOverridesContext.SavingDeclaration);
		}

		public virtual ContinueWithSave IsSupervisorApproved()
		{
			var supervisorOverrides = GetSupervisorOverrides();
			return SupervisorOverridesHelper.IsSupervisorApproved(supervisorOverrides, declaration.Logs) ? ContinueWithSave.Yes : ContinueWithSave.No;
		}
	}
}
