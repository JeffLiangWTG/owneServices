namespace Enterprise.MasterFiles.Business
{
	[ImportRelatedActivityPromptUserDecider("Enterprise.MasterFiles.GUI.ImportRelatedActivityPromptUserYesNoDecider, Enterprise.MasterFiles.GUI")]
	public interface IImportRelatedActivityYesNoDecider : IImportRelatedActivityDecider
	{
		bool GetDecision(string question);
	}
}
