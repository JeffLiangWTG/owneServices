using CargoWise.EntityFramework;
using Enterprise.Customs.GUI;
using Enterprise.Customs.TW.Business;

namespace Enterprise.Customs.TW.GUI
{
	class CalculateDutyStrategy : PreSaveDialogStrategy
	{
		readonly JobDeclaration declaration;

		public CalculateDutyStrategy(JobDeclaration declaration)
		{
			this.declaration = declaration;
		}

		protected override bool ShouldRunPreSaveAction()
		{
			return declaration?.DutyCalculationManagerAnyEntryLineHasChangesSinceLastMark ?? false;
		}

		protected override ContinueWithSave RunPreSaveAction()
		{
			declaration.CalculateDuties();
			return ContinueWithSave.Yes;
		}
	}
}
