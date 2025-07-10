using Microsoft.AspNetCore.Components;

namespace WinzorFramework;

public interface IPortalProvider
{
	public void AddOrUpdatePortalContent(Guid portalId, RenderFragment renderFragment);

	public void RemovePortalContent(Guid portalId);

	public IEnumerable<PortalInsance> PortalInstances { get; }

	public event EventHandler? PortalChanged;
}

public partial class PortalProvider : ComponentBase, IPortalProvider
{
	readonly Dictionary<Guid, RenderFragment> portals = new Dictionary<Guid, RenderFragment>();

	[Parameter]
	public RenderFragment? ChildContent { get; set; }

	public void AddOrUpdatePortalContent(Guid portalId, RenderFragment renderFragment)
	{
		if (portals.ContainsKey(portalId))
		{
			portals[portalId] = renderFragment;
		}
		else
		{
			portals.Add(portalId, renderFragment);
		}
		PortalChanged?.Invoke(this, EventArgs.Empty);
	}

	public void RemovePortalContent(Guid portalId)
	{
		portals.Remove(portalId);
		PortalChanged?.Invoke(this, EventArgs.Empty);
	}
	
	public IEnumerable<PortalInsance> PortalInstances => portals.Select(p => new PortalInsance(p.Key, p.Value));

	public event EventHandler? PortalChanged;
}

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
public record PortalInsance(Guid Id, RenderFragment Content);
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter

