using CargoWise.EntityFramework;
using Enterprise.Integration;

namespace Enterprise.MasterFiles.Integration
{
	public interface IUniversalTemplateTrigger : ITemplateTrigger
	{
		bool AreConditionsMet(IBusiness job, IStmALog log);
		void ValidateShouldTriggerOnEstimateEvents();
	}
}
