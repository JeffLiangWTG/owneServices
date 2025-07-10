using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace CargoWiseNext.Blazor.Components;
#nullable enable
/// <summary>
/// Represents a base class for designing CW-Next components.
/// </summary>
public abstract class CwnComponentBase : ComponentBase
{
	[Inject]
	ILoggerFactory LoggerFactory { get; set; } = null!;
	ILogger? _logger;
	protected ILogger Logger => _logger ??= LoggerFactory.CreateLogger(GetType());

	[Parameter]
	public virtual string? Class { get; set; }

	[Parameter]
	public string? Style { get; set; }

	[Parameter]
	public object? Tag { get; set; }

	[Parameter(CaptureUnmatchedValues = true)]
	public Dictionary<string, object?> UserAttributes { get; set; } = new Dictionary<string, object?>();

	protected bool IsJSRuntimeAvailable { get; set; }

	readonly string _id = $"wtg-{Guid.NewGuid().ToString("N")[..10]}";

	public string FieldId => UserAttributes.TryGetValue($"id", out var id) && id is not null
		? id.ToString() ?? _id
		: _id;

	protected override void OnAfterRender(bool firstRender)
	{
		IsJSRuntimeAvailable = true;
		base.OnAfterRender(firstRender);
	}
}
