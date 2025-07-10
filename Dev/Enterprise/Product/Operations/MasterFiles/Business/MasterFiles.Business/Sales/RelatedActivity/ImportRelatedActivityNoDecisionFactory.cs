namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// Use this decision factory when you only want to import data that:
	///  1) Does not require any decisions during import of related activity
	///  2) Or decisions explicitly specified (through AddDecider())
	/// </summary>
	public class ImportRelatedActivityNoDecisionFactory : ImportRelatedActivityDeciderFactory
	{
		public override string Reason
		{
			get { return string.Empty; }
		}

		protected override T GetIfAvailableCore<T>()
		{
			return null;
		}
	}
}
