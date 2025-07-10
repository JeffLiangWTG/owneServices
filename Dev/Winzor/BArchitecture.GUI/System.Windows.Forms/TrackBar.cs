using System.ComponentModel;

namespace System.Windows.Forms;

[Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Referenced in TrackBar.razor")]
public partial class TrackBar : Control, ISupportInitialize
{
	int largeChange = 5;
	int maximum = 10;
	int minimum;
	int val;
	int smallChange = 1;
	int tickFrequency = 1;

	public int LargeChange
	{
		get => largeChange;
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(value), value, string.Format(SR.TrackBarLargeChangeError, value));
			}

			if (largeChange == value)
			{
				return;
			}

			largeChange = value;
		}
	}

	public int Maximum
	{
		get => maximum;
		set
		{
			if (maximum == value)
			{
				return;
			}

			if (value < minimum)
			{
				minimum = value;
			}

			SetRange(minimum, value);
		}
	}

	public int Minimum
	{
		get => minimum;
		set
		{
			if (minimum == value)
			{
				return;
			}

			if (value > maximum)
			{
				maximum = value;
			}

			SetRange(value, maximum);
		}
	}

	public int SmallChange
	{
		get => smallChange;
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(value), value, string.Format(SR.TrackBarSmallChangeError, value));
			}

			if (smallChange == value)
			{
				return;
			}

			smallChange = value;
		}
	}

	public int TickFrequency
	{
		get => tickFrequency;
		set
		{
			if (tickFrequency == value)
			{
				return;
			}

			tickFrequency = value;
		}
	}

	public TickStyle TickStyle { get; set; }

	public int Value
	{
		get
		{
			return val;
		}
		set
		{
			if (value == val)
			{
				return;
			}

			if (((value < minimum) || (value > maximum)))
			{
				throw new ArgumentOutOfRangeException(nameof(value), value, string.Format(SR.InvalidBoundArgument, nameof(Value), value, $"'{nameof(Minimum)}'", $"'${nameof(Maximum)}'"));
			}

			val = value;
			_ = InvokeWinzorDispatcherAsync(() => { OnScroll(EventArgs.Empty); }); 
		}
	}

	public event EventHandler? Scroll;

	protected virtual void OnScroll(EventArgs e)
	{
		Scroll?.Invoke(this, e);
	}

	public void BeginInit()
	{
	}

	public void EndInit()
	{
	}

	public void SetRange(int minValue, int maxValue)
	{
		if (minimum != minValue || maximum != maxValue)
		{
			if (minValue > maxValue)
			{
				maxValue = minValue;
			}

			minimum = minValue;
			maximum = maxValue;

			if (val < minimum)
			{
				val = minimum;
			}

			if (val > maximum)
			{
				val = maximum;
			}
		}
	}

	int RunnableTrackHeight => Math.Max((int)(Size.Height / 33f * 5),1);

	int RunnableTrackLeftRightMargin => Math.Max((int)(Size.Width / 226f * 6),1);

	int RunnableTrackDownMargin => (int)(Size.Height / 33f * 1);

	int SliderThumbImageWidth => Math.Max((int)(Size.Height / 33f * 12),1);

	int SliderThumbImageHeight => Math.Max((int)(Size.Height / 33f * 24),2);

	int RunnableTrackMarginTop => -(int)Math.Ceiling((SliderThumbImageHeight - RunnableTrackHeight) / 2f);

	string SliderThumbDefaultBackgroundImage => @"url(""data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' x='0px' y='0px' width='" + SliderThumbImageWidth + @"' height='" + SliderThumbImageHeight + @"' viewBox='-50 0 100 200'%3E%3Cpolygon points='0,0 50,50 50,180 -50,180 -50,50' style='fill:%230078d7;stroke:%230078d7;stroke.-width:1' /%3E%3C/svg%3E"")";

	string SliderThumbHoverBackgroundImage => @"url(""data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' x='0px' y='0px' width='" + SliderThumbImageWidth + @"' height='" + SliderThumbImageHeight + @"' viewBox='-50 0 100 200'%3E%3Cpolygon points='0,0 50,50 50,180 -50,180 -50,50' style='fill:black;stroke:black;stroke.-width:1' /%3E%3C/svg%3E"")";

	string SliderThumbDragBackgroundImage => @"url(""data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' x='0px' y='0px' width='" + SliderThumbImageWidth + @"' height='" + SliderThumbImageHeight + @"' viewBox='-50 0 100 200'%3E%3Cpolygon points='0,0 50,50 50,180 -50,180 -50,50' style='fill:%23cccccc;stroke:%23cccccc;stroke.-width:1' /%3E%3C/svg%3E"")";
}
