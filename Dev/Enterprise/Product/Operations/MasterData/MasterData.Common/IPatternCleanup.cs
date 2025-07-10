namespace Enterprise.MasterData.Common
{
	public interface IPatternCleanup
	{
		void DeleteAllPatterns(bool deleteChildren = true);
	}
}
