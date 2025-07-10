using System.Windows.Forms;
using Microsoft.AspNetCore.Components.Web;

namespace WinzorFramework;

[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:Static Fields Are Thread Static Rule", Justification = "Read Only Dictionaries")]
internal static class JSKeyMap
{
	static readonly Dictionary<string, Keys> Location0Map = new ()
	{
		{ "\\", Keys.OemPipe },
		{ " ", Keys.Space },
		{ "!", Keys.D1 },
		{ "-", Keys.OemMinus },
		{ "#", Keys.D3 },
		{ "$", Keys.D4 },
		{ "%", Keys.D5 },
		{ "&", Keys.D7 },
		{ "(", Keys.D9 },
		{ ")", Keys.D0 },
		{ "*", Keys.D8 },
		{ ",", Keys.Oemcomma },
		{ ".", Keys.OemPeriod },
		{ "/", Keys.OemQuestion },
		{ ":", Keys.OemSemicolon },
		{ ";", Keys.OemSemicolon },
		{ "?", Keys.OemQuestion },
		{ "@", Keys.D2 },
		{ "[", Keys.OemOpenBrackets },
		{ "\"", Keys.OemQuotes },
		{ "\'", Keys.OemQuotes },
		{ "\x00", Keys.Delete },
		{ "]", Keys.OemCloseBrackets },
		{ "^", Keys.D6 },
		{ "_", Keys.OemMinus },
		{ "`", Keys.Oemtilde },
		{ "{", Keys.OemOpenBrackets },
		{ "|", Keys.OemPipe },
		{ "}", Keys.OemCloseBrackets },
		{ "~", Keys.Oemtilde },
		{ "+", Keys.Oemplus },
		{ "<", Keys.Oemcomma },
		{ "=", Keys.Oemplus },
		{ ">", Keys.OemPeriod },
		{ "Alt", Keys.Menu },
		{ "AltGraph", Keys.Control | Keys.Alt },
		{ "ArrowDown", Keys.Down },
		{ "ArrowLeft", Keys.Left },
		{ "ArrowRight", Keys.Right },
		{ "ArrowUp", Keys.Up },
		{ "Backspace", Keys.Back },
		{ "CapsLock", Keys.CapsLock },
		{ "Clear", Keys.Clear },
		{ "ContextMenu", Keys.Apps },
		{ "Control", Keys.ControlKey },
		{ "Delete", Keys.Delete },
		{ "End", Keys.End },
		{ "Enter", Keys.Enter },
		{ "Escape", Keys.Escape },
		{ "Home", Keys.Home },
		{ "Insert", Keys.Insert },
		{ "Meta", Keys.LWin },
		{ "NumLock", Keys.NumLock },
		{ "PageDown", Keys.PageDown },
		{ "PageUp", Keys.PageUp },
		{ "Pause", Keys.Pause },
		{ "PrintScreen", Keys.PrintScreen },
		{ "ScrollLock", Keys.Scroll },
		{ "Shift", Keys.ShiftKey },
		{ "Tab", Keys.Tab },
		{ "AudioVolumeMute", Keys.VolumeMute },
		{ "AudioVolumeDown", Keys.VolumeDown },
		{ "AudioVolumeUp", Keys.VolumeUp },
		{ "LaunchMediaPlayer", Keys.SelectMedia },
		{ "LaunchApplication1", Keys.LaunchApplication1 },
		{ "LaunchApplication2", Keys.LaunchApplication2 },
		{ "MediaPlayPause", Keys.MediaPlayPause },
		{ "Process", Keys.ProcessKey },
		{ "MediaNextTrack", Keys.MediaNextTrack },
		{ "MediaTrackNext", Keys.MediaNextTrack },
		{ "MediaPreviousTrack", Keys.MediaPreviousTrack },
		{ "MediaTrackPrevious", Keys.MediaPreviousTrack },
		{ "MediaStop", Keys.MediaStop },
		{ "Unidentified", Keys.None },
		{ "Return", Keys.Enter },
		{ "HangulMode", Keys.HangulMode },
		{ "Dead", Keys.None },
		{ "KanaMode", Keys.KanaMode },
		{ "JunjaMode", Keys.JunjaMode },
		{ "FinalMode", Keys.FinalMode },
		{ "HanjaMode", Keys.HanjaMode },
		{ "KanjiMode", Keys.KanjiMode },
		{ "HanguelMode", Keys.HanguelMode },
		{ "BrowserBack", Keys.BrowserBack },
		{ "BrowserForward", Keys.BrowserForward },
		{ "BrowserRefresh", Keys.BrowserRefresh },
		{ "BrowserStop", Keys.BrowserStop },
		{ "BrowserSearch", Keys.BrowserSearch },
		{ "BrowserFavorites", Keys.BrowserFavorites },
		{ "BrowserHome", Keys.BrowserHome },
	};

	static readonly Dictionary<string, Keys> Location1Map = new ()
	{
		{ "Alt", Keys.LMenu },
		{ "Control", Keys.LControlKey },
		{ "Meta", Keys.LWin },
		{ "Shift", Keys.LShiftKey },
	};

	static readonly Dictionary<string, Keys> Location2Map = new ()
	{
		{ "Alt", Keys.RMenu },
		{ "Control", Keys.RControlKey },
		{ "Meta", Keys.RWin },
		{ "Shift", Keys.RShiftKey },
	};

	static readonly Dictionary<string, Keys> Location3Map = new ()
	{
		{ "-", Keys.Subtract },
		{ "*", Keys.Multiply },
		{ ".", Keys.Decimal },
		{ "/", Keys.Divide },
		{ "+", Keys.Add },
		{ "ArrowDown", Keys.Down },
		{ "ArrowLeft", Keys.Left },
		{ "ArrowRight", Keys.Right },
		{ "ArrowUp", Keys.Up },
		{ "Clear", Keys.Clear },
		{ "End", Keys.End },
		{ "Enter", Keys.Enter },
		{ "Home", Keys.Home },
		{ "Insert", Keys.Insert },
		{ "PageDown", Keys.PageDown },
		{ "PageUp", Keys.PageUp },
		{ "\x00", Keys.Delete },
		{ "Delete", Keys.Delete },
		{ "Unidentified", Keys.None },
		{ "AltGraph", Keys.Control | Keys.Alt },
		{ "Process", Keys.ProcessKey },
	};

	internal static Keys Get(KeyboardEventArgs args)
	{
		var key = args.Key;

		if (key.Length == 1)
		{
			var c = key.First();
			if (char.IsAsciiLetter(c))
			{
				return (Keys)char.ToUpper(c);
			}
			if (char.IsAsciiDigit(c))
			{
				var isNumpad = args.Location == 3;
				return Enum.Parse<Keys>(isNumpad ? $"Numpad{c}" : $"D{c}", true);
			}
		}
		if (key.StartsWith('F') && int.TryParse(key.Substring(1), out var value))
		{
			if (value < 1 || value > 24)
			{
				return Keys.None;
			}
			return Keys.F1 + --value;
		}
		var map = args.Location switch
		{
			1 => Location1Map, // Left Keys
			2 => Location2Map, // Right Keys
			3 => Location3Map, // Numpad Keys
			_ => Location0Map, // Default
		};

		if (!map.TryGetValue(key, out var keyValue))
		{
			if (Enum.TryParse<JSKeyCodes>(args.Code, out var keyVal))
			{
				return (Keys)keyVal;
			}

			if (!string.IsNullOrEmpty(args.Code))
			{
				Application.ReportDeveloperException($"JSKeyMap Failed to parse key: {key} at Location: {args.Location}, Code {args.Code} , ShiftKey: {args.ShiftKey}, CtrlKey: {args.CtrlKey}, AltKey: {args.AltKey}, MetaKey: {args.MetaKey}");
			}
			return Keys.None;
		}

		return keyValue;
	}

	internal static string GetKeyString(Keys key)
	{
		return Location0Map.FirstOrDefault(x => x.Value == key).Key
			?? Location1Map.FirstOrDefault(x => x.Value == key).Key
			?? Location2Map.FirstOrDefault(x => x.Value == key).Key
			?? Location3Map.FirstOrDefault(x => x.Value == key).Key
			?? key.ToString();
	}
}
