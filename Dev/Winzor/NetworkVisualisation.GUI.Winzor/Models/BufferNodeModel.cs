#nullable enable
using CargoWise.NetworkVisualisation.GUI.Services.BufferNode;
using CargoWise.NetworkVisualisation.GUI.Services.Utils;
using CargoWise.PAVE.Common.Interfaces;
using Enterprise.BufferManagement.NetworkVisualisation.Business;

namespace CargoWise.NetworkVisualisation.GUI.Models;

/// <summary>
/// Represents a model that links to a <see cref="BufferViewModel"/> and manages the state and presentation of a buffer node.
/// </summary>
public class BufferNodeModel : LinkableNodeModel
{
	IBufferNodeService Service { get; }
	BufferNodeData Data { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="BufferNodeModel"/> class.
	/// </summary>
	/// <param name="service">The service abstraction for the view model.</param>
	public BufferNodeModel(IBufferNodeService service) : base(service, disableDragNewLink: true)
	{
		Service = service;
		Data = Service.GetBufferNodeModelData();
		UpdateHasProgressBar(HasProgressBar);
	}

	/// <summary>
	/// Gets the description of the buffer node.
	/// </summary>
	public string Description => Data.Description;

	/// <summary>
	/// Gets the additional details of the buffer node.
	/// </summary>
	public string AdditionalDetail => Data.AdditionalDetail;

	/// <summary>
	/// Gets the status of the buffer node.
	/// </summary>
	public WorkStatus Status => Data.Status;

	/// <summary>
	/// Gets the status tooltip of the buffer node.
	/// </summary>
	public string StatusTooltip => Data.StatusTooltip;

	/// <summary>
	/// Gets the penetration percent label.
	/// </summary>
	public string PenetrationPercentLabel => Data.PenetrationPercentLabel;

	/// <summary>
	/// Gets the progress bar model.
	/// </summary>
	public ProgressBarModel? ProgressBarModel { get; private set; }

	/// <summary>
	/// Disposes the resources used by the <see cref="BufferNodeModel"/> class.
	/// </summary>
	public override void Dispose()
	{
		ProgressBarModel?.Dispose();
		ProgressBarModel = null;
		base.Dispose();
	}

	protected override void InitializePropertiesUpdater(PropertiesUpdater updater) => base.InitializePropertiesUpdater(
	updater
		.Add(nameof(BufferNodeData.Description),      (v) => Properties.TryUpdate((string)v, Data.Description, (value) => Data.Description = value))
		.Add(nameof(BufferNodeData.AdditionalDetail), (v) => Properties.TryUpdate((string)v, Data.AdditionalDetail, (value) => Data.AdditionalDetail = value))
		.Add(nameof(BufferNodeData.StatusTooltip),    (v) => Properties.TryUpdate((string)v, Data.StatusTooltip, (value) => Data.StatusTooltip = value))
		.Add(nameof(BufferNodeData.Status),           (v) => Properties.TryUpdate((WorkStatus)v, Data.Status, (value) => Data.Status = value))
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
