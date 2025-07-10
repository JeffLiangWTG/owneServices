using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using NUnit.Framework;
using WTG.PlaywrightTesting;

namespace WinzorFramework.Extensions;

using static PlaywrightTestContext;

internal class EventExtensionsTest
{
	[Test]
	public void GetKeyShouldReturnNoneAndNoExceptionWhenPassedInvalidKey()
	{
		var args1 = new WinzorKeyboardEventArgs { Key = "NotAnActualKey" };
		Assert.DoesNotThrow(() => args1.GetKey());
		Assert.That(args1.GetKey(), Is.EqualTo(Keys.None));
	}

	[Test]
	public void GetKeyShouldReturnProcessKeyWhenPassedProcess()
	{
		var argsA = new WinzorKeyboardEventArgs { Key = "Process" };
	}

	[Test]
	public void GetKeyShouldWorkForDifferentLayouts()
	{
		// S Key Event Args when using QUERTY layout
		var quertyArgs = new WinzorKeyboardEventArgs
		{
			Code = "KeyS",
			Key = "s",
			Location = 0,
		};
		Assert.That(quertyArgs.GetKey(), Is.EqualTo(Keys.S));

		// S Key Event Args when using Dvorak layout
		var dvorakArgs = new WinzorKeyboardEventArgs
		{
			Code = "Semicolon",
			Key = "s",
			Location = 0,
		};
		Assert.That(dvorakArgs.GetKey(), Is.EqualTo(Keys.S));
	}

	[TestCase("Alt", Keys.Menu, true, false, false, TestName = "{m}_Alt")]
	[TestCase("Control", Keys.ControlKey, false, true, false, TestName = "{m}_Control")]
	[TestCase("Shift", Keys.ShiftKey, false, false, true, TestName = "{m}_Shift")]
	public void ModifierKeyShouldReturnKeyWhenKeyValueIsEmpty(string keyName, Keys keyValue, bool alt, bool ctrl, bool shift)
	{
		Assert.That((new WinzorKeyboardEventArgs { Key = keyName, }).GetKey(), Is.EqualTo(keyValue));
		Assert.That((new WinzorKeyboardEventArgs { Key = keyName, AltKey = alt, CtrlKey = ctrl, ShiftKey = shift, }).GetKey(), Is.EqualTo(keyValue));
		Assert.That((new WinzorKeyboardEventArgs { AltKey = alt, CtrlKey = ctrl, ShiftKey = shift, }).GetKey(), Is.EqualTo(keyValue));
	}

	[Test]
	public void ModifierKeyShouldNotReturnKeyWhenMultipleModifiersSet()
	{
		Assert.That((new WinzorKeyboardEventArgs { AltKey = true, CtrlKey = true, ShiftKey = true, }).GetKey(), Is.EqualTo(Keys.None));
	}

	[TestCaseSource(nameof(KeyCodeCases_Playwright)), WithPlaywrightPage]
	public async Task GetKeyShouldReturnCorrectKey_Playwright(string key, Keys result)
	{
		await Page.EvaluateAsync("() => document.addEventListener('keydown', e => window.event = e);");
		await Page.Keyboard.PressAsync(key);
		var keyArgs = await Page.EvaluateAsync<WinzorKeyboardEventArgs>(@"() => { return { key: event.key, location: event.location }};");
		Assert.That(keyArgs, Is.Not.Null);
		Assert.That(keyArgs.GetKey(), Is.EqualTo(result));
	}

	[TestCaseSource(nameof(KeyCodeCases_Manual))]
	public void GetKeyShouldReturnCorrectKey_Manual(string key, Keys result)
	{
		var args = new WinzorKeyboardEventArgs { Key = key };
		Assert.That(args.GetKey(), Is.EqualTo(result));
	}

	[TestCaseSource(nameof(KeyCodeCases_Manual_SpecialChars))]
	public void GetKeyShouldFallBackToCodeForSpecialCharsInOtherKeyBoardLayout(string key, string code, Keys result)
	{
		var args = new WinzorKeyboardEventArgs { Key = key, Code = code };
		Assert.That(args.GetKey(), Is.EqualTo(result));
	}

	static IEnumerable<TestCaseData> KeyCodeCases_Playwright
	{
		get
		{
			yield return new TestCaseData("AltGraph", Keys.Control | Keys.Alt) { TestName = "{m}_AltGraph" };
			yield return new TestCaseData("AltLeft", Keys.LMenu) { TestName = "{m}_AltLeft" };
			yield return new TestCaseData("AltRight", Keys.RMenu) { TestName = "{m}_AltRight" };
			yield return new TestCaseData("ArrowDown", Keys.Down) { TestName = "{m}_ArrowDown" };
			yield return new TestCaseData("ArrowLeft", Keys.Left) { TestName = "{m}_ArrowLeft" };
			yield return new TestCaseData("ArrowRight", Keys.Right) { TestName = "{m}_ArrowRight" };
			yield return new TestCaseData("ArrowUp", Keys.Up) { TestName = "{m}_ArrowUp" };
			yield return new TestCaseData("Backquote", Keys.Oemtilde) { TestName = "{m}_Backquote" };
			yield return new TestCaseData("Backslash", Keys.Oem5) { TestName = "{m}_Backslash" };
			yield return new TestCaseData("Backspace", Keys.Back) { TestName = "{m}_Backspace" };
			yield return new TestCaseData("BracketLeft", Keys.OemOpenBrackets) { TestName = "{m}_BracketLeft" };
			yield return new TestCaseData("BracketRight", Keys.OemCloseBrackets) { TestName = "{m}_BracketRight" };
			yield return new TestCaseData("CapsLock", Keys.Capital) { TestName = "{m}_CapsLock" };
			yield return new TestCaseData("Comma", Keys.Oemcomma) { TestName = "{m}_Comma" };
			yield return new TestCaseData("ContextMenu", Keys.Apps) { TestName = "{m}_ContextMenu" };
			yield return new TestCaseData("ControlLeft", Keys.LControlKey) { TestName = "{m}_ControlLeft" };
			yield return new TestCaseData("ControlRight", Keys.RControlKey) { TestName = "{m}_ControlRight" };
			yield return new TestCaseData("Delete", Keys.Delete) { TestName = "{m}_Delete" };
			yield return new TestCaseData("Digit0", Keys.D0) { TestName = "{m}_Digit0" };
			yield return new TestCaseData("Digit1", Keys.D1) { TestName = "{m}_Digit1" };
			yield return new TestCaseData("Digit2", Keys.D2) { TestName = "{m}_Digit2" };
			yield return new TestCaseData("Digit3", Keys.D3) { TestName = "{m}_Digit3" };
			yield return new TestCaseData("Digit4", Keys.D4) { TestName = "{m}_Digit4" };
			yield return new TestCaseData("Digit5", Keys.D5) { TestName = "{m}_Digit5" };
			yield return new TestCaseData("Digit6", Keys.D6) { TestName = "{m}_Digit6" };
			yield return new TestCaseData("Digit7", Keys.D7) { TestName = "{m}_Digit7" };
			yield return new TestCaseData("Digit8", Keys.D8) { TestName = "{m}_Digit8" };
			yield return new TestCaseData("Digit9", Keys.D9) { TestName = "{m}_Digit9" };
			yield return new TestCaseData("End", Keys.End) { TestName = "{m}_End" };
			yield return new TestCaseData("Enter", Keys.Return) { TestName = "{m}_Enter" };
			yield return new TestCaseData("Equal", Keys.Oemplus) { TestName = "{m}_Equal" };
			yield return new TestCaseData("Escape", Keys.Escape) { TestName = "{m}_Escape" };
			yield return new TestCaseData("F1", Keys.F1) { TestName = "{m}_F1" };
			yield return new TestCaseData("F10", Keys.F10) { TestName = "{m}_F10" };
			yield return new TestCaseData("F11", Keys.F11) { TestName = "{m}_F11" };
			yield return new TestCaseData("F12", Keys.F12) { TestName = "{m}_F12" };
			yield return new TestCaseData("F2", Keys.F2) { TestName = "{m}_F2" };
			yield return new TestCaseData("F3", Keys.F3) { TestName = "{m}_F3" };
			yield return new TestCaseData("F4", Keys.F4) { TestName = "{m}_F4" };
			yield return new TestCaseData("F5", Keys.F5) { TestName = "{m}_F5" };
			yield return new TestCaseData("F6", Keys.F6) { TestName = "{m}_F6" };
			yield return new TestCaseData("F7", Keys.F7) { TestName = "{m}_F7" };
			yield return new TestCaseData("F8", Keys.F8) { TestName = "{m}_F8" };
			yield return new TestCaseData("F9", Keys.F9) { TestName = "{m}_F9" };
			yield return new TestCaseData("Home", Keys.Home) { TestName = "{m}_Home" };
			yield return new TestCaseData("Insert", Keys.Insert) { TestName = "{m}_Insert" };
			yield return new TestCaseData("KeyA", Keys.A) { TestName = "{m}_KeyA" };
			yield return new TestCaseData("KeyB", Keys.B) { TestName = "{m}_KeyB" };
			yield return new TestCaseData("KeyC", Keys.C) { TestName = "{m}_KeyC" };
			yield return new TestCaseData("KeyD", Keys.D) { TestName = "{m}_KeyD" };
			yield return new TestCaseData("KeyE", Keys.E) { TestName = "{m}_KeyE" };
			yield return new TestCaseData("KeyF", Keys.F) { TestName = "{m}_KeyF" };
			yield return new TestCaseData("KeyG", Keys.G) { TestName = "{m}_KeyG" };
			yield return new TestCaseData("KeyH", Keys.H) { TestName = "{m}_KeyH" };
			yield return new TestCaseData("KeyI", Keys.I) { TestName = "{m}_KeyI" };
			yield return new TestCaseData("KeyJ", Keys.J) { TestName = "{m}_KeyJ" };
			yield return new TestCaseData("KeyK", Keys.K) { TestName = "{m}_KeyK" };
			yield return new TestCaseData("KeyL", Keys.L) { TestName = "{m}_KeyL" };
			yield return new TestCaseData("KeyM", Keys.M) { TestName = "{m}_KeyM" };
			yield return new TestCaseData("KeyN", Keys.N) { TestName = "{m}_KeyN" };
			yield return new TestCaseData("KeyO", Keys.O) { TestName = "{m}_KeyO" };
			yield return new TestCaseData("KeyP", Keys.P) { TestName = "{m}_KeyP" };
			yield return new TestCaseData("KeyQ", Keys.Q) { TestName = "{m}_KeyQ" };
			yield return new TestCaseData("KeyR", Keys.R) { TestName = "{m}_KeyR" };
			yield return new TestCaseData("KeyS", Keys.S) { TestName = "{m}_KeyS" };
			yield return new TestCaseData("KeyT", Keys.T) { TestName = "{m}_KeyT" };
			yield return new TestCaseData("KeyU", Keys.U) { TestName = "{m}_KeyU" };
			yield return new TestCaseData("KeyV", Keys.V) { TestName = "{m}_KeyV" };
			yield return new TestCaseData("KeyW", Keys.W) { TestName = "{m}_KeyW" };
			yield return new TestCaseData("KeyX", Keys.X) { TestName = "{m}_KeyX" };
			yield return new TestCaseData("KeyY", Keys.Y) { TestName = "{m}_KeyY" };
			yield return new TestCaseData("KeyZ", Keys.Z) { TestName = "{m}_KeyZ" };
			yield return new TestCaseData("MetaLeft", Keys.LWin) { TestName = "{m}_MetaLeft" };
			yield return new TestCaseData("MetaRight", Keys.RWin) { TestName = "{m}_MetaRight" };
			yield return new TestCaseData("Minus", Keys.OemMinus) { TestName = "{m}_Minus" };
			yield return new TestCaseData("NumLock", Keys.NumLock) { TestName = "{m}_NumLock" };
			yield return new TestCaseData("Numpad0", Keys.Insert) { TestName = "{m}_Numpad0" };
			yield return new TestCaseData("Numpad1", Keys.End) { TestName = "{m}_Numpad1" };
			yield return new TestCaseData("Numpad2", Keys.Down) { TestName = "{m}_Numpad2" };
			yield return new TestCaseData("Numpad3", Keys.Next) { TestName = "{m}_Numpad3" };
			yield return new TestCaseData("Numpad4", Keys.Left) { TestName = "{m}_Numpad4" };
			yield return new TestCaseData("Numpad5", Keys.Clear) { TestName = "{m}_Numpad5" };
			yield return new TestCaseData("Numpad6", Keys.Right) { TestName = "{m}_Numpad6" };
			yield return new TestCaseData("Numpad7", Keys.Home) { TestName = "{m}_Numpad7" };
			yield return new TestCaseData("Numpad8", Keys.Up) { TestName = "{m}_Numpad8" };
			yield return new TestCaseData("Numpad9", Keys.PageUp) { TestName = "{m}_Numpad9" };
			yield return new TestCaseData("NumpadAdd", Keys.Add) { TestName = "{m}_NumpadAdd" };
			yield return new TestCaseData("NumpadDecimal", Keys.Delete) { TestName = "{m}_NumpadDecimal" };
			yield return new TestCaseData("NumpadDivide", Keys.Divide) { TestName = "{m}_NumpadDivide" };
			yield return new TestCaseData("NumpadEnter", Keys.Return) { TestName = "{m}_NumpadEnter" };
			yield return new TestCaseData("NumpadMultiply", Keys.Multiply) { TestName = "{m}_NumpadMultiply" };
			yield return new TestCaseData("NumpadSubtract", Keys.Subtract) { TestName = "{m}_NumpadSubtract" };
			yield return new TestCaseData("PageDown", Keys.Next) { TestName = "{m}_PageDown" };
			yield return new TestCaseData("PageUp", Keys.PageUp) { TestName = "{m}_PageUp" };
			yield return new TestCaseData("Pause", Keys.Pause) { TestName = "{m}_Pause" };
			yield return new TestCaseData("Period", Keys.OemPeriod) { TestName = "{m}_Period" };
			yield return new TestCaseData("PrintScreen", Keys.PrintScreen) { TestName = "{m}_PrintScreen" };
			yield return new TestCaseData("Quote", Keys.Oem7) { TestName = "{m}_Quote" };
			yield return new TestCaseData("ScrollLock", Keys.Scroll) { TestName = "{m}_ScrollLock" };
			yield return new TestCaseData("Semicolon", Keys.Oem1) { TestName = "{m}_Semicolon" };
			yield return new TestCaseData("Shift+Backquote", Keys.Oemtilde) { TestName = "{m}_Shift+Backquote" };
			yield return new TestCaseData("Shift+Backslash", Keys.Oem5) { TestName = "{m}_Shift+Backslash" };
			yield return new TestCaseData("Shift+BracketLeft", Keys.OemOpenBrackets) { TestName = "{m}_Shift+BracketLeft" };
			yield return new TestCaseData("Shift+BracketRight", Keys.OemCloseBrackets) { TestName = "{m}_Shift+BracketRight" };
			yield return new TestCaseData("Shift+Comma", Keys.Oemcomma) { TestName = "{m}_Shift+Comma" };
			yield return new TestCaseData("Shift+Digit0", Keys.D0) { TestName = "{m}_Shift+Digit0" };
			yield return new TestCaseData("Shift+Digit1", Keys.D1) { TestName = "{m}_Shift+Digit1" };
			yield return new TestCaseData("Shift+Digit2", Keys.D2) { TestName = "{m}_Shift+Digit2" };
			yield return new TestCaseData("Shift+Digit3", Keys.D3) { TestName = "{m}_Shift+Digit3" };
			yield return new TestCaseData("Shift+Digit4", Keys.D4) { TestName = "{m}_Shift+Digit4" };
			yield return new TestCaseData("Shift+Digit5", Keys.D5) { TestName = "{m}_Shift+Digit5" };
			yield return new TestCaseData("Shift+Digit6", Keys.D6) { TestName = "{m}_Shift+Digit6" };
			yield return new TestCaseData("Shift+Digit7", Keys.D7) { TestName = "{m}_Shift+Digit7" };
			yield return new TestCaseData("Shift+Digit8", Keys.D8) { TestName = "{m}_Shift+Digit8" };
			yield return new TestCaseData("Shift+Digit9", Keys.D9) { TestName = "{m}_Shift+Digit9" };
			yield return new TestCaseData("Shift+Equal", Keys.Oemplus) { TestName = "{m}_Shift+Equal" };
			yield return new TestCaseData("Shift+KeyA", Keys.A) { TestName = "{m}_Shift+KeyA" };
			yield return new TestCaseData("Shift+KeyB", Keys.B) { TestName = "{m}_Shift+KeyB" };
			yield return new TestCaseData("Shift+KeyC", Keys.C) { TestName = "{m}_Shift+KeyC" };
			yield return new TestCaseData("Shift+KeyD", Keys.D) { TestName = "{m}_Shift+KeyD" };
			yield return new TestCaseData("Shift+KeyE", Keys.E) { TestName = "{m}_Shift+KeyE" };
			yield return new TestCaseData("Shift+KeyF", Keys.F) { TestName = "{m}_Shift+KeyF" };
			yield return new TestCaseData("Shift+KeyG", Keys.G) { TestName = "{m}_Shift+KeyG" };
			yield return new TestCaseData("Shift+KeyH", Keys.H) { TestName = "{m}_Shift+KeyH" };
			yield return new TestCaseData("Shift+KeyI", Keys.I) { TestName = "{m}_Shift+KeyI" };
			yield return new TestCaseData("Shift+KeyJ", Keys.J) { TestName = "{m}_Shift+KeyJ" };
			yield return new TestCaseData("Shift+KeyK", Keys.K) { TestName = "{m}_Shift+KeyK" };
			yield return new TestCaseData("Shift+KeyL", Keys.L) { TestName = "{m}_Shift+KeyL" };
			yield return new TestCaseData("Shift+KeyM", Keys.M) { TestName = "{m}_Shift+KeyM" };
			yield return new TestCaseData("Shift+KeyN", Keys.N) { TestName = "{m}_Shift+KeyN" };
			yield return new TestCaseData("Shift+KeyO", Keys.O) { TestName = "{m}_Shift+KeyO" };
			yield return new TestCaseData("Shift+KeyP", Keys.P) { TestName = "{m}_Shift+KeyP" };
			yield return new TestCaseData("Shift+KeyQ", Keys.Q) { TestName = "{m}_Shift+KeyQ" };
			yield return new TestCaseData("Shift+KeyR", Keys.R) { TestName = "{m}_Shift+KeyR" };
			yield return new TestCaseData("Shift+KeyS", Keys.S) { TestName = "{m}_Shift+KeyS" };
			yield return new TestCaseData("Shift+KeyT", Keys.T) { TestName = "{m}_Shift+KeyT" };
			yield return new TestCaseData("Shift+KeyU", Keys.U) { TestName = "{m}_Shift+KeyU" };
			yield return new TestCaseData("Shift+KeyV", Keys.V) { TestName = "{m}_Shift+KeyV" };
			yield return new TestCaseData("Shift+KeyW", Keys.W) { TestName = "{m}_Shift+KeyW" };
			yield return new TestCaseData("Shift+KeyX", Keys.X) { TestName = "{m}_Shift+KeyX" };
			yield return new TestCaseData("Shift+KeyY", Keys.Y) { TestName = "{m}_Shift+KeyY" };
			yield return new TestCaseData("Shift+KeyZ", Keys.Z) { TestName = "{m}_Shift+KeyZ" };
			yield return new TestCaseData("Shift+Minus", Keys.OemMinus) { TestName = "{m}_Shift+Minus" };
			yield return new TestCaseData("Shift+Numpad0", Keys.NumPad0) { TestName = "{m}_Shift+Numpad0" };
			yield return new TestCaseData("Shift+Numpad1", Keys.NumPad1) { TestName = "{m}_Shift+Numpad1" };
			yield return new TestCaseData("Shift+Numpad2", Keys.NumPad2) { TestName = "{m}_Shift+Numpad2" };
			yield return new TestCaseData("Shift+Numpad3", Keys.NumPad3) { TestName = "{m}_Shift+Numpad3" };
			yield return new TestCaseData("Shift+Numpad4", Keys.NumPad4) { TestName = "{m}_Shift+Numpad4" };
			yield return new TestCaseData("Shift+Numpad5", Keys.NumPad5) { TestName = "{m}_Shift+Numpad5" };
			yield return new TestCaseData("Shift+Numpad6", Keys.NumPad6) { TestName = "{m}_Shift+Numpad6" };
			yield return new TestCaseData("Shift+Numpad7", Keys.NumPad7) { TestName = "{m}_Shift+Numpad7" };
			yield return new TestCaseData("Shift+Numpad8", Keys.NumPad8) { TestName = "{m}_Shift+Numpad8" };
			yield return new TestCaseData("Shift+Numpad9", Keys.NumPad9) { TestName = "{m}_Shift+Numpad9" };
			yield return new TestCaseData("Shift+NumpadDecimal", Keys.Decimal) { TestName = "{m}_Shift+NumpadDecimal" };
			yield return new TestCaseData("Shift+Period", Keys.OemPeriod) { TestName = "{m}_Shift+Period" };
			yield return new TestCaseData("Shift+Quote", Keys.Oem7) { TestName = "{m}_Shift+Quote" };
			yield return new TestCaseData("Shift+Semicolon", Keys.Oem1) { TestName = "{m}_Shift+Semicolon" };
			yield return new TestCaseData("Shift+Slash", Keys.OemQuestion) { TestName = "{m}_Shift+Slash" };
			yield return new TestCaseData("ShiftLeft", Keys.LShiftKey) { TestName = "{m}_ShiftLeft" };
			yield return new TestCaseData("ShiftRight", Keys.RShiftKey) { TestName = "{m}_ShiftRight" };
			yield return new TestCaseData("Slash", Keys.OemQuestion) { TestName = "{m}_Slash" };
			yield return new TestCaseData("Space", Keys.Space) { TestName = "{m}_Space" };
			yield return new TestCaseData("Tab", Keys.Tab) { TestName = "{m}_Tab" };
		}
	}

	// These keys aren't recognised by playwright, so we will manually specify the key codes
	static IEnumerable<TestCaseData> KeyCodeCases_Manual
	{
		get
		{
			yield return new TestCaseData("Alt", Keys.Menu) { TestName = "{m}_Alt" };
			yield return new TestCaseData("Control", Keys.ControlKey) { TestName = "{m}_Control" };
			yield return new TestCaseData("Meta", Keys.LWin) { TestName = "{m}_Meta" };
			yield return new TestCaseData("Shift", Keys.ShiftKey) { TestName = "{m}_Shift" };
			yield return new TestCaseData("Unidentified", Keys.None) { TestName = "{m}_Unidentified" };
			yield return new TestCaseData("Return", Keys.Enter) { TestName = "{m}_Return" };
			yield return new TestCaseData("F13", Keys.F13) { TestName = "{m}_F13" };
			yield return new TestCaseData("F14", Keys.F14) { TestName = "{m}_F14" };
			yield return new TestCaseData("F15", Keys.F15) { TestName = "{m}_F15" };
			yield return new TestCaseData("F16", Keys.F16) { TestName = "{m}_F16" };
			yield return new TestCaseData("F17", Keys.F17) { TestName = "{m}_F17" };
			yield return new TestCaseData("F18", Keys.F18) { TestName = "{m}_F18" };
			yield return new TestCaseData("F19", Keys.F19) { TestName = "{m}_F19" };
			yield return new TestCaseData("F20", Keys.F20) { TestName = "{m}_F20" };
			yield return new TestCaseData("F21", Keys.F21) { TestName = "{m}_F21" };
			yield return new TestCaseData("F22", Keys.F22) { TestName = "{m}_F22" };
			yield return new TestCaseData("F23", Keys.F23) { TestName = "{m}_F23" };
			yield return new TestCaseData("F24", Keys.F24) { TestName = "{m}_F24" };
			yield return new TestCaseData("AudioVolumeMute", Keys.VolumeMute) { TestName = "{m}_AudioVolumeMute" };
			yield return new TestCaseData("AudioVolumeDown", Keys.VolumeDown) { TestName = "{m}_AudioVolumeDown" };
			yield return new TestCaseData("AudioVolumeUp", Keys.VolumeUp) { TestName = "{m}_AudioVolumeUp" };
			yield return new TestCaseData("MediaNextTrack", Keys.MediaNextTrack) { TestName = "{m}_MediaNextTrack" };
			yield return new TestCaseData("MediaPreviousTrack", Keys.MediaPreviousTrack) { TestName = "{m}_MediaPreviousTrack" };
			yield return new TestCaseData("MediaStop", Keys.MediaStop) { TestName = "{m}_MediaStop" };
			yield return new TestCaseData("MediaPlayPause", Keys.MediaPlayPause) { TestName = "{m}_MediaPlayPause" };
			yield return new TestCaseData("LaunchMediaPlayer", Keys.SelectMedia) { TestName = "{m}_LaunchMediaPlayer" };
			yield return new TestCaseData("LaunchApplication1", Keys.LaunchApplication1) { TestName = "{m}_LaunchApplication1" };
			yield return new TestCaseData("LaunchApplication2", Keys.LaunchApplication2) { TestName = "{m}_LaunchApplication2" };
			yield return new TestCaseData("Process", Keys.ProcessKey) { TestName = "{m}_Process" };
			yield return new TestCaseData("HangulMode", Keys.HangulMode) { TestName = "{m}_HangulMode" };
			yield return new TestCaseData("Dead", Keys.None) { TestName = "{m}_None" };
		}
	}

	static IEnumerable<TestCaseData> KeyCodeCases_Manual_SpecialChars
	{
		get 
		{
			yield return new TestCaseData("ת", "Comma", Keys.Oemcomma) { TestName = "{m}_Comma" };
			yield return new TestCaseData("ץ", "Period", Keys.OemPeriod) { TestName = "{m}_Period" };
			yield return new TestCaseData("ו", "KeyU", Keys.U) { TestName = "{m}_U" };
			yield return new TestCaseData("ы", "KeyS", Keys.S) { TestName = "{m}_S" };
			yield return new TestCaseData("в", "KeyD", Keys.D) { TestName = "{m}_D" };
			yield return new TestCaseData("м", "KeyV", Keys.V) { TestName = "{m}_V" };
			yield return new TestCaseData("á", "KeyQ", Keys.Q) { TestName = "{m}_Q" };
			yield return new TestCaseData("ó", "KeyA", Keys.A) { TestName = "{m}_A" };
			yield return new TestCaseData("í", "KeyB", Keys.B) { TestName = "{m}_B" };
			yield return new TestCaseData("ı", "KeyZ", Keys.Z) { TestName = "{m}_Z" };
			yield return new TestCaseData("Ư", "", Keys.None) { TestName = "{m}_Ư" };
			yield return new TestCaseData("special_key", "", Keys.None) { TestName = "{m}_EmptyCode" };
			yield return new TestCaseData("MediaTrackPrevious", "MediaTrackPrevious", Keys.MediaPreviousTrack) { TestName = "{m}_MediaTrackPrevious" };
			yield return new TestCaseData("MediaTrackNext", "MediaTrackNext", Keys.MediaNextTrack) { TestName = "{m}_MediaTrackNext" };
			yield return new TestCaseData("KanaMode", "KanaMode", Keys.KanaMode) { TestName = "{m}_KanaMode" };
			yield return new TestCaseData("HanguelMode", "HanguelMode", Keys.HanguelMode) { TestName = "{m}_HanguelMode" };
			yield return new TestCaseData("JunjaMode", "JunjaMode", Keys.JunjaMode) { TestName = "{m}_JunjaMode" };
			yield return new TestCaseData("FinalMode", "FinalMode", Keys.FinalMode) { TestName = "{m}_FinalMode" };
			yield return new TestCaseData("KanjiMode", "KanjiMode", Keys.KanjiMode) { TestName = "{m}_KanjiMode" };
			yield return new TestCaseData("BrowserBack", "BrowserBack", Keys.BrowserBack) { TestName = "{m}_BrowserBack" };
			yield return new TestCaseData("BrowserForward", "BrowserForward", Keys.BrowserForward) { TestName = "{m}_BrowserForward" };
			yield return new TestCaseData("BrowserRefresh", "BrowserRefresh", Keys.BrowserRefresh) { TestName = "{m}_BrowserRefresh" };
			yield return new TestCaseData("BrowserSearch", "BrowserSearch", Keys.BrowserSearch) { TestName = "{m}_BrowserSearch" };
			yield return new TestCaseData("BrowserStop", "BrowserStop", Keys.BrowserStop) { TestName = "{m}_BrowserStop" };
			yield return new TestCaseData("BrowserFavorites", "BrowserFavorites", Keys.BrowserFavorites) { TestName = "{m}_BrowserFavorites" };
			yield return new TestCaseData("BrowserHome", "KeyBrowserHome", Keys.BrowserHome) { TestName = "{m}_BrowserHome" };
		}
	}
}
