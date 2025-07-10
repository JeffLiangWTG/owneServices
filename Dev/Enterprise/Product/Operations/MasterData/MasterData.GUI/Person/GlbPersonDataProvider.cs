using System.Collections.Generic;
using CargoWise.Tools.DuplicateDetector;
using Enterprise.MasterData.Business;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterData.GUI
{
	public class GlbPersonDataProvider : IDataProvider<IDeduplicationGlowObject>
	{
		public IDeduplicationGlowObject MasterGlow { get; set; }

		public GlbPersonDataProvider(DeduplicationGlbPerson masterGlow)
		{
			MasterGlow = masterGlow;
		}

		public IDeduplicationBatcher GetBusinessObjectBatcher(IEnumerable<ScoringResult> previousResults, PatternMatchingResultModel[] resultModels, IEnumerable<IDeduplicationGlowObject> targetGlows)
		{
			return new GlbPersonDeduplicationBatcher(MasterGlow, targetGlows, previousResults, resultModels);
		}

		public IDeduplicationDataSource GetDeduplicationDataSource(IEnumerable<DeduplicationPresenterModel> presenterModels, IEnumerable<IDeduplicationGlowObject> targetGlows, Dictionary<ScoringResult, PatternMatchingResult> resultDictionary = null)
		{
			return new GlbPersonDeduplicationDataSource((DeduplicationGlbPerson)MasterGlow, targetGlows, presenterModels);
		}

		public string GetTitle()
		{
			if (MasterGlow != null && !((DeduplicationGlbPerson)MasterGlow).IsInDatabase)
			{
				return ResString.GetMultilingualString("acf33127-f296-4712-88a1-9ca8f0d7ca89", "New Person");
			}

			return ResString.GetMultilingualString("3448b31b-2cb8-4f53-94eb-f69a0e189988", "Current Person");
		}
	}
}
