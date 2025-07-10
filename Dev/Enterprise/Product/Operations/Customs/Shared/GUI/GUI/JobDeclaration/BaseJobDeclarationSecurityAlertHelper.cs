using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.GUI
{
	public class BaseJobDeclarationSecurityAlertHelper
	{
		public BaseJobDeclarationSecurityAlertHelper(BaseJobDeclaration declaration)
		{
			this.declaration = declaration;
		}

		public ContinueWithSave ShowSecurityAlertMessage()
		{
			var result = ContinueWithSave.Yes;

			if (declaration.IsExport && !Env.Security.ExportEdit.IsAllowed)
			{
				Globals.Message.ShowError(Res.GetString("b144c709-33eb-48fb-a226-ef2ca87089bc", "You do not have security rights to save an {0} Customs job. {1}", declaration.MessageTypeDescription, Env.Security.ExportEdit.DisplayTextPathToSecurityRight));
				result = ContinueWithSave.No;
			}
			else if (!declaration.IsExport && !Env.Security.ImportEdit.IsAllowed)
			{
				Globals.Message.ShowError(Res.GetString("b144c709-33eb-48fb-a226-ef2ca87089bc", "You do not have security rights to save an {0} Customs job. {1}", declaration.MessageTypeDescription, Env.Security.ImportEdit.DisplayTextPathToSecurityRight));
				result = ContinueWithSave.No;
			}
			else if (!declaration.CurrentUserHasBondedWarehouseSecurityAccess && (declaration.IsInwardBondedWarehousingEnabled || declaration.IsOutwardBondedWarehousingEnabled))
			{
				Globals.Message.ShowError(Res.GetString("EEDEF9C1-6125-471B-8590-C1F409C9C15B", "You do not have security rights to save a Bonded Warehousing Customs job. {0}", Env.Security.ImportEditBondedWarehouse.DisplayTextPathToSecurityRight));
				result = ContinueWithSave.No;
			}
			return result;
		}

		readonly BaseJobDeclaration declaration;
	}
}
