using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public abstract class PreSaveDialogStrategy
	{
		protected abstract bool ShouldRunPreSaveAction();

		protected abstract ContinueWithSave RunPreSaveAction();

		public virtual ContinueWithSave ShowPreSaveDialogs(ContinueWithSave baseShowPreSaveDialogsResult)
		{
			ContinueWithSave result = baseShowPreSaveDialogsResult;
			if (result == ContinueWithSave.Yes && ShouldRunPreSaveAction())
			{
				result = RunPreSaveAction();
			}
			return result;
		}
	}

	public class MergePreSaveDialogStrategy : PreSaveDialogStrategy
	{
		public MergePreSaveDialogStrategy(BaseJobDeclaration declaration)
		{
			this.declaration = declaration;
		}
		readonly BaseJobDeclaration declaration;

		protected override bool ShouldRunPreSaveAction()
		{
			return declaration != null &&
			declaration.MergeManager.RequiresMergeBeforeSave;
		}

		protected override ContinueWithSave RunPreSaveAction()
		{
			var result = ContinueWithSave.No;
			if (!declaration.DoMerge())
			{
				result = ContinueWithSave.No;
			}
			else if (declaration.HasErrors)
			{
				result = ContinueWithSave.No;
				if (!Globals.IsTest)
				{
					using (var form = new ZErrorMessageBox(declaration))
					{
						ZFormModaliser.ShowMessageBoxWithoutDispose(form);
					}
				}
				else
				{
					Globals.Message.ShowError(Res.GetString("92e1afb2-148e-4ad4-a3d7-ec678ef39089", "There are errors - can't save."), Res.GetString("7b8949c3-f7c6-4913-9c04-9589cf3f491f", "Errors!"));
				}
			}
			else
			{
				result = ContinueWithSave.Yes;
			}
			return result;
		}
	}
}
