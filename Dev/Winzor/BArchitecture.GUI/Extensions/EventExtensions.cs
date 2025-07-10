using System.Windows.Forms;
using System.Windows.Input;
using Microsoft.AspNetCore.Components.Web;

namespace WinzorFramework.Extensions;
public static class EventExtensions
{
	public static MouseButtons GetMouseButtons(this WebMouseEventArgs webMouseEventArgs)
	{
		switch (webMouseEventArgs.Button)
		{
			case 0: return MouseButtons.Left;
			case 1: return MouseButtons.Middle;
			case 2: return MouseButtons.Right;
			case 3: return MouseButtons.XButton1;
			case 4: return MouseButtons.XButton2;
			default: return MouseButtons.None;
		}
	}

	public static Keys GetModifierKeys(this WebMouseEventArgs args) => GetModifierKeys(args.AltKey, args.CtrlKey, args.ShiftKey);

	public static Keys GetModifierKeys(this KeyboardEventArgs args) => GetModifierKeys(args.AltKey, args.CtrlKey, args.ShiftKey);

	internal static Keys GetModifierKeys(bool altValue, bool controlValue, bool shiftValue) => (altValue ? Keys.Alt : Keys.None) | (controlValue ? Keys.Control : Keys.None) | (shiftValue ? Keys.Shift : Keys.None);

	public static Keys GetKey(this KeyboardEventArgs args)
	{
		if (string.IsNullOrEmpty(args.Key))
		{
			return args.GetModifierKeys() switch
			{
				Keys.Alt => Keys.Menu,
				Keys.Control => Keys.ControlKey,
				Keys.Shift => Keys.ShiftKey,
				_ => Keys.None,
			};
		}

		return JSKeyMap.Get(args);
	}

	public static bool IsMouseInitiated(this WebMouseEventArgs args)
	{
		return args is not null && args.ClientX > 0 && args.ClientY > 0;
	}

	public static void GetModifierState(WinzorKeyboardEventArgs args)
	{
		Keyboard.IsCapsLockOn = args.CapsLockKey;
	}

	public static void SetKeyDownStatus(WinzorKeyboardEventArgs args)
	{
		if ((args.Type == "keydown" || args.Type == "keyup") && Enum.TryParse<Key>(args.Key, out var key))
		{
			var status = args.Type == "keydown";
			Keyboard.KeyDownStatus.AddOrUpdate(key, k => status, (k, v) => status);
		}
	}

	public static long GetConsecutiveClickCount(this WebMouseEventArgs args) => args.Detail;
}
public class WinzorKeyboardEventArgs : KeyboardEventArgs
{
	public bool CapsLockKey { get; set; }
}
