// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable enable

using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Text;

namespace System.Windows.Forms
{
	internal sealed partial class WindowsFormsUtils
	{
		public const ContentAlignment AnyRightAlign = ContentAlignment.TopRight | ContentAlignment.MiddleRight | ContentAlignment.BottomRight;
		public const ContentAlignment AnyLeftAlign = ContentAlignment.TopLeft | ContentAlignment.MiddleLeft | ContentAlignment.BottomLeft;
		public const ContentAlignment AnyTopAlign = ContentAlignment.TopLeft | ContentAlignment.TopCenter | ContentAlignment.TopRight;
		public const ContentAlignment AnyBottomAlign = ContentAlignment.BottomLeft | ContentAlignment.BottomCenter | ContentAlignment.BottomRight;
		public const ContentAlignment AnyMiddleAlign = ContentAlignment.MiddleLeft | ContentAlignment.MiddleCenter | ContentAlignment.MiddleRight;
		public const ContentAlignment AnyCenterAlign = ContentAlignment.TopCenter | ContentAlignment.MiddleCenter | ContentAlignment.BottomCenter;

		/// <summary>
		///  If you want to know if a piece of text contains one and only one &amp;
		///  this is your function. If you have a character "t" and want match it to &amp;Text
		///  Control.IsMnemonic is a better bet.
		/// </summary>
		public static bool ContainsMnemonic(string? text)
		{
			if (text is not null)
			{
				int textLength = text.Length;
				int firstAmpersand = text.IndexOf('&', 0);
				if (firstAmpersand >= 0 && firstAmpersand <= /*second to last char=*/textLength - 2)
				{
					// we found one ampersand and it's either the first character
					// or the second to last character
					// or a character in between

					// We're so close!  make sure we don't have a double ampersand now.
					int secondAmpersand = text.IndexOf('&', firstAmpersand + 1);
					if (secondAmpersand == -1)
					{
						// didn't find a second one in the string.
						return true;
					}
				}
			}

			return false;
		}

		internal static Rectangle ConstrainToBounds(Rectangle constrainingBounds, Rectangle bounds)
		{
			// use screen instead of SystemInformation.WorkingArea for better multimon support.
			if (!constrainingBounds.Contains(bounds))
			{
				// make sure size does not exceed working area.
#pragma warning disable CW1017 // Non DPI-aware code has been detected
				bounds.Size = new Size(Math.Min(constrainingBounds.Width - 2, bounds.Width),
									   Math.Min(constrainingBounds.Height - 2, bounds.Height));
#pragma warning restore CW1017 // Non DPI-aware code has been detected

				// X calculations
				//
				// scooch so it will fit on the screen.
				if (bounds.Right > constrainingBounds.Right)
				{
					// its too far to the right.
					bounds.X = constrainingBounds.Right - bounds.Width;
				}
				else if (bounds.Left < constrainingBounds.Left)
				{
					// its too far to the left.
					bounds.X = constrainingBounds.Left;
				}

				// Y calculations
				//
				// scooch so it will fit on the screen.
				if (bounds.Bottom > constrainingBounds.Bottom)
				{
					// its too far to the bottom.
					bounds.Y = constrainingBounds.Bottom - 1 - bounds.Height;
				}
				else if (bounds.Top < constrainingBounds.Top)
				{
					// its too far to the top.
					bounds.Y = constrainingBounds.Top;
				}
			}

			return bounds;
		}

		/// <summary>
		///  Adds an extra &amp; to to the text so that "Fish &amp; Chips" can be displayed on a menu item
		///  without underlining anything.
		///  Fish &amp; Chips --> Fish &amp;&amp; Chips
		/// </summary>
		internal static string? EscapeTextWithAmpersands(string? text)
		{
			if (text is null)
			{
				return null;
			}

			int index = text.IndexOf('&');
			if (index == -1)
			{
				return text;
			}

			StringBuilder str = new StringBuilder(text.Substring(0, index));
			for (; index < text.Length; ++index)
			{
				if (text[index] == '&')
				{
					str.Append('&');
				}

				if (index < text.Length)
				{
					str.Append(text[index]);
				}
			}

			return str.ToString();
		}

		/// <summary>
		///  Retrieves the mnemonic from a given string, or zero if no mnemonic.
		///  As used by the Control.Mnemonic to get mnemonic from Control.Text.
		/// </summary>
		public static char GetMnemonic(string? text, bool convertToUpperCase)
		{
			char mnemonic = '\0';
			if (text is not null)
			{
				int len = text.Length;
				for (int i = 0; i < len - 1; i++)
				{
					if (text[i] == '&')
					{
						if (text[i + 1] == '&')
						{
							// we have an escaped &, so we need to skip it.
							i++;
							continue;
						}

						if (convertToUpperCase)
						{
							mnemonic = char.ToUpper(text[i + 1], CultureInfo.CurrentCulture);
						}
						else
						{
							mnemonic = char.ToLower(text[i + 1], CultureInfo.CurrentCulture);
						}

						break;
					}
				}
			}

			return mnemonic;
		}

		/// <summary>
		///  Strips all keyboard mnemonic prefixes from a given string, eg. turning "He&amp;lp" into "Help".
		/// </summary>
		/// <remarks>
		///  Note: Be careful not to call this multiple times on the same string, otherwise you'll turn
		///  something like "Fi&amp;sh &amp;&amp; Chips" into "Fish &amp; Chips" on the first call, and then "Fish Chips"
		///  on the second call.
		/// </remarks>
		[return: NotNullIfNotNull(nameof(text))]
		public static string? TextWithoutMnemonics(string? text)
		{
			if (text is null)
			{
				return null;
			}

			int index = text.IndexOf('&');
			if (index == -1)
			{
				return text;
			}

			StringBuilder str = new StringBuilder(text.Substring(0, index));
			for (; index < text.Length; ++index)
			{
				if (text[index] == '&')
				{
					// Skip this & and copy the next character instead
					index++;
				}

				if (index < text.Length)
				{
					str.Append(text[index]);
				}
			}

			return str.ToString();
		}

		/// <summary>
		///  Compares the strings using invariant culture for Turkish-I support. Returns true if they match.
		///
		///  If your strings are symbolic (returned from APIs, not from user) the following calls
		///  are faster than this method:
		///
		///  String.Equals(s1, s2, StringComparison.Ordinal)
		///  String.Equals(s1, s2, StringComparison.OrdinalIgnoreCase)
		/// </summary>
		public static bool SafeCompareStrings(string? string1, string? string2, bool ignoreCase)
		{
			if ((string1 is null) || (string2 is null))
			{
				// if either key is null, we should return false
				return false;
			}

			// Because String.Compare returns an ordering, it can not terminate early if lengths are not the same.
			// Also, equivalent characters can be encoded in different byte sequences, so it can not necessarily
			// terminate on the first byte which doesn't match. Hence this optimization.
			if (string1.Length != string2.Length)
			{
				return false;
			}

			return string.Compare(string1, string2, ignoreCase, CultureInfo.InvariantCulture) == 0;
		}

		public static string GetComponentName(IComponent component, string defaultNameValue)
		{
			Debug.Assert(component is not null, "component passed here cannot be null");
			if (string.IsNullOrEmpty(defaultNameValue))
			{
				return component.Site?.Name ?? string.Empty;
			}
			else
			{
				return defaultNameValue;
			}
		}

		/// <devdoc>
		///     This is a ControlCollection which can be made readonly.  In readonly mode, this
		///     ControlCollection throws NotSupportedExceptions for any operation that attempts
		///     to modify the collection.
		/// </devdoc>
		internal class ReadOnlyControlCollection : Control.ControlCollection
		{

			private readonly bool _isReadOnly;


			public ReadOnlyControlCollection(Control owner, bool isReadOnly) : base(owner)
			{
				_isReadOnly = isReadOnly;
			}

			public override void Add(Control value)
			{
				if (IsReadOnly)
				{
					throw new NotSupportedException(SR.ReadonlyControlsCollection);
				}
				AddInternal(value);
			}

			internal virtual void AddInternal(Control value)
			{
				base.Add(value);
			}

			public override void Clear()
			{
				if (IsReadOnly)
				{
					throw new NotSupportedException(SR.ReadonlyControlsCollection);
				}
				base.Clear();
			}

			internal virtual void RemoveInternal(Control value)
			{
				base.Remove(value);
			}

			public override void RemoveByKey(string key)
			{
				if (IsReadOnly)
				{
					throw new NotSupportedException(SR.ReadonlyControlsCollection);
				}
				base.RemoveByKey(key);
			}

			public override bool IsReadOnly
			{
				get { return _isReadOnly; }
			}
		}
	}
}
