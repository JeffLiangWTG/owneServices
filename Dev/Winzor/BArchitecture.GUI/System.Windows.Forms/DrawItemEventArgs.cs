using System.Drawing;

#nullable disable

namespace System.Windows.Forms;

public class DrawItemEventArgs : EventArgs
{
	/// <summary>
	///  The backColor to paint each menu item with.
	/// </summary>
	readonly Color _backColor;

	/// <summary>
	///  The foreColor to paint each menu item with.
	/// </summary>
	readonly Color _foreColor;

	/// <summary>
	///  Creates a new DrawItemEventArgs with the given parameters.
	/// </summary>
	public DrawItemEventArgs(BGraphics graphics, Font font, Rectangle rect,
							 int index, DrawItemState state)
	{
		Graphics = graphics ?? throw new ArgumentNullException(nameof(graphics));
		Font = font;
		Bounds = rect;
		Index = index;
		State = state;
		_foreColor = SystemColors.WindowText;
		_backColor = SystemColors.Window;
	}

	/// <summary>
	///  Creates a new DrawItemEventArgs with the given parameters, including the foreColor and backColor of the control.
	/// </summary>
	public DrawItemEventArgs(BGraphics graphics, Font font, Rectangle rect,
							 int index, DrawItemState state, Color foreColor, Color backColor)
	{
		Graphics = graphics ?? throw new ArgumentNullException(nameof(graphics));
		Font = font;
		Bounds = rect;
		Index = index;
		State = state;
		_foreColor = foreColor;
		_backColor = backColor;
	}

	/// <summary>
	/// Creates a new DrawItemEventArgs with the given parameters.
	/// </summary>
	/// <param name="index"></param>
	/// <param name="state"></param>
	public DrawItemEventArgs(int index, DrawItemState state)
	{
		Index = index;
		State = state;
	}

	/// <summary>
	///  Graphics object with which painting should be done.
	/// </summary>
	public BGraphics Graphics { get; }

	/// <summary>
	///  A suggested font, usually the parent control's Font property.
	/// </summary>
	public Font Font { get; }

	/// <summary>
	///  The rectangle outlining the area in which the painting should be  done.
	/// </summary>
	public Rectangle Bounds { get; }

	/// <summary>
	///  The index of the item that should be painted.
	/// </summary>
	public int Index { get; }

	/// <summary>
	///  Miscellaneous state information, such as whether the item is
	///  "selected", "focused", or some other such information.  ComboBoxes
	///  have one special piece of information which indicates if the item
	///  being painted is the editable portion of the ComboBox.
	/// </summary>
	public DrawItemState State { get; }

	/// <summary>
	///  A suggested color drawing: either SystemColors.WindowText or SystemColors.HighlightText,
	///  depending on whether this item is selected.
	/// </summary>
	public Color ForeColor
	{
		get
		{
			if ((State & DrawItemState.Selected) == DrawItemState.Selected)
			{
				return SystemColors.HighlightText;
			}

			return _foreColor;
		}
	}

	public Color BackColor
	{
		get
		{
			if ((State & DrawItemState.Selected) == DrawItemState.Selected)
			{
				return SystemColors.Highlight;
			}

			return _backColor;
		}
	}

	/// <summary>
	///  Draws the background of the given rectangle with the color returned from the BackColor property.
	/// </summary>
	public virtual void DrawBackground()
	{
	}

	/// <summary>
	///  Draws a handy focus rect in the given rectangle.
	/// </summary>
	public virtual void DrawFocusRectangle()
	{
	}
}
