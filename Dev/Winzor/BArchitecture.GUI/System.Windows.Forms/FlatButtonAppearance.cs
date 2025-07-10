using System.Drawing;

namespace System.Windows.Forms;

public class FlatButtonAppearance
{
	internal FlatButtonAppearance(ButtonBase owner)
	{
		this.owner = owner;
	}

	public int BorderSize
	{
		get => borderSize;
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(value), value, string.Format(SR.InvalidLowBoundArgumentEx, nameof(BorderSize), value, 0));
			}

			UpdateProperty(ref borderSize, value);
		}
	}
	int borderSize = 1;

	public Color BorderColor
	{
		get => borderColor;
		set
		{
			if (value.Equals(Color.Transparent))
			{
				throw new NotSupportedException(SR.ButtonFlatAppearanceInvalidBorderColor);
			}

			UpdateProperty(ref borderColor, value);
		}
	}
	Color borderColor = Color.Empty;

	public Color CheckedBackColor
	{
		get => checkedBackColor;
		set => UpdateProperty(ref checkedBackColor, value);
	}
	Color checkedBackColor = Color.Empty;

	public Color MouseDownBackColor
	{
		get => mouseDownBackColor;
		set => UpdateProperty(ref mouseDownBackColor, value);
	}
	Color mouseDownBackColor = Color.Empty;

	public Color MouseOverBackColor
	{
		get => mouseOverBackColor;
		set => UpdateProperty(ref mouseOverBackColor, value);
	}
	Color mouseOverBackColor = Color.Empty;

	void UpdateProperty<T>(ref T field, T value)
	{
		if (!field?.Equals(value) ?? value != null)
		{
			field = value;
			owner.FlatAppearanceUpdated();
		}
	}

	readonly ButtonBase owner;
}
