using System.Collections.Generic;
using CargoWise.Tools.DuplicateDetector;
using Enterprise.MasterData.Business;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterData.GUI
{
	public class OrgHeaderDataProvider : IDataProvider<IDeduplicationGlowObject>
	{
		public IDeduplicationGlowObject MasterGlow { get; set; }

		public OrgHeaderDataProvider(DeduplicationOrgHeader masterGlow)
		{
			MasterGlow = masterGlow;
		}

		public IDeduplicationBatcher GetBusinessObjectBatcher(IEnumerable<ScoringResult> previousResults, PatternMatchingResultModel[] resultModels, IEnumerable<IDeduplicationGlowObject> targetGlows)
		{
			return new OrgHeaderDeduplicationBatcher(MasterGlow, targetGlows, previousResults, resultModels);
		}

		public IDeduplicationDataSource GetDeduplicationDataSource(IEnumerable<DeduplicationPresenterModel> presenterModels, IEnumerable<IDeduplicationGlowObject> targetGlows, Dictionary<ScoringResult, PatternMatchingResult> resultDictionary = null)
		{
			return new OrgHeaderDeduplicationDataSource((DeduplicationOrgHeader)MasterGlow, targetGlows, presenterModels, resultDictionary);
		}

		public string GetTitle()
		{
			if (MasterGlow != null && !((DeduplicationOrgHeader)MasterGlow).IsInDatabase)
			{
				return ResString.GetMultilingualString("eaea7313-5e25-4000-8e70-4bc6f76fcb61", "New Organization");
			}

			return ResString.GetMultilingualString("13369ed6-4b3a-405d-8e26-035ee2397f94", "Current Organization");
		}
	}
}

