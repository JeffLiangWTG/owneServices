using CargoWise.EntityFramework;

namespace Enterprise.MasterData.Common
{
	public interface IPatternMatchingRegenerator<TBizo> where TBizo : IDeduplicatable
	{
		int InitializeDataCounter(TBizo bizo, BusinessObjectFactory factory);
		int RegeneratePatterns(TBizo bizo, BusinessObjectFactory factory);
	}
}
