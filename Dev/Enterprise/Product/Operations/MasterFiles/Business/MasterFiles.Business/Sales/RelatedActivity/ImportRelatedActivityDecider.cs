namespace Enterprise.MasterFiles.Business
{
	public interface IImportRelatedActivityDecider
	{
		string DecideReason { get; set; }
	}

	public abstract class ImportRelatedActivityDecider : IImportRelatedActivityDecider
	{
		public string DecideReason { get; set; }
	}
}
