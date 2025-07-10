namespace Enterprise.MasterFiles.Business
{
	public abstract class OrgMergeParameterisedSqlProvider
	{
		public const string OldPkParameter = "@OldPk";

		public const string NewPkParameter = "@NewPk";

		protected abstract string Description { get; }

		protected abstract string SqlText { get; }

		public string FullSqlText => "-- " + Description + System.Environment.NewLine + SqlText;
	}
}
