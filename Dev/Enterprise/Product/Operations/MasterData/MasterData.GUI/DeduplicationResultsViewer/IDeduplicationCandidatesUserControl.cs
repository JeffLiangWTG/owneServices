using Enterprise.MasterData.Business;

namespace Enterprise.MasterData.GUI
{
	public interface IDeduplicationCandidatesUserControl
	{
		void SetupDataContext(IDeduplicationResultDetail resultDetail);
	}
}
