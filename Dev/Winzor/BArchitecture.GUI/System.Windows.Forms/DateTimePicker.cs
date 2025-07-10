using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms.Layout;
using Microsoft.AspNetCore.Components;

namespace System.Windows.Forms;

[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Referenced in DateTimePicker.razor")]
public partial class DateTimePicker : Control
{
	public DateTimePicker()
	{
		Calendar = new MonthCalendar() { ShowTime = false };
		Calendar.DateChanged += (sender, value) => Value = Clamp(value.End);
		Controls.Add(Calendar);
	}

	readonly bool validTime = true;
	int prefHeightCache = -1;
	bool userHasSetValue;
	string? customFormat;
	DateTimePickerFormat format;
	DateTime value = DateTime.Now;
	readonly DateTime creationTime = DateTime.Now;
	DateTime max = DateTime.MaxValue;
	DateTime min = DateTime.MinValue;
	protected MonthCalendar Calendar;
	[WinFormApi]
	public static readonly DateTime MinDateTime = new DateTime(1753, 1, 1);
	[WinFormApi]
	public static readonly DateTime MaxDateTime = new DateTime(9998, 12, 31);

	[WinFormApi]
	public bool Checked { get; set; }
	[WinFormApi]
	public EventHandler? ValueChanged { get; set; }
	[WinFormApi]
	public virtual bool RightToLeftLayout { get; set; }
	protected override Size DefaultSize => new Size(200, PreferredHeight);

	/// <summary>
	///  Returns the current value of the format property. This determines the
	///  style of format the date is displayed in.
	/// </summary>
	[WinFormApi]
	public DateTimePickerFormat Format
	{
		get
		{
			return format;
		}

		set
		{
			//valid values are 0x1, 0x2,0x4,0x8. max number of bits on at a time is 1
			if (!ClientUtils.IsEnumValid(value, (int)value, (int)DateTimePickerFormat.Long, (int)DateTimePickerFormat.Custom, /*maxNumberOfBitsOn*/1))
			{
				throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(DateTimePickerFormat));
			}

			if (format != value)
			{
				format = value;
			}
		}
	}

	/// <summary>
	///  Returns the custom format.
	/// </summary>
	[WinFormApi]
	public string? CustomFormat
	{
		get
		{
			return customFormat;
		}

		set
		{
			if ((value != null && !value.Equals(customFormat)) ||
				(value == null && customFormat != null))
			{
				customFormat = value;

				if (IsHandleCreated)
				{
					if (format == DateTimePickerFormat.Custom)
					{
					}
				}
			}
		}
	}
	async Task UpdateDateAsync(ChangeEventArgs e)
	{
		if (e.Value is null)
		{
			return;
		}

		await InvokeWinzorDispatcherAsync(() =>
		{
			if (DateTime.TryParse(e.Value.ToString(), CultureInfo.CurrentCulture, out var date))
			{
				Value = date;
			}
		});
	}

	DateTime Clamp(DateTime date)
	{
		if (date < MinDate)
		{
			return new DateTime(MinDate.Year, date.Month, date.Day);
		}
		else if (date > MaxDate)
		{
			return new DateTime(MaxDate.Year, date.Month, date.Day);
		}

		return date;
	}

	async Task OnBlurAsync()
	{
		await InvokeWinzorDispatcherAsync(() =>
		{
			UpdateProperty(ref value, Clamp(value));
		});
	}

	/// <summary>
	///  Indicates the DateTime value assigned to the control.
	/// </summary>
	[WinFormApi]
	public DateTime Value
	{
		get
		{
			if (!userHasSetValue && validTime)
			{
				return creationTime;
			}

			return value;
		}
		set
		{
			bool valueChanged = !DateTime.Equals(Value, value);

			if (!userHasSetValue || valueChanged)
			{
				string text = Text;
				UpdateProperty(ref this.value, value);
				userHasSetValue = true;

				if (!text.Equals(Text))
				{
					OnTextChanged(EventArgs.Empty);
				}
			}
		}
	}

	/// <summary>
	///  Indicates the maximum date and time
	///  selectable in the control.
	/// </summary>
	[WinFormApi("https://github.com/dotnet/winforms/blob/release/3.0/src/System.Windows.Forms/src/System/Windows/Forms/DateTimePicker.cs#L720")]
	public DateTime MaxDate
	{
		get => EffectiveMaxDate(max);
		set
		{
			if (value != max)
			{
				if (value < EffectiveMinDate(min))
				{
					throw new ArgumentOutOfRangeException(nameof(value), "MaxDate cannot be less than MinDate.");
				}

				if (value > MaximumDateTime)
				{
					throw new ArgumentOutOfRangeException(nameof(value), $"MaxDate cannot be greater than {MaximumDateTime}.");
				}

				max = value;
				if (Value > max)
				{
					Value = max;
				}
			}
		}
	}

	/// <summary>
	///  Specifies the maximum date value. This property is read-only.
	/// </summary>
	[WinFormApi("https://github.com/dotnet/winforms/blob/release/3.0/src/System.Windows.Forms/src/System/Windows/Forms/DateTimePicker.cs#L755")]
	public static DateTime MaximumDateTime
	{
		get
		{
			DateTime maxSupportedDateTime = CultureInfo.CurrentCulture.Calendar.MaxSupportedDateTime;
			if (maxSupportedDateTime.Year > MaxDateTime.Year)
			{
				return MaxDateTime;
			}
			return maxSupportedDateTime;
		}
	}

	/// <summary>
	///  Indicates the minimum date and time
	///  selectable in the control.
	/// </summary>
	[WinFormApi("https://github.com/dotnet/winforms/blob/release/3.0/src/System.Windows.Forms/src/System/Windows/Forms/DateTimePicker.cs#L776")]
	public DateTime MinDate
	{
		get => EffectiveMinDate(min);
		set
		{
			if (value != min)
			{
				if (value > EffectiveMaxDate(max))
				{
					throw new ArgumentOutOfRangeException(nameof(value), "MinDate cannot be greater than MaxDate.");
				}

				if (value < MinimumDateTime)
				{
					throw new ArgumentOutOfRangeException(nameof(value), $"MinDate cannot be less than {MinimumDateTime}.");
				}

				min = value;
				if (Value < min)
				{
					Value = min;
				}
			}
		}
	}

	// We restrict the available dates to >= 1753 because of oddness in the Gregorian calendar about
	// that time.  We do this even for cultures that don't use the Gregorian calendar -- we're not
	// really that worried about calendars for >250 years ago.
	//
	/// <summary>
	///  Specifies the minimum date value. This property is read-only.
	/// </summary>
	[WinFormApi("https://github.com/dotnet/winforms/blob/release/3.0/src/System.Windows.Forms/src/System/Windows/Forms/DateTimePicker.cs#L815")]
	public static DateTime MinimumDateTime
	{
		get
		{
			DateTime minSupportedDateTime = CultureInfo.CurrentCulture.Calendar.MinSupportedDateTime;
			if (minSupportedDateTime.Year < 1753)
			{
				return new DateTime(1753, 1, 1);
			}
			return minSupportedDateTime;
		}
	}

	//Make sure the passed in minDate respects the current culture: this
	//is especially important if the culture changes from a Gregorian or
	//other calendar with a lowish minDate (see comment on MinimumDateTime)
	//to a calendar, which has a minimum date of 1/1/1912.
	[WinFormApi("https://github.com/dotnet/winforms/blob/release/3.0/src/System.Windows.Forms/src/System/Windows/Forms/DateTimePicker.cs#L688")]
	static internal DateTime EffectiveMinDate(DateTime minDate)
	{
		DateTime minSupportedDate = DateTimePicker.MinimumDateTime;
		if (minDate < minSupportedDate)
		{
			return minSupportedDate;
		}
		return minDate;
	}

	//Similarly, make sure the maxDate respects the current culture.  No
	//problems are anticipated here: I don't believe there are calendars
	//around that have max dates on them.  But if there are, we'll deal with
	//them correctly.
	[WinFormApi("https://github.com/dotnet/winforms/blob/release/3.0/src/System.Windows.Forms/src/System/Windows/Forms/DateTimePicker.cs#L702")]
	static internal DateTime EffectiveMaxDate(DateTime maxDate)
	{
		DateTime maxSupportedDate = DateTimePicker.MaximumDateTime;
		if (maxDate > maxSupportedDate)
		{
			return maxSupportedDate;
		}
		return maxDate;
	}

	/// <summary>
	///  Raises the <see cref='ValueChanged'/> event.
	/// </summary>
	protected virtual void OnValueChanged(EventArgs eventargs)
	{
		ValueChanged?.Invoke(this, eventargs);
	}

	// GetPreferredSize and SetBoundsCore call this method to allow controls to self impose
	// constraints on their size.
	internal override Rectangle ApplyBoundsConstraints(int suggestedX, int suggestedY, int proposedWidth, int proposedHeight)
	{
		// Lock DateTimePicker to its preferred height.
		return base.ApplyBoundsConstraints(suggestedX, suggestedY, proposedWidth, PreferredHeight);
	}

	internal override Size GetPreferredSizeCore(Size proposedConstraints)
	{
		int height = PreferredHeight;
		int width = CommonProperties.GetSpecifiedBounds(this).Width;
		return new Size(width, height);
	}

	/// <summary>
	///  Overrides Text to allow for setting of the value via a string.  Also, returns
	///  a formatted Value when getting the text.  The DateTime class will throw
	///  an exception if the string (value) being passed in is invalid.
	/// </summary>
	[AllowNull]
	[WinFormApi]
	public override string Text
	{
		get
		{
			return base.Text;
		}
		set
		{
			// Clause to check length
			//
			if (value == null || value.Length == 0)
			{
				ResetValue();
			}
			else
			{
				// this different from winform we use TryParse instead of Parse
				if (DateTime.TryParse(value, CultureInfo.CurrentCulture, out var date))
				{
					Value = date;
				}
			}
		}
	}

	/// <summary>
	///  Resets the <see cref='Value'/> property to its default value.
	/// </summary>
	void ResetValue()
	{
		// If ShowCheckBox = true, then userHasSetValue can be false (null value).
		// otherwise, userHasSetValue is valid...
		// userHasSetValue = !ShowCheckBox;

		// After ResetValue() the flag indicating whether the user
		// has set the value should be false.
		userHasSetValue = false;

		OnValueChanged(EventArgs.Empty);
		OnTextChanged(EventArgs.Empty);
	}

	/// <summary>
	///  Indicates the preferred height of the DateTimePicker control. This property is read-only.
	/// </summary>
	[WinFormApi]
	public int PreferredHeight
	{
		get
		{
			if (prefHeightCache > -1)
			{
				return prefHeightCache;
			}

			// Base the preferred height on the current font
			int height = FontHeight;

			// Adjust for the border
			height += SystemInformation.BorderSize.Height * 4 + 3;
			prefHeightCache = (short)height;

			return height;
		}
	}

	public async Task ShowCalendarAsync()
	{
		Calendar.SetDate(Value);
		await Calendar.ShowCalendarAsync();
	}

	string FormatDateTime(DateTime dateTime, DateTimePickerFormat dateTimePickerFormat)
	{
		var dateTimeFormat = dateTimePickerFormat switch
		{
			//Winform will cached current format as custom format we use
			//default format for now need refactor this later
			//Also long format has strange format Wednesday, April 7, 1999
			//need to add here later. we dont direct use dateTimepicker format
			//because its not pad with zeros and can't dispaly in html format
			_ when dateTimePickerFormat == DateTimePickerFormat.Custom => "yyyy-MM-dd",
			_ when dateTimePickerFormat == DateTimePickerFormat.Time => "hh:mm:ss tt",
			_ => "yyyy-MM-dd"
		};

		return dateTime.ToString(dateTimeFormat);
	}
}
