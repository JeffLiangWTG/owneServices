using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using WinzorFramework.Extensions;
namespace System.Windows.Forms;

public partial class LinkLabel : Label, IButtonControl
{
	public LinkLabel()
	{
		ResetLinkArea();
	}

	public LinkCollection Links
	{
		get => links ??= new LinkCollection(this);
		set
		{
			links = value;
			ValidateNoOverlappingLinks();
		}
	}

	LinkCollection? links;

	public LinkBehavior LinkBehavior { get; set; }

	public LinkArea LinkArea {
		get
		{
			if (Links.Count == 0)
			{
				return new LinkArea(0, 0);
			}

			return new LinkArea(Links[0].Start, Links[0].Length);
		}
		set
		{
			Links.Clear();

			if (!value.IsEmpty)
			{
				if (value.Start < 0)
				{
					throw new ArgumentOutOfRangeException(nameof(LinkArea), value, SR.LinkLabelAreaStart);
				}

				if (value.Length < -1)
				{
					throw new ArgumentOutOfRangeException(nameof(LinkArea), value, SR.LinkLabelAreaLength);
				}

				if (value.Start != 0 || !value.IsEmpty)
				{
					Links.Add(new Link(this));
					Links[0].Start = value.Start;
					Links[0].Length = value.Length;
				}
			}

			UpdateSelectability();
		}
	}

	public Color LinkColor
	{
		get => linkColor.IsEmpty ? Color.Blue : linkColor;
		set => UpdateProperty(ref linkColor, value);
	}
	Color linkColor;

	public Color DisabledLinkColor
	{
		get => disabledLinkColor.IsEmpty ? Color.Red : disabledLinkColor;
		set => UpdateProperty(ref disabledLinkColor, value);
	}
	Color disabledLinkColor = Color.FromArgb(106, 107, 102);

	public Color ActiveLinkColor
	{
		get => activeLinkColor.IsEmpty ? Color.Red : activeLinkColor;
		set => UpdateProperty(ref activeLinkColor, value);
	}
	Color activeLinkColor;

	public Color VisitedLinkColor
	{
		get => visitedLinkColor.IsEmpty ? Color.Purple : visitedLinkColor;
		set => UpdateProperty(ref visitedLinkColor, value);
	}
	Color visitedLinkColor;

	public object? LinkData { get; set; }

	public bool LinkVisited { get; set; }

	public DialogResult DialogResult { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

	Link? focusLink;

	Link? FocusLink
	{
		get
		{
			return focusLink;
		}
		set
		{
			if (focusLink != value)
			{
				if (focusLink != null)
				{
					InvalidateLink(focusLink);
				}

				focusLink = value;

				if (focusLink != null)
				{
					InvalidateLink(focusLink);
				}
			}
		}
	}

	/// <summary>
	///  Invalidates only the portions of the text that is linked to
	///  the specified link. If link is null, then all linked text
	///  is invalidated.
	/// </summary>
	void InvalidateLink(Link link)
	{
		if (IsHandleCreated)
		{
			if (link == null || link.VisualRegion == null || IsOneLink())
			{
				Invalidate();
			}
			else
			{
				Invalidate(link.VisualRegion);
			}
		}
	}

	/// <summary>
	///  Determines whether the whole link label contains only one link,
	///  and the link runs from the beginning of the label to the end of it
	/// </summary>
	bool IsOneLink()
	{
		if (links == null || links.Count != 1 || Text == null)
		{
			return false;
		}
		var stringInfo = new StringInfo(Text);
		if (LinkArea.Start == 0 && LinkArea.Length == stringInfo.LengthInTextElements)
		{
			return true;
		}
		return false;
	}

	void ResetLinkArea()
	{
		LinkArea = new LinkArea(0, -1);
	}

	bool LinkAreaIsDefault => Links.Count == 1 && Links[0].Start == 0 && Links[0].Length == -1;

	public void NotifyDefault(bool value)
	{
	}

	public void PerformClick()
	{
		if (FocusLink is null)
		{
			FocusLink = Links.FirstOrDefault(l => l.Enabled);
		}

		if (FocusLink is not null)
		{
			OnLinkClicked(new LinkLabelLinkClickedEventArgs(FocusLink));
		}
	}

	protected virtual void OnLinkClicked(LinkLabelLinkClickedEventArgs e)
	{
		LinkClicked?.Invoke(this, e);
	}

	public event LinkLabelLinkClickedEventHandler? LinkClicked;

	protected override void OnTextChanged(EventArgs e)
	{
		base.OnTextChanged(e);
		UpdateSelectability();
	}

	protected override void OnFontChanged(EventArgs e)
	{
		base.OnFontChanged(e);
		UpdateSelectability();
	}

	/// <summary>
	///  Invalidates only the portions of the text that is linked to the specified link. If link is null, then
	///  all linked text is invalidated.
	/// </summary>
	protected override void OnKeyDown(KeyEventArgs e)
	{
		base.OnKeyDown(e);

		if (e.KeyCode == Keys.Enter)
		{
			if (FocusLink is not null && FocusLink.Enabled)
			{
				OnLinkClicked(new LinkLabelLinkClickedEventArgs(FocusLink));
			}
		}
	}

	bool processingOnGotFocus;  // used to avoid raising the OnGotFocus event twice after selecting a focus link.

	protected override void OnGotFocus(EventArgs e)
	{
		if (!processingOnGotFocus)
		{
			base.OnGotFocus(e);
			processingOnGotFocus = true;
		}

		try
		{
			if (FocusLink is null)
			{
				// Set focus on first link.
				// This will raise the OnGotFocus event again but it will not be processed because processingOnGotFocus is true.
				Select(directed: true, forward: true);
			}
			else
			{
				InvalidateLink(FocusLink);
			}
		}
		finally
		{
			processingOnGotFocus = false;
		}
	}

	void ValidateNoOverlappingLinks()
	{
		for (int x = 0; x < Links.Count; x++)
		{
			Link left = Links[x];
			if (left.Length < 0)
			{
				throw new InvalidOperationException("Overlapping link regions");
			}

			for (int y = x; y < Links.Count; y++)
			{
				if (x != y)
				{
					Link right = Links[y];
					int maxStart = Math.Max(left.Start, right.Start);
					int minEnd = Math.Min(left.Start + left.Length, right.Start + right.Length);
					if (maxStart < minEnd)
					{
						throw new InvalidOperationException("Overlapping link regions");
					}
				}
			}
		}
	}

	[Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Referenced in LinkLabel.razor")]
	string LinkLabelLinkBehaviorClass
	{
		get => this.LinkBehaviorClass();
	}

	void UpdateSelectability()
	{
		bool selectable = false;
		int charEnd;
		int charStart;
		foreach (var link in Links)
		{
			charEnd = Text.Length;
			charStart = link.Start + link.Length;
			if (link.Enabled && LinkInText(link.Start, charEnd - charStart))
			{
				selectable = true;
				break;
			} 
		}
		TabStop = selectable;
		SetStyle(ControlStyles.Selectable, selectable);
	}
	bool LinkInText(int start, int length) => start >= 0 && start < Text.Length && length > 0;

	/// <summary>
	///  Processes a dialog key. This method is called during message pre-processing
	///  to handle dialog characters, such as TAB, RETURN, ESCAPE, and arrow keys. This
	///  method is called only if the isInputKey() method indicates that the control
	///  isn't interested in the key. processDialogKey() simply sends the character to
	///  the parent's processDialogKey() method, or returns false if the control has no
	///  parent. The Form class overrides this method to perform actual processing
	///  of dialog keys. When overriding processDialogKey(), a control should return true
	///  to indicate that it has processed the key. For keys that aren't processed by the
	///  control, the result of "base.processDialogChar()" should be returned. Controls
	///  will seldom, if ever, need to override this method.
	/// </summary>
	protected override bool ProcessDialogKey(Keys keyData)
	{
		if ((keyData & (Keys.Alt | Keys.Control)) != Keys.Alt)
		{
			var keyCode = keyData & Keys.KeyCode;
			switch (keyCode)
			{
				case Keys.Tab:
					if (TabStop)
					{
						var forward = (keyData & Keys.Shift) != Keys.Shift;
						if (FocusNextLink(forward))
						{
							return true;
						}
					}
					break;

				case Keys.Up:
				case Keys.Left:
					if (FocusNextLink(false))
					{
						return true;
					}
					break;

				case Keys.Down:
				case Keys.Right:
					if (FocusNextLink(true))
					{
						return true;
					}
					break;
			}
		}
		return base.ProcessDialogKey(keyData);
	}

	bool FocusNextLink(bool forward)
	{
		var focusIndex = -1;
		if (FocusLink is not null)
		{
			for (var i = 0; i < Links.Count; i++)
			{
				if (Links[i] == FocusLink)
				{
					focusIndex = i;
					break;
				}
			}
		}

		focusIndex = GetNextLinkIndex(focusIndex, forward);
		if (focusIndex != -1)
		{
			FocusLink = Links[focusIndex];
			return true;
		}
		else
		{
			FocusLink = null;
			return false;
		}
	}

	int GetNextLinkIndex(int focusIndex, bool forward)
	{
		Link? test;
		var text = Text;
		var charStart = 0;
		var charEnd = 0;

		if (forward)
		{
			do
			{
				focusIndex++;
				if (focusIndex < Links.Count)
				{
					test = Links[focusIndex];
					charStart = ConvertToCharIndex(test.Start, text);
					charEnd = ConvertToCharIndex(test.Start + test.Length, text);
				}
				else
				{
					test = null;
				}
			} while (test is not null && !test.Enabled && LinkInText(charStart, charEnd - charStart));
		}
		else
		{
			do
			{
				focusIndex--;
				if (focusIndex >= 0)
				{
					test = Links[focusIndex];
					charStart = ConvertToCharIndex(test.Start, text);
					charEnd = ConvertToCharIndex(test.Start + test.Length, text);
				}
				else
				{
					test = null;
				}
			} while (test is not null && !test.Enabled && LinkInText(charStart, charEnd - charStart));
		}

		return focusIndex < 0 || focusIndex >= Links.Count ? -1 : focusIndex;
	}

	/// <summary>
	///  Converts the character index into char index of the string.
	/// </summary>
	/// <remarks>
	///  <para>
	///   This method mainly deal with surrogate. Suppose we have a string consisting of 3 surrogates, and we want the
	///   second character, then the index we need should be 2 instead of 1, and this method returns the correct index.
	///  </para>
	/// </remarks>
	static int ConvertToCharIndex(int index, string text)
	{
		// This method is copied in LinkCollectionEditor. Update the other one as well if you change this method.

		if (index <= 0)
		{
			return 0;
		}

		if (string.IsNullOrEmpty(text))
		{
			Debug.Assert(text is not null, "string should not be null");

			return index;
		}

		// Dealing with surrogate characters in some languages, characters can expand over multiple
		// chars, using StringInfo lets us properly deal with it.
		StringInfo stringInfo = new(text);
		var numTextElements = stringInfo.LengthInTextElements;

		if (index > numTextElements)
		{
			// Pretend all the characters after are ASCII characters
			return index - numTextElements + text.Length;
		}

		var sub = stringInfo.SubstringByTextElements(0, index);
		return sub.Length;
	}

	protected override void Select(bool directed, bool forward)
	{
		// In a multi-link label, if the tab came from another control, we want to keep the currently
		// focused link, otherwise, we set the focus to the next link.
		if (directed && Links.Count > 0)
		{
			var focusIndex = -1;
			if (FocusLink is not null)
			{
				focusIndex = Links.IndexOf(FocusLink);
			}

			FocusLink = null;

			var newFocus = GetNextLinkIndex(focusIndex, forward);
			if (newFocus == -1)
			{
				if (forward)
				{
					newFocus = GetNextLinkIndex(-1, forward);
				}
				else
				{
					newFocus = GetNextLinkIndex(Links.Count, forward);
				}
			}

			if (newFocus != -1)
			{
				FocusLink = Links[newFocus];
			}
		}

		base.Select(directed, forward);
	}
}
