#nullable enable

using System.Collections.Generic;
using System.Linq;
using static System.Windows.Forms.Control;

namespace System.Windows.Forms;

/// <summary>
///  Provides methods for sending keystrokes to an application.
/// </summary>
public static class SendKeys
{
	// It is unclear what significance the value 10 has, but it seems to make sense to make this a constant rather
	// than have 10 sprinkled throughout the code. It appears to be a sentinel value of some sort - indicating an
	// unknown grouping level.
	const int UnknownGrouping = 10;

	/// <summary>
	///  Sends keystrokes to the active application.
	/// </summary>
	public static void Send(string keys) => Send(keys, null, false);

	static void Send(string keys, Control? control, bool wait)
	{
		if (string.IsNullOrEmpty(keys))
		{
			return;
		}

		// if control is not null we use itself
		if (control == null)
		{
			//It is assumed that the newest form will be used.
			var form = Application.OpenForms.Last();
			control = form.LastFocusedControl ?? form;
		}

		//ParseKeys will populate the EventQueue.
		EventQueue.Clear();
		ParseKeys(keys);
		foreach (var keyEvent in EventQueue)
		{
			if (keyEvent.EventType == Control.KeyEventType.KeyDown)
			{
				SendInput(control, keyEvent);
			}
			else if (keyEvent.EventType == Control.KeyEventType.KeyUp)
			{
				control.ProcessKeyEventCore(Control.KeyEventType.KeyUp, keyEvent.RawKey, keyEvent.Key, keyEvent.AltKey, keyEvent.CtrlKey, keyEvent.ShiftKey, false);
			}
		}
	}

	/// <summary>
	///  Sends the given keys to the active application, and then waits for  the messages to be processed.
	/// </summary>
	public static void SendWait(string keys) => SendWait(keys, null);

	/// <summary>
	///  Sends the given keys to the active application, and then waits for the messages to be processed.
	/// </summary>
	/// <remarks>
	///  WARNING: this method will never work if control is not null, because while Windows journaling *looks* like it
	///  can be directed to a specific HWND, it can't.
	///
	///  However, when testing some controls, we don't always create a parent form for them,
	///  so we need to make this function work when control is directly specified.
	/// </remarks>
	public static void SendWait(string keys, Control? control) => Send(keys, control, true);

	/// <summary>
	///  Parse the string the user has given us, and generate the appropriate events for the journaling hook.
	/// </summary>
	static void ParseKeys(string keys)
	{
		var i = 0;

		// These four variables are used for grouping
		(int HaveShift, int HaveCtrl, int HaveAlt) haveKeys = default;
		var cGrp = 0;

		// Start walking through the characters one at a time.
		var keysLen = keys.Length;
		while (i < keysLen)
		{
			var repeat = 1;
			var ch = keys[i];
			Keys vk;

			switch (ch)
			{
				case '}':
					// If these appear at this point they are out of context, so return an error. KeyStart
					// processes ochKeys up to the appropriate KeyEnd.
					throw new ArgumentException(string.Format(SR.InvalidSendKeysString, keys));

				case '{':
					var j = i + 1;

					// There's a unique class of strings of the form "{} n}" where n is an integer - in this case
					// we want to send n copies of the '}' character. Here we test for the possibility of this
					// class of problems, and skip the first '}' in the string if necessary.
					if (j + 1 < keysLen && keys[j] == '}')
					{
						// Scan for the final '}' character
						var final = j + 1;
						while (final < keysLen && keys[final] != '}')
						{
							final++;
						}

						if (final < keysLen)
						{
							// Found the special case, so skip the first '}' in the string. The remainder of the
							// code will attempt to find the repeat count.
							j++;
						}
					}

					// We're in a {<KEYWORD>...} situation. Look for the keyword.
					while (j < keysLen && keys[j] != '}'
						   && !char.IsWhiteSpace(keys[j]))
					{
						j++;
					}

					if (j >= keysLen)
					{
						throw new ArgumentException(SR.SendKeysKeywordDelimError);
					}

					// Have our KEYWORD. Verify it's one we know about.
					var keyName = keys.Substring(i + 1, j - (i + 1));

					// See if we have a space, which would mean a repeat count.
					if (char.IsWhiteSpace(keys[j]))
					{
						int digit;
						while (j < keysLen && char.IsWhiteSpace(keys[j]))
						{
							j++;
						}

						if (j >= keysLen)
						{
							throw new ArgumentException(SR.SendKeysKeywordDelimError);
						}

						if (char.IsDigit(keys[j]))
						{
							digit = j;
							while (j < keysLen && char.IsDigit(keys[j]))
							{
								j++;
							}

							repeat = int.Parse(keys.AsSpan(digit, j - digit)/*, CultureInfo.InvariantCulture*/);
						}
					}

					if (j >= keysLen)
					{
						throw new ArgumentException(SR.SendKeysKeywordDelimError);
					}

					if (keys[j] != '}')
					{
						throw new ArgumentException(SR.InvalidSendKeysRepeat);
					}

					vk = MatchKeyword(keyName);
					if (vk != Keys.None)
					{
						AddEventToQueue(keyName, vk, haveKeys, repeat);
						CancelMods(ref haveKeys, UnknownGrouping);
					}
					else if (keyName.Length == 1)
					{
						AddSimpleKey(keyName[0], repeat, haveKeys);
					}
					else
					{
						throw new ArgumentException(string.Format(SR.InvalidSendKeysKeyword, keys.Substring(i + 1, j - (i + 1))));
					}

					// don't forget to position ourselves at the end of the {...} group
					i = j;
					break;

				case '+':
					if (haveKeys.HaveShift != 0)
					{
						throw new ArgumentException(string.Format(SR.InvalidSendKeysString, keys));
					}

					haveKeys.HaveShift = UnknownGrouping;
					AddEventToQueue("Shift", Keys.ShiftKey, haveKeys, keyUp: false);
					break;

				case '^':
					if (haveKeys.HaveCtrl != 0)
					{
						throw new ArgumentException(string.Format(SR.InvalidSendKeysString, keys));
					}

					haveKeys.HaveCtrl = UnknownGrouping;
					AddEventToQueue("Control", Keys.ControlKey, haveKeys, keyUp: false);
					break;

				case '%':
					if (haveKeys.HaveAlt != 0)
					{
						throw new ArgumentException(string.Format(SR.InvalidSendKeysString, keys));
					}

					haveKeys.HaveAlt = UnknownGrouping;
					AddEventToQueue("Alt", Keys.Menu, haveKeys, keyUp: false);
					break;

				case '(':
					// Convert all immediate mode states to group mode. Allows multiple keys with the same shift,
					// etc. state. Nests three deep.
					cGrp++;
					if (cGrp > 3)
					{
						throw new ArgumentException(SR.SendKeysNestingError);
					}
					break;

				case ')':
					if (cGrp < 1)
					{
						throw new ArgumentException(string.Format(SR.InvalidSendKeysString, keys));
					}

					CancelMods(ref haveKeys, cGrp);
					cGrp--;
					break;

				case '~':
					vk = Keys.Return;
					AddEventToQueue("Return", vk, haveKeys, repeat);
					break;

				default:
					AddSimpleKey(keys[i], repeat, haveKeys);
					break;
			}

			// Next element in the string.
			i++;
		}

		if (cGrp != 0)
		{
			throw new ArgumentException(SR.SendKeysGroupDelimError);
		}

		CancelMods(ref haveKeys, UnknownGrouping);
	}

	// In Winzor, we store a queue of keyboard events (KeyboardEventArgs), which are fired in sequence.
	// KeyUp and KeyDown events are stored separately, as Ctrl/Alt/Shift keys may be held down across multiple key presses.
	static readonly List<KeyEventData> EventQueue = new List<KeyEventData>();

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
	record KeyEventData(Control.KeyEventType EventType, string RawKey, Keys Key, bool AltKey, bool CtrlKey, bool ShiftKey);
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter

	static void AddSimpleKey(char character, int repeat, (int HaveShift, int HaveCtrl, int HaveAlt) haveKeys)
	{
		var charEntry = Chars.FirstOrDefault(c => c.Char == character);

		//Some simple keys will require Shift to be pressed.
		var pressShiftKey = false;
		if (haveKeys.HaveShift == 0 && charEntry.Shift)
		{
			pressShiftKey = true;
			AddEventToQueue("Shift", Keys.Shift, haveKeys, keyUp: false);
			haveKeys.HaveShift = UnknownGrouping;
		}

		AddEventToQueue(character.ToString(), charEntry.VK, haveKeys, repeat, forceShift: pressShiftKey);
		CancelMods(ref haveKeys, UnknownGrouping);
	}

	static void CancelMods(ref (int HaveShift, int HaveCtrl, int HaveAlt) haveKeys, int level)
	{
		if (haveKeys.HaveShift == level)
		{
			AddEventToQueue("Shift", Keys.ShiftKey, haveKeys, keyDown: false);
			haveKeys.HaveShift = 0;
		}

		if (haveKeys.HaveCtrl == level)
		{
			AddEventToQueue("Control", Keys.ControlKey, haveKeys, keyDown: false);
			haveKeys.HaveCtrl = 0;
		}

		if (haveKeys.HaveAlt == level)
		{
			AddEventToQueue("Alt", Keys.Menu, haveKeys, keyDown: false);
			haveKeys.HaveAlt = 0;
		}
	}

	static void AddEventToQueue(string rawKey, Keys key, (int HaveShift, int HaveCtrl, int HaveAlt) haveKeys, int repeat = 1, bool keyDown = true, bool keyUp = true, bool forceShift = false)
	{
		var keyEventData = new KeyEventData(Control.KeyEventType.KeyDown, rawKey, key, haveKeys.HaveAlt != 0, haveKeys.HaveCtrl != 0, haveKeys.HaveShift != 0 | forceShift);
		for (var i = 0; i < repeat; i++)
		{
			if (keyDown)
			{
				EventQueue.Add(keyEventData);
			}
			if (keyUp)
			{
				EventQueue.Add(keyEventData with { EventType = Control.KeyEventType.KeyUp });
			}
		}
	}

	/// <summary>
	///  Given the string, match the keyword to a VK. Return -1 if we don't match anything.
	/// </summary>
	static Keys MatchKeyword(string keyword)
	{
		for (var i = 0; i < Keywords.Length; i++)
		{
			if (string.Equals(Keywords[i].Keyword, keyword, StringComparison.OrdinalIgnoreCase))
			{
				return Keywords[i].VK;
			}
		}

		return Keys.None;
	}

	static readonly KeywordVk[] Keywords = new KeywordVk[]
	{
		new KeywordVk("ENTER",      Keys.Return),
		new KeywordVk("TAB",        Keys.Tab),
		new KeywordVk("ESC",        Keys.Escape),
		new KeywordVk("ESCAPE",     Keys.Escape),
		new KeywordVk("HOME",       Keys.Home),
		new KeywordVk("END",        Keys.End),
		new KeywordVk("LEFT",       Keys.Left),
		new KeywordVk("RIGHT",      Keys.Right),
		new KeywordVk("UP",         Keys.Up),
		new KeywordVk("DOWN",       Keys.Down),
		new KeywordVk("PGUP",       Keys.Prior),
		new KeywordVk("PGDN",       Keys.Next),
		new KeywordVk("NUMLOCK",    Keys.NumLock),
		new KeywordVk("SCROLLLOCK", Keys.Scroll),
		new KeywordVk("PRTSC",      Keys.PrintScreen),
		new KeywordVk("BREAK",      Keys.Cancel),
		new KeywordVk("BACKSPACE",  Keys.Back),
		new KeywordVk("BKSP",       Keys.Back),
		new KeywordVk("BS",         Keys.Back),
		new KeywordVk("CLEAR",      Keys.Clear),
		new KeywordVk("CAPSLOCK",   Keys.Capital),
		new KeywordVk("INS",        Keys.Insert),
		new KeywordVk("INSERT",     Keys.Insert),
		new KeywordVk("DEL",        Keys.Delete),
		new KeywordVk("DELETE",     Keys.Delete),
		new KeywordVk("HELP",       Keys.Help),
		new KeywordVk("F1",         Keys.F1),
		new KeywordVk("F2",         Keys.F2),
		new KeywordVk("F3",         Keys.F3),
		new KeywordVk("F4",         Keys.F4),
		new KeywordVk("F5",         Keys.F5),
		new KeywordVk("F6",         Keys.F6),
		new KeywordVk("F7",         Keys.F7),
		new KeywordVk("F8",         Keys.F8),
		new KeywordVk("F9",         Keys.F9),
		new KeywordVk("F10",        Keys.F10),
		new KeywordVk("F11",        Keys.F11),
		new KeywordVk("F12",        Keys.F12),
		new KeywordVk("F13",        Keys.F13),
		new KeywordVk("F14",        Keys.F14),
		new KeywordVk("F15",        Keys.F15),
		new KeywordVk("F16",        Keys.F16),
		new KeywordVk("MULTIPLY",   Keys.Multiply),
		new KeywordVk("ADD",        Keys.Add),
		new KeywordVk("SUBTRACT",   Keys.Subtract),
		new KeywordVk("DIVIDE",     Keys.Divide),
		new KeywordVk("+",          Keys.Add),
		new KeywordVk("%",          Keys.D5 | Keys.Shift),
		new KeywordVk("^",          Keys.D6 | Keys.Shift),
	};

	static readonly CharVk[] Chars = new CharVk[]
	{
		new CharVk('a', Keys.A),
		new CharVk('b', Keys.B),
		new CharVk('c', Keys.C),
		new CharVk('d', Keys.D),
		new CharVk('e', Keys.E),
		new CharVk('f', Keys.F),
		new CharVk('g', Keys.G),
		new CharVk('h', Keys.H),
		new CharVk('i', Keys.I),
		new CharVk('j', Keys.J),
		new CharVk('k', Keys.K),
		new CharVk('l', Keys.L),
		new CharVk('m', Keys.M),
		new CharVk('n', Keys.N),
		new CharVk('o', Keys.O),
		new CharVk('p', Keys.P),
		new CharVk('q', Keys.Q),
		new CharVk('r', Keys.R),
		new CharVk('s', Keys.S),
		new CharVk('t', Keys.T),
		new CharVk('u', Keys.U),
		new CharVk('v', Keys.V),
		new CharVk('w', Keys.W),
		new CharVk('x', Keys.X),
		new CharVk('y', Keys.Y),
		new CharVk('z', Keys.Z),
		new CharVk('A', Keys.A, true),
		new CharVk('B', Keys.B, true),
		new CharVk('C', Keys.C, true),
		new CharVk('D', Keys.D, true),
		new CharVk('E', Keys.E, true),
		new CharVk('F', Keys.F, true),
		new CharVk('G', Keys.G, true),
		new CharVk('H', Keys.H, true),
		new CharVk('I', Keys.I, true),
		new CharVk('J', Keys.J, true),
		new CharVk('K', Keys.K, true),
		new CharVk('L', Keys.L, true),
		new CharVk('M', Keys.M, true),
		new CharVk('N', Keys.N, true),
		new CharVk('O', Keys.O, true),
		new CharVk('P', Keys.P, true),
		new CharVk('Q', Keys.Q, true),
		new CharVk('R', Keys.R, true),
		new CharVk('S', Keys.S, true),
		new CharVk('T', Keys.T, true),
		new CharVk('U', Keys.U, true),
		new CharVk('V', Keys.V, true),
		new CharVk('W', Keys.W, true),
		new CharVk('X', Keys.X, true),
		new CharVk('Y', Keys.Y, true),
		new CharVk('Z', Keys.Z, true),
		new CharVk('1', Keys.D1),
		new CharVk('2', Keys.D2),
		new CharVk('3', Keys.D3),
		new CharVk('4', Keys.D4),
		new CharVk('5', Keys.D5),
		new CharVk('6', Keys.D6),
		new CharVk('7', Keys.D7),
		new CharVk('8', Keys.D8),
		new CharVk('9', Keys.D9),
		new CharVk('0', Keys.D0),
		new CharVk('!', Keys.D1, true),
		new CharVk('@', Keys.D2, true),
		new CharVk('#', Keys.D3, true),
		new CharVk('$', Keys.D4, true),
		new CharVk('%', Keys.D5, true),
		new CharVk('^', Keys.D6, true),
		new CharVk('&', Keys.D7, true),
		new CharVk('*', Keys.D8, true),
		new CharVk('(', Keys.D9, true),
		new CharVk(')', Keys.D0, true),
		new CharVk('`', Keys.Oemtilde),
		new CharVk('-', Keys.OemMinus),
		new CharVk('-', Keys.Subtract),
		new CharVk('=', Keys.Oemplus),
		new CharVk('[', Keys.OemOpenBrackets),
		new CharVk(']', Keys.OemCloseBrackets),
		new CharVk('\\', Keys.OemPipe),
		new CharVk('\b', Keys.Back),
		new CharVk('\r', Keys.Enter),
		new CharVk('\u001b', Keys.Escape),
		new CharVk(';', Keys.OemSemicolon),
		new CharVk('\'', Keys.OemQuotes),
		new CharVk(',', Keys.Oemcomma),
		new CharVk('.', Keys.OemPeriod),
		new CharVk('/', Keys.OemQuestion),
		new CharVk('~', Keys.Oemtilde, true),
		new CharVk('_', Keys.OemMinus, true),
		new CharVk('+', Keys.Oemplus, true),
		new CharVk('+', Keys.Add),
		new CharVk('[', Keys.OemOpenBrackets, true),
		new CharVk(']', Keys.OemCloseBrackets, true),
		new CharVk('|', Keys.OemPipe, true),
		new CharVk(':', Keys.OemSemicolon, true),
		new CharVk('\"', Keys.OemQuotes, true),
		new CharVk('<', Keys.Oemcomma, true),
		new CharVk('>', Keys.OemPeriod, true),
		new CharVk('?', Keys.OemQuestion, true),
		new CharVk('0', Keys.NumPad0),
		new CharVk('1', Keys.NumPad1),
		new CharVk('2', Keys.NumPad2),
		new CharVk('3', Keys.NumPad3),
		new CharVk('4', Keys.NumPad4),
		new CharVk('5', Keys.NumPad5),
		new CharVk('6', Keys.NumPad6),
		new CharVk('7', Keys.NumPad7),
		new CharVk('8', Keys.NumPad8),
		new CharVk('9', Keys.NumPad9),
	};

	static string GetKeyString(Keys key)
	{
		var hasShift = (key & Keys.Shift) == Keys.Shift;
		var keyCode = (key & Keys.KeyCode);
		var match = Chars.FirstOrDefault(c => c.VK == keyCode && c.Shift == hasShift);
		return match.Char != '\0' ? match.Char.ToString() : key.ToString();
	}

	public static Keys GetKeyFromChar(char ch)
	{
		var match = Chars.FirstOrDefault(c => c.Char == ch);
		if (match.VK != Keys.None)
		{
			return match.VK | (match.Shift ? Keys.Shift : Keys.None);
		}
		// Fallback: cast the character code to a Keys value directly.
		// This works for many common ASCII characters (e.g. letters, digits, Backspace),
		// but may produce invalid or undefined results for extended Unicode characters.
		return (Keys)ch;
	}

	/// <summary>
	///  Holds a keyword and the associated VK_ for it.
	/// </summary>
	readonly struct KeywordVk
	{
		public readonly string Keyword;
		public readonly Keys VK;

		public KeywordVk(string keyword, Keys key)
		{
			Keyword = keyword;
			VK = key;
		}
	}

	/// <summary>
	/// Stores a list of chars, along with their VK and whether to activate shift.
	/// </summary>
	readonly struct CharVk
	{
		public readonly char Char;
		public readonly Keys VK;
		public readonly bool Shift;

		public CharVk(char character, Keys key, bool shift = false)
		{
			Char = character;
			VK = key;
			Shift = shift;
		}
	}

	#region Simulate Browser Behaviour

	public static void SendInput(Control control, Keys key)
	{
		var parentForm = control.FindForm();
		if (parentForm != null)
		{
			parentForm.CurrentKeyEvent = KeyEventType.KeyDown;
		}
		var rawKey = key & Keys.KeyCode;

		var keyEventData = new KeyEventData(Control.KeyEventType.KeyDown, GetKeyString(rawKey), rawKey, (key & Keys.Alt) == Keys.Alt, (key & Keys.Control) == Keys.Control, (key & Keys.Shift) == Keys.Shift);

		SendInput(control, keyEventData);

		if (parentForm != null)
		{
			parentForm.CurrentKeyEvent = KeyEventType.None;
		}
	}

	static void SendInput(Control control, KeyEventData keyEvent)
	{
		//The WinForms version of SendKeys will simulate an actual key press.
		//As some Winzor behaviour is handled by the browser (e.g. text entry, tab focusing, etc.),
		//we should simulate it here to ensure compatibility with legacy tests using SendKeys.

		SimulateBrowserKeyDown(keyEvent.Key, control, keyEvent.ShiftKey);

		// record textbox
		var oldText = string.Empty;
		var oldSelectionStart = 0;
		var oldSelectionLength = 0;
		if (control is TextBox textBox)
		{
			oldText = textBox.Text;
			oldSelectionStart = textBox.SelectionStart;
			oldSelectionLength = textBox.SelectionLength;
		}

		control.ProcessKeyEventCore(Control.KeyEventType.KeyDown, keyEvent.RawKey, keyEvent.Key, keyEvent.AltKey, keyEvent.CtrlKey, keyEvent.ShiftKey, true);

		SimulateBrowserOnInput(keyEvent.Key, keyEvent.RawKey, control, oldText, oldSelectionStart, oldSelectionLength);
	}

	public static void SimulateBrowserKeyDown(Keys key, Control control, bool shiftKey)
	{
		// DataGrid is not yet supported.
		if (control is DataGrid || ControlIsPartOfDataGrid(control, withSupportColumnStyle: false))
		{
			return;
		}

		// Grid cell should not simulate Tab key
		switch (key)
		{
			case Keys.Tab:
				SimulateTab(control, !shiftKey);
				return;
		}
	}

	public static void SimulateBrowserOnInput(Keys key, string rawKey, Control control, string text, int selectionStart, int selectionLength)
	{
		if (TriggerKeyPressEvent(rawKey, control))
		{
			return;
		}

		// DataGrid is not yet supported.
		if (control is DataGrid || ControlIsPartOfDataGrid(control))
		{
			return;
		}

		if (control is TextBox textBox)
		{
			var originalText = text;
			var originalSelectionStart = selectionStart;
			var originalSelectionLength = selectionLength;

			// simulate browser
			SimulateTextBox(key, rawKey, textBox, ref text, ref selectionStart, ref selectionLength);

			if (originalText != text)
			{
				SimulateAutoCompleteText(key, textBox, ref text, ref selectionLength);
			}

			if(originalSelectionStart != selectionStart || originalSelectionLength != selectionLength)
			{
				// simulate server OnTextBoxSelectionChanged
				textBox.OnTextBoxSelectionChanged(selectionStart, selectionLength);
			}
			if (originalText != text)
			{
				// simulate server OnInput
				textBox.OnInput(text);
			}
		}
	}

	static void SimulateTab(Control control, bool forward)
	{
		var activeForm = control.FindForm();
		if (activeForm != null)
		{
			var currentControl = activeForm.ActiveControl;
			activeForm.SelectNextControl(currentControl, forward, tabStopOnly: true, nested: true, wrap: true);
			activeForm.ActiveControl?.Focus();
		}
	}

	static void SimulateAutoCompleteText(Keys key, TextBox textBox, ref string text, ref int selectionLength)
	{
		if (key == Keys.Delete || key == Keys.Back)
		{
			return;
		}

		if (textBox is { ReadOnly: false, AutoComplete: true } && textBox.Suggestions != null)
		{
			var input = text;
			var suggestion = textBox.Suggestions
				.FirstOrDefault(x => x.ToUpperInvariant().StartsWith(input.ToUpperInvariant()));

			if (suggestion is not null)
			{
				text = suggestion;
				selectionLength = suggestion.Length;
			}
		}
	}

	static void SimulateTextBox(Keys key, string rawKey, TextBox textBox, ref string text, ref int selectionStart, ref int selectionLength)
	{
		if (textBox.ReplacementCharacters != null && rawKey.Length == 1 && textBox.ReplacementCharacters.TryGetValue(rawKey[0], out var replacement))
		{
			rawKey = replacement.ToString();
			key = GetKeyFromChar(replacement);
		}

		if (key == Keys.Return && !textBox.AcceptsReturn)
		{
			return;
		}

		var modifyText = !textBox.ReadOnly &&
					(rawKey.Length == 1 || key == Keys.Back || key == Keys.Delete);

		if (modifyText)
		{
			switch (key)
			{
				case Keys.Back:
					if (selectionLength > 0)
					{
						RemoveSelectedText(ref text, ref selectionStart, ref selectionLength);
					}
					else if (selectionStart > 0)
					{
						// Remove prior character
						text = text.Remove(selectionStart - 1, 1);
						selectionLength = 0;
						selectionStart = selectionStart - 1;
					}
					break;

				case Keys.Delete:
					if (selectionLength > 0)
					{
						RemoveSelectedText(ref text, ref selectionStart, ref selectionLength);
					}
					else if (selectionStart < text.Length)
					{
						// Remove character after the cursor
						text = text.Remove(selectionStart, 1);
						// Cursor position stays the same, so no need to adjust selectionStart
					}
					break;

				default:
					var newLength = text.Length - selectionLength + rawKey.Length;
					if (textBox.MaxLength == 0 || newLength <= textBox.MaxLength)
					{
						// Remove currently-selected text.
						text = text.Remove(selectionStart, selectionLength);

						// Add text at cursor position.
						text = text.Insert(selectionStart, rawKey);
						selectionStart = selectionStart + rawKey.Length;
					}
					break;
			}
		}
		else
		{
			// Cursor manipulation.
			// Multiline textboxes are not yet supported.
			if (!textBox.Multiline)
			{
				switch (key)
				{
					case Keys.Left:
					case Keys.Up:
						selectionStart = Math.Max(selectionStart - 1, 0);
						selectionLength = 0;
						break;
					case Keys.Right:
					case Keys.Down:
						selectionStart = Math.Min(selectionStart + 1, text.Length);
						selectionLength = 0;
						break;
					case Keys.Home:
						selectionStart = 0;
						selectionLength = 0;
						break;
					case Keys.End:
						selectionStart = text.Length;
						selectionLength = 0;
						break;
				}
			}
		}

		void RemoveSelectedText(ref string text, ref int selectionStart, ref int selectionLength)
		{
			text = text.Remove(selectionStart, selectionLength);
			selectionLength = 0;
		}
	}

	static bool ControlIsPartOfDataGrid(Control control, bool withSupportColumnStyle = true)
	{
		var parent = control.Parent;

		while (parent != null)
		{
			if (parent is DataGrid grid)
			{
				if (withSupportColumnStyle && SupportColumnStyles.Contains(grid.TableStyles[0].GridColumnStyles[0].ToString()))
				{
					return false;
				}

				return true;
			}

			if (parent == parent.Parent)
			{
				return false;
			}

			parent = parent.Parent;
		}

		return false;
	}

	static IEnumerable<string> SupportColumnStyles
	{
		get
		{
			yield return "Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyle";
			yield return "Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyle";
			yield return "Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyle";
			yield return "Enterprise.ZArchitecture.ZCalcEditColumnStyle";
			yield return "Enterprise.ZArchitecture.ZTextBoxColumnStyle";
			yield return "Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyle";
		}
	}

	#endregion

	public class KeyPressedEventArgs : EventArgs
	{
		public string Key { get; }
		public Control Control { get; }
		public bool Handled { get; set; }

		public KeyPressedEventArgs(string key, Control control)
		{
			Key = key;
			Control = control;
			Handled = false;
		}
	}

	public static event EventHandler<KeyPressedEventArgs>? KeyPressed;

	static bool TriggerKeyPressEvent(string key, Control control)
	{
		var args = new KeyPressedEventArgs(key, control);
		KeyPressed?.Invoke(null, args);
		return args.Handled;
	}

	public static void ClearKeyPressedHandlers()
	{
		KeyPressed = null;
	}
}
