using System.Drawing;
using System.Drawing.Imaging;

#nullable disable

namespace System.Windows.Forms;

public class DataGridCaption : Control
{
	readonly DataGrid dataGrid;
	static readonly ColorMap[] colorMap = new ColorMap[] { new ColorMap() };

	internal DataGridCaption(DataGrid dataGrid)
	{
		this.dataGrid = dataGrid;
		downButtonVisible = dataGrid.ParentRowsVisible;
		colorMap[0].OldColor = Color.White;
		colorMap[0].NewColor = ForeColor;
		OnGridFontChanged();
	}

	internal void OnGridFontChanged()
	{
		if (dataGridFont == null || !dataGridFont.Equals(dataGrid.Font))
		{
			try
			{
				dataGridFont = new Font(dataGrid.Font, FontStyle.Bold);
			}
			catch
			{
			}
		}
	}

	internal bool BackButtonActive
	{
		get => backActive;
		set => UpdateProperty(ref backActive, value);
	}
	bool backActive;

	internal bool DownButtonActive
	{
		get => downActive;
		set => UpdateProperty(ref downActive, value);
	}
	bool downActive;

	public override Color BackColor
	{
		get => backColor;
		set
		{
			if (!backColor.Equals(value))
			{
				if (value.IsEmpty)
				{
					throw new ArgumentException(string.Format(SR.DataGridEmptyColor, "Caption BackColor"));
				}

				backColor = value;
				NotifyRenderRequired();
			}
		}
	}
	Color backColor = DefaultBackColor;
	internal static new Color DefaultBackColor => SystemColors.ActiveCaption;

	internal new Font Font
	{
		get => textFont ?? dataGridFont;
		set
		{
			if (textFont == null || !textFont.Equals(value))
			{
				textFont = value;
				// this property gets called in the constructor before dataGrid has a caption
				// and we don't need this special-handling then...
				if (dataGrid.Caption != null)
				{
					dataGrid.RecalculateFonts();
					dataGrid.PerformLayout();
					dataGrid.Invalidate(); // smaller invalidate rect?
				}
			}
}
	}
	Font dataGridFont;
	Font textFont;

	public override string Text
	{
		get => base.Text;
		set => base.Text = value ?? string.Empty;
	}

	internal bool BackButtonVisible
	{
		get => backButtonVisible;
		set => UpdateProperty(ref backButtonVisible, value);
	}
	bool backButtonVisible;

	internal bool DownButtonVisible
	{
		get => downButtonVisible;
		set => UpdateProperty(ref downButtonVisible, value);
	}
	bool downButtonVisible;

	internal void SetDownButtonDirection(bool pointDown)
	{
		DownButtonDown = pointDown;
	}

	bool DownButtonDown
	{
		set => UpdateProperty(ref downButtonDown, value);
	}
	bool downButtonDown;
}
