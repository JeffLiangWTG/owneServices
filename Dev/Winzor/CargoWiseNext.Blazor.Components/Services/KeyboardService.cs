using Microsoft.AspNetCore.Components.Web;

namespace CargoWiseNext.Blazor.Components.Services
{
	public class KeyboardService
	{
		public event EventHandler<KeyboardEventArgs> OnKeyDown = delegate { };
		public void NotifyKeyDown(object sender, KeyboardEventArgs e) => OnKeyDown(sender, e);
	}
}
