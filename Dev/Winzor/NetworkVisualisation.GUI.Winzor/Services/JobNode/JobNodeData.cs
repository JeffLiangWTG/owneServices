using System;
using System.Collections.Generic;
using System.Drawing;
using CargoWise.PAVE.Common.Interfaces;

namespace CargoWise.NetworkVisualisation.GUI.Services.JobNode;

public class JobNodeData
{
	public string JobNumber { get; internal set; }
	public string JobNumberReadableText { get; internal set; }
	public string JobName { get; internal set; }
	public string AppliedAttributesReadableText { get; internal set; }
	public string DurationReadableText { get; internal set; }
	public string CompletionCriteria { get; internal set; }
	public string CompletionCriteriaPlaceholder { get; internal set; }
	public string Description { get; internal set; }
	public string StartDateReadableText { get; internal set; }
	public string RemainingDurationReadableText { get; internal set; }
	public string FinishDateReadableText { get; internal set; }
	public string DurationTooltip { get; internal set; }
	public string StartDateToolTip { get; internal set; }
	public string RemainingDurationToolTip { get; internal set; }
	public string FinishDateToolTip { get; internal set; }
	public string StatusTooltip { get; internal set; }
	public string JobNumberTooltip { get; internal set; }
	public string AppliedAttributesTooltip { get; internal set; }
	public bool ShowJobBar { get; internal set; }
	public bool ShowScheduleDetails { get; internal set; }
	public bool ShowJobBarAffinities { get; internal set; }
	public bool HasLinkedEntity { get; internal set; }
	public bool HasPostReqLinks { get; internal set; }
	public Color CompletionCriteriaTextColor { get; internal set; }
	public WorkStatus Status { get; internal set; }
	public IEnumerable<Guid> ChildNodesPK { get; internal set; }
}
