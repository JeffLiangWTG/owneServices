using System.Threading.Tasks;
using CargoWise.NetworkVisualisation.GUI.Services.LinkableNode;
using CargoWise.NetworkVisualisation.GUI.Services.ProgressBarModel;

namespace CargoWise.NetworkVisualisation.GUI.Services.JobNode;

public interface IJobNodeService : ILinkableNodeService
{
	JobNodeData GetJobNodeData();
	Task UpdateJobNameAsync(string value);
	Task UpdateCompletionCriteriaAsync(string value);

	IProgressBarModelService GetProgressBarService();
}
