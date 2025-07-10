using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using WinzorFramework.Extensions;

namespace System.Windows.Forms;

public partial class NumericUpDown : UpDownBase, ISupportInitialize
{
	public NumericUpDown() : base()
	{
		Text = "0";
	}

	public override bool UseParentDivForLayout => false;

	public bool ThousandsSeparator
	{
		get => thousandsSeparator;
		set
		{
			thousandsSeparator = value;
			Value = this.value;
		}
	}
	bool thousandsSeparator;

	public decimal Value
	{
		get => value;
		set
		{
			var processedValue = Math.Clamp(value, Minimum, Maximum);

			if (UpdateProperty(ref this.value, processedValue))
			{
				ValueChanged?.Invoke(this, new EventArgs());
			}
			else if (value < Minimum || value > Maximum)
			{
				NotifyRenderRequired();
			}

			valueText = Value.ToString($"{(ThousandsSeparator ? "N" : "F")}{DecimalPlaces}", CultureInfo.CurrentCulture);
			base.Text = valueText;
		}
	}

	decimal value = 0;

	[AllowNull]
	public override string Text
	{
		get => base.Text;
		set
		{
			base.Text = value;

			if (string.IsNullOrEmpty(value))
			{
				valueText = string.Empty;
			}
			else if (decimal.TryParse(value, out var numericValue))
			{
				Value = numericValue;
			}
		}
	}

		public event EventHandler? ValueChanged;

	public decimal Minimum { get; set; } = 0;

	public decimal Maximum { get; set; } = 100;

	public int DecimalPlaces { get; set; }

	public decimal Increment { get; set; } = 1;

	protected internal override string ControlStyleString => base.ControlStyleString + this.TextAlign();

	public override void UpButton()
	{
		Value += Increment;
	}

	public override void DownButton()
	{
		Value -= Increment;
	}

	public void BeginInit()
	{
	}

	public void EndInit()
	{
	}
}
