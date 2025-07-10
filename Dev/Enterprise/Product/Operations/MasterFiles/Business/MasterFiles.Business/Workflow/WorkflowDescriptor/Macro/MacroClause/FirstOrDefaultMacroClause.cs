namespace Enterprise.MasterFiles.Business
{
	internal class FirstOrDefaultMacroClause : FirstMacroClause
	{
		public override string Keyword => "FirstOrDefault";

		public override bool ShouldReturnDefaultValueIfNull()
		{
			return true;
		}
	}
}
