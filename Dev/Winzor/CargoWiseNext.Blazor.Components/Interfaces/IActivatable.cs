using Microsoft.AspNetCore.Components.Web;

namespace CargoWiseNext.Blazor.Components;

public interface IActivatable
{
	void Activate(object activator, MouseEventArgs args);
}
