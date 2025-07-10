using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using AngleSharp.Dom;
using Bunit;
using WinzorFramework;
using WinzorFramework.Extensions;

namespace WinzorTestFramework;

[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1118:Use Keys.KeyCode and Keys.Modifiers Bitmask", Justification = "<Pending>")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "<Pending>")]
public static class RenderedFragmentExtensions
{
	public static async Task KeyPressAsync(this IRenderedComponent<ControlProxyComponent> rendered, Keys key, IElement target)
	{
		var controlId = target.GetAttribute("data-winzor-control-id");
		var cleanKey = RemoveModifiers(key);
		var eventArgs = new WinzorKeyboardEventArgs()
		{
			Key = JSKeyMap.GetKeyString(cleanKey),
			AltKey = (key & Keys.Alt) == Keys.Alt,
			CtrlKey = (key & Keys.Control) == Keys.Control,
			ShiftKey = (key & Keys.Shift) == Keys.Shift,
		};

		var form = rendered.GetForm();

		eventArgs.Type = "keydown";
		await form.OnFormKeyEventAsync(eventArgs, controlId);

		eventArgs.Type = "keyup";
		await form.OnFormKeyEventAsync(eventArgs, controlId);
	}

	public static Form GetForm(this IRenderedComponent<ControlProxyComponent> rendered)
	{
		return rendered.Instance.Control as Form;
	}

	public static T GetControl<T>(this IRenderedComponent<ControlProxyComponent> rendered) where T : Control
	{
		return rendered.Instance.Control.Controls.OfType<T>().Single();
	}

	static Keys RemoveModifiers(Keys key)
	{
		var keyResult = key;

		keyResult = ((keyResult & Keys.Control) == Keys.Control) ? keyResult ^ Keys.Control : keyResult;
		keyResult = ((keyResult & Keys.Alt) == Keys.Alt) ? keyResult ^ Keys.Alt : keyResult;
		keyResult = ((keyResult & Keys.Shift) == Keys.Shift) ? keyResult ^ Keys.Shift : keyResult;

		return keyResult;
	}
}
