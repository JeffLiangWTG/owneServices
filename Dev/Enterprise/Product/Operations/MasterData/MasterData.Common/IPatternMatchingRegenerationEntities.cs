using CargoWise.EntityFramework;

namespace Enterprise.MasterData.Common
{
	public interface IPatternMatchingRegenerationEntities<TBizo> where TBizo : BusinessObject, IDeduplicatable, IPatternMatchingRegenerationEntities<TBizo>
	{
		IPatternMatchingRegenerator<TBizo>[] RegenerationEntities(PatternMatchingRecalculator<TBizo> recalculator);
	}
}
