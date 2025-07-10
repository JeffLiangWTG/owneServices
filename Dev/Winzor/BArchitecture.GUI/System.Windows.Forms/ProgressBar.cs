namespace System.Windows.Forms;

public partial class ProgressBar : Control
{
	public override bool UseParentDivForLayout => false;

	int _value;
	int minimum;
	int maximum = 100;

	public int Value
	{
		get => _value;
		set
		{
			if (value < minimum || value > maximum)
			{
				throw new ArgumentOutOfRangeException(nameof(value), value, string.Format(SR.InvalidBoundArgument, nameof(Value), value, "'minimum'", "'maximum'"));
			}
			UpdateProperty(ref _value, value);
		}
	}

	public int Minimum
	{
		get => minimum;
		set
		{
			if (minimum != value)
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException(nameof(value), value, string.Format(SR.InvalidLowBoundArgumentEx, nameof(Minimum), value, 0));
				}

				if (maximum < value)
				{
					maximum = value;
				}

				minimum = value;

				if (_value < minimum)
				{
					_value = minimum;
				}
			}
		}
	}

	public int Maximum
	{
		get => maximum;
		set
		{
			if (maximum != value)
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException(nameof(value), value, string.Format(SR.InvalidLowBoundArgumentEx, nameof(Maximum), value, 0));
				}

				if (minimum > value)
				{
					minimum = value;
				}

				maximum = value;

				if (_value > maximum)
				{
					_value = maximum;
				}
			}
		}
	}

	public int Step { get; set; } = 10;

	public ProgressBarStyle Style { get; set; }

	public void PerformStep()
	{
		Increment(Step);
	}

	public void Increment(int value)
	{
		var newValue = Value + value;

		if (newValue < minimum)
		{
			newValue = minimum;
		}

		if (newValue > maximum)
		{
			newValue = maximum;
		}
		Value = newValue;
	}

	protected virtual string ProgressBarStyleString
	{
		get
		{
			var progressBarStyleString = string.Empty;
			var value = Maximum > 0 ? Value * 100L / Maximum : 0;
			progressBarStyleString += $"width:{value}%;";
			return progressBarStyleString;
		}
	}
}
