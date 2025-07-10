using System.Drawing;
using System.Windows.Forms;
using WinzorFramework.Extensions;

namespace WinzorFramework;

[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Referenced in NotificationIcon.razor")]
public partial class NotificationIcon : Control
{
	public string? NotificationType
	{
		get => notificationType;
		set
		{
			UpdateProperty(ref notificationType, value);
		}
	}
	string? notificationType;

	public string? NotificationIconBase64DataUrl
	{
		get => notificationIconBase64DataUrl;
		set
		{
			UpdateProperty(ref notificationIconBase64DataUrl, value);
		}
	}
	string? notificationIconBase64DataUrl;

	public Size IconDimensions { get; set; }

	public Control? NotificationAnchorControl { get; set; }

	public new EventHandler<MouseEventArgs>? OnMouseOver { get; set; }

	public EventHandler<MouseEventArgs>? OnMouseOut { get; set; }

	public EventHandler<MouseEventArgs>? OnClickIcon { get; set; }

	async Task ClickAsync(WebMouseEventArgs args)
	{
		var anchorControl = NotificationAnchorControl;
		if (anchorControl is not null)
		{
			await anchorControl.InvokeWinzorDispatcherAsync(() =>
			{
				OnClickIcon?.Invoke(this, new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
				InvokeOnClick(anchorControl, args);

				if (args.IsMouseInitiated())
				{
					InvokeOnMouseUp(anchorControl, new MouseEventArgs(MouseButtons.Left, 1, (int)args.ClientX, (int)args.ClientY, 0));
				}
			});
		}
		else
		{
			await InvokeWinzorDispatcherAsync(() => OnClickIcon?.Invoke(this, new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0)));
		}
	}

	async Task MouseDownAsync(WebMouseEventArgs args)
	{
		var anchorControl = NotificationAnchorControl;
		if (anchorControl is not null)
		{
			await anchorControl.InvokeWinzorDispatcherAsync(() =>
			{
				if (args.IsMouseInitiated())
				{
					InvokeOnMouseDown(anchorControl, new MouseEventArgs(MouseButtons.Left, 1, (int)args.ClientX, (int)args.ClientY, 0));
				}

				if ((anchorControl as TextBoxBase) != null)
				{
					anchorControl.Focus();
				}
			});
		}
	}

	async Task MouseOverAsync(WebMouseEventArgs args)
	{
		await InvokeWinzorDispatcherAsync(() =>
		{
			OnMouseOver?.Invoke(this, new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
		});
	}

	async Task MouseOutAsync(WebMouseEventArgs args)
	{
		await InvokeWinzorDispatcherAsync(() =>
		{
			OnMouseOut?.Invoke(this, new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
		});
	}

	// NotificationIcon has no width set so we must override to allow it to render.
	protected internal override bool ShouldRender => Visible;

	protected override bool SelectableByTabKey => false;

	protected override void Dispose(bool disposing)
	{
		NotificationAnchorControl = null;
		base.Dispose(disposing);
	}
}
