#nullable enable
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Blazor.Diagrams.Core.Models.Base;
using CargoWise.Common;
using CargoWise.NetworkVisualisation.GUI.Services.JobNode;
using CargoWise.NetworkVisualisation.GUI.Services.Utils;
using CargoWise.PAVE.Common.Interfaces;

namespace CargoWise.NetworkVisualisation.GUI.Models;

/// <summary>
/// This model contains the properties that are displayed on a job node Blazor component.
/// The values are initialized using a NodeViewModel which contains the business logic. <br/>
/// When the properties on the NodeViewModel change, the corresponding properties on this model are updated.
/// When the user interacts with a job node, the properties are updated on this model first and then this model updates the NodeViewModel.<br/>
/// <remarks>
///	<see cref="JobNodeModel"/> is also used for shapes that are not linked to a job.
/// </remarks>
/// </summary>
public class JobNodeModel : LinkableNodeModel
{
	IJobNodeService Service { get; }
	JobNodeData Data { get; }

	public JobNodeModel(IJobNodeService service) : base(service)
	{
		Service = service;
		Output.Changed += OnPostReqLinksChanged;
		Data = Service.GetJobNodeData();
		UpdateHasProgressBar(HasProgressBar);
	}

	void OnPostReqLinksChanged(Model model)
	{
		if (model is NetworkPortModel portModel)
		{
			HasPostReqLinks = portModel.Links.Any();
			Refresh();
		}
	}

	internal async Task Diagram_JobNameChangedAsync() => await Service.UpdateJobNameAsync(JobName);

	internal async Task Diagram_CompletionCriteriaChangedAsync()
	{
		var adjustedCompletionCriteria = Regex.Replace(CompletionCriteria, "(?<!\r)\n", System.Environment.NewLine);
		await Service.UpdateCompletionCriteriaAsync(adjustedCompletionCriteria);
	}

	/// <summary>
	/// Resets this node's list of child nodes so that only child nodes present on the current diagram are attached to this node model.
	/// Can be used when the collection of nodes on a diagram has changed and this node's child nodes may no longer be present on the diagram.
	/// </summary>
	/// <param name="allNodes">All nodes on the diagram.</param>
	public void ResetChildNodes(IEnumerable<NetworkNodeModel> allNodes)
	{
		childNodes.Clear();

		var dic = allNodes.ToDictionary((n) => n.EntityPK);
		Data.ChildNodesPK
			.Where(pk => dic.ContainsKey(pk))
			.Select(pk => dic[pk])
			.ForEach(n => AddChildNode(n));
	}

	internal List<NetworkNodeModel> GetAllChildNodes()
	{
		var allChildren = new List<NetworkNodeModel>();
		foreach (var child in childNodes)
		{
			allChildren.Add(child);
			if (child is JobNodeModel jobChild)
			{
				allChildren.AddRange(jobChild.GetAllChildNodes());
			}
		}
		return allChildren;
	}

	internal void AddChildNode(NetworkNodeModel child)
	{
		child.ParentNode = this;
		childNodes.Add(child);
	}

	internal void RemoveChildNode(NetworkNodeModel child)
	{
		childNodes.Remove(child);
		child.ParentNode = null;
	}

	readonly List<NetworkNodeModel> childNodes = new List<NetworkNodeModel>();

	/// <summary>
	/// The JobNumber of the linked job.
	/// </summary>
	public string JobNumber => Data.JobNumber;

	public string JobNumberReadableText => Data.JobNumberReadableText;

	/// <summary>
	/// The title of the linked job.
	/// </summary>
	public string JobName
	{
		get => Data.JobName;
		internal set => Data.JobName = value;
	}

	/// <summary>
	/// Concatenated string of the names of the affinities applied to this node.
	/// </summary>
	public string AppliedAttributesReadableText => Data.AppliedAttributesReadableText;

	/// <summary>
	/// Estimated duration of the linked job.
	/// </summary>
	public string DurationReadableText => Data.DurationReadableText;

	/// <summary>
	/// Completion criteria for the linked job, this can be edited by the user by editing the field inside a shape.
	/// </summary>
	public string CompletionCriteria
	{
		get => Data.CompletionCriteria;
		internal set => Data.CompletionCriteria = value;
	}

	/// <summary>
	/// This is a placeholder for the <see cref="CompletionCriteria"/> field that is displayed when <see cref="CompletionCriteria"/> is empty.
	/// </summary>
	public string CompletionCriteriaPlaceholder => Data.CompletionCriteriaPlaceholder;

	/// <summary>
	/// The color that will be used for the Completion Criteria text inside a node.
	/// </summary>
	public Color CompletionCriteriaTextColor => Data.CompletionCriteriaTextColor;

	/// <summary>
	/// The Node's description, this can be modified by the user by writing inside the Notes field in a shape
	/// </summary>
	public string Description => Data.Description;

	/// <summary>
	/// This controls whether we should display start date, remaining duration and finish date.
	/// </summary>
	public bool ShowScheduleDetails => Data.ShowScheduleDetails;

	/// <summary>
	/// Indicates whether to show a top bar in the diagram node.
	/// </summary>
	public bool ShowJobBar => Data.ShowJobBar;

	/// <summary>
	/// This controls whether <see cref="AppliedAttributesReadableText"/> should be displayed.
	/// </summary>
	public bool ShowJobBarAffinities => Data.ShowJobBarAffinities;

	/// <summary>
	/// Indicates if the node had a linked entity or not
	/// </summary>
	public bool HasLinkedEntity => Data.HasLinkedEntity;

	/// <summary>
	/// Returns the text representation for the start date of a job.
	/// </summary>
	public string StartDateReadableText => Data.StartDateReadableText;

	/// <summary>
	/// Returns the text representation of the remaining duration of a job.
	/// </summary>
	public string RemainingDurationReadableText => Data.RemainingDurationReadableText;

	/// <summary>
	/// Returns the text representation of the finish date of a job.
	/// </summary>
	public string FinishDateReadableText => Data.FinishDateReadableText;

	/// <summary>
	/// Returns the current status of a Job, Eg: Working.
	/// </summary>
	public WorkStatus Status => Data.Status;

	/// <summary>
	/// Returns the tooltip that will be displayed when hovering the mouse over the duration field.
	/// </summary>
	public string DurationTooltip => Data.DurationTooltip;

	/// <summary>
	/// Returns the tooltip that will be displayed when hovering the mouse over the start date field.
	/// </summary>
	public string StartDateTooltip => Data.StartDateToolTip;

	/// <summary>
	/// Returns the tooltip that will be displayed when hovering the mouse over the remaining duration field.
	/// </summary>
	public string RemainingDurationTooltip => Data.RemainingDurationToolTip;

	/// <summary>
	/// Returns the tooltip that will be displayed when hovering the mouse over the finish date field.
	/// </summary>
	public string FinishDateTooltip => Data.FinishDateToolTip;

	/// <summary>
	/// Returns the tooltip that will be displayed when hovering the mouse over the status field.
	/// </summary>
	public string StatusTooltip => Data.StatusTooltip;

	/// <summary>
	/// Returns the tooltip that will be displayed when hovering the mouse over the Job Number field.
	/// </summary>
	public string JobNumberTooltip => Data.JobNumberTooltip;

	/// <summary>
	/// The tooltip text that is displayed when hovering the mouse over the Node's affinities section.
	/// </summary>
	public string AppliedAttributesTooltip => Data.AppliedAttributesTooltip;

	/// <summary>
	/// Returns a progress bar that tracks a job's completion percentage.
	/// </summary>
	public ProgressBarModel? ProgressBarModel { get; private set; }

	/// <summary>
	/// Indicates if this node has any other nodes on the diagram linked as post requisites.
	/// This property is useful in determining how to display the right hand port of the node.
	/// </summary>
	public bool HasPostReqLinks
	{
		get => Data.HasPostReqLinks;
		internal set => Data.HasPostReqLinks = value;
	}

	/// <summary>
	/// Overridden dispose also handles removing child nodes in addition to disposing resources and unsubscribing from event handlers.
	/// </summary>
	public override void Dispose()
	{
		ParentNode?.RemoveChildNode(this);
		Output.Changed -= OnPostReqLinksChanged;
		ProgressBarModel?.Dispose();
		ProgressBarModel = null;
		base.Dispose();
	}

	/// <summary>
	/// Gets the Node's name
	/// </summary>
	/// <returns>The string representation of JobNumber that is used as the Node name.</returns>
	public override string GetName()
	{
		return JobNumber;
	}

	protected override void InitializePropertiesUpdater(PropertiesUpdater updater) => base.InitializePropertiesUpdater(
			updater
				.Add(nameof(JobNodeData.HasLinkedEntity),               (v) => Properties.TryUpdate((bool)v, Data.HasLinkedEntity, (value) => Data.HasLinkedEntity = value))
				.Add(nameof(JobNodeData.ShowJobBar),                    (v) => Properties.TryUpdate((bool)v, Data.ShowJobBar, (value) => Data.ShowJobBar = value))
				.Add(nameof(JobNodeData.ShowJobBarAffinities),          (v) => Properties.TryUpdate((bool)v, Data.ShowJobBarAffinities, (value) => Data.ShowJobBarAffinities = value))
				.Add(nameof(JobNodeData.ShowScheduleDetails),           (v) => Properties.TryUpdate((bool)v, Data.ShowScheduleDetails, (value) => Data.ShowScheduleDetails = value))
				.Add(nameof(JobNodeData.AppliedAttributesReadableText), (v) => Properties.TryUpdate((string)v, Data.AppliedAttributesReadableText, (value) => Data.AppliedAttributesReadableText = value))
				.Add(nameof(JobNodeData.AppliedAttributesTooltip),      (v) => Properties.TryUpdate((string)v, Data.AppliedAttributesTooltip, (value) => Data.AppliedAttributesTooltip = value))
				.Add(nameof(JobNodeData.CompletionCriteria),            (v) => Properties.TryUpdate((string)v, Data.CompletionCriteria, (value) => Data.CompletionCriteria = value))
				.Add(nameof(JobNodeData.CompletionCriteriaPlaceholder), (v) => Properties.TryUpdate((string)v, Data.CompletionCriteriaPlaceholder, (value) => Data.CompletionCriteriaPlaceholder = value))
				.Add(nameof(JobNodeData.Description),                   (v) => Properties.TryUpdate((string)v, Data.Description, (value) => Data.Description = value))
				.Add(nameof(JobNodeData.DurationReadableText),          (v) => Properties.TryUpdate((string)v, Data.DurationReadableText, (value) => Data.DurationReadableText = value))
				.Add(nameof(JobNodeData.DurationTooltip),               (v) => Properties.TryUpdate((string)v, Data.DurationTooltip, (value) => Data.DurationTooltip = value))
				.Add(nameof(JobNodeData.FinishDateReadableText),        (v) => Properties.TryUpdate((string)v, Data.FinishDateReadableText, (value) => Data.FinishDateReadableText = value))
				.Add(nameof(JobNodeData.FinishDateToolTip),             (v) => Properties.TryUpdate((string)v, Data.FinishDateToolTip, (value) => Data.FinishDateToolTip = value))
				.Add(nameof(JobNodeData.JobName),                       (v) => Properties.TryUpdate((string)v, Data.JobName, (value) => Data.JobName = value))
				.Add(nameof(JobNodeData.JobNumber),                     (v) => Properties.TryUpdate((string)v, Data.JobNumber, (value) => Data.JobNumber = value))
				.Add(nameof(JobNodeData.JobNumberReadableText),         (v) => Properties.TryUpdate((string)v, Data.JobNumberReadableText, (value) => Data.JobNumberReadableText = value))
				.Add(nameof(JobNodeData.JobNumberTooltip),              (v) => Properties.TryUpdate((string)v, Data.JobNumberTooltip, (value) => Data.JobNumberTooltip = value))
				.Add(nameof(JobNodeData.RemainingDurationReadableText), (v) => Properties.TryUpdate((string)v, Data.RemainingDurationReadableText, (value) => Data.RemainingDurationReadableText = value))
				.Add(nameof(JobNodeData.RemainingDurationToolTip),      (v) => Properties.TryUpdate((string)v, Data.RemainingDurationToolTip, (value) => Data.RemainingDurationToolTip = value))
				.Add(nameof(JobNodeData.StartDateReadableText),         (v) => Properties.TryUpdate((string)v, Data.StartDateReadableText, (value) => Data.StartDateReadableText = value))
				.Add(nameof(JobNodeData.StartDateToolTip),              (v) => Properties.TryUpdate((string)v, Data.StartDateToolTip, (value) => Data.StartDateToolTip = value))
				.Add(nameof(JobNodeData.StatusTooltip),                 (v) => Properties.TryUpdate((string)v, Data.StatusTooltip, (value) => Data.StatusTooltip = value))
				.Add(nameof(JobNodeData.CompletionCriteriaTextColor),   (v) => Properties.TryUpdate((Color)v, Data.CompletionCriteriaTextColor, (value) => Data.CompletionCriteriaTextColor = value))
				.Add(nameof(JobNodeData.Status),                        (v) => Properties.TryUpdate((WorkStatus)v, Data.Status, (value) => Data.Status = value))
		);
	protected override bool UpdateHasProgressBar(bool value)
	{
		ProgressBarModel?.Dispose();
		ProgressBarModel = null;
		if (value)
		{
			ProgressBarModel = new ProgressBarModel(Service.GetProgressBarService());
		}

		return base.UpdateHasProgressBar(value);
	}
}
