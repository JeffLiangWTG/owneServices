using System.Linq;
using System.Threading.Tasks;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.GUI.Extensions;
using CargoWise.NetworkVisualisation.GUI.Services.LinkableNode;

namespace CargoWise.NetworkVisualisation.GUI.Services.JobNode;

public class JobNodeService : LinkableNodeService, IJobNodeService
{
	public JobNodeService(NodeViewModel viewModel, IWinzorDispatch winzorDispatcher) : base(viewModel, winzorDispatcher)
	{
	}

	#region IJobNodeViewModel

	public JobNodeData GetJobNodeData()
	{
		winzorDispatcher.AssertInWinzorThread();

		return new JobNodeData
		{
			JobNumber = viewModel.JobNumber,
			JobNumberReadableText = viewModel.JobNumberReadableText,
			JobName = viewModel.JobName,
			AppliedAttributesReadableText = viewModel.AppliedAttributesReadableText,
			DurationReadableText = viewModel.DurationReadableText,
			CompletionCriteria = viewModel.CompletionCriteria,
			CompletionCriteriaPlaceholder = viewModel.CompletionCriteriaPlaceholder,
			CompletionCriteriaTextColor = viewModel.CompletionCriteriaTextColor,
			Description = viewModel.Description,
			ShowScheduleDetails = viewModel.ShowScheduleDetails,
			ShowJobBar = viewModel.ShowJobBar,
			ShowJobBarAffinities = viewModel.ShowJobBarAffinities,
			HasLinkedEntity = viewModel.HasLinkedEntity,
			StartDateReadableText = viewModel.StartDateReadableText,
			RemainingDurationReadableText = viewModel.RemainingDurationReadableText,
			FinishDateReadableText = viewModel.FinishDateReadableText,
			Status = viewModel.Status,
			DurationTooltip = viewModel.DurationTooltip,
			StartDateToolTip = viewModel.StartDateToolTip,
			RemainingDurationToolTip = viewModel.RemainingDurationToolTip,
			FinishDateToolTip = viewModel.FinishDateToolTip,
			StatusTooltip = viewModel.StatusTooltip,
			JobNumberTooltip = viewModel.JobNumberTooltip,
			AppliedAttributesTooltip = viewModel.AppliedAttributesTooltip,
			HasPostReqLinks = viewModel.Entity.PostRequisiteLinks.Any(),
			ChildNodesPK = viewModel.ChildNodes.Select(n => n.Entity.EntityPK).ToArray(),
		};
	}

	public async Task UpdateJobNameAsync(string value)
	{
		await winzorDispatcher.InvokeWinzorDispatcherAsync(() => viewModel.JobName = value);
	}

	public async Task UpdateCompletionCriteriaAsync(string value)
	{
		await winzorDispatcher.InvokeWinzorDispatcherAsync(() => viewModel.CompletionCriteria = value);
	}

	#endregion

}
