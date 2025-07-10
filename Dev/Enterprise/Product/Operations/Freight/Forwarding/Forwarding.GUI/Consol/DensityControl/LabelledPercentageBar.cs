using System.Globalization;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class LabelledPercentageBar : ZUserControl
	{
		public LabelledPercentageBar()
		{
			InitializeComponent();
		}

		public void SetValue(ZDecimal value)
		{
			var percentage = GetPercentageAsInteger(value);

			SetProgressBarValue(percentage);
			SetProgressLabel(percentage);
		}

		public void ResetValues()
		{
			ProgressValue.Text = ZString.Empty;
			ProgressBar.Value = default;
		}

		int GetPercentageAsInteger(ZDecimal value)
		{
			if (value > int.MaxValue)
			{
				return int.MaxValue;
			}
			else if (value < int.MinValue)
			{
				return 0;
			}

			return decimal.ToInt32(Utilities.Round(value, 0));
		}

		void SetProgressBarValue(int percentage)
		{
			if (percentage > 100)
			{
				ProgressBar.Value = 100;
			}
			else if (percentage < 0)
			{
				ProgressBar.Value = 0;
			}
			else
			{
				ProgressBar.Value = percentage;
			}
		}

		void SetProgressLabel(int percentage)
		{
			ProgressValue.Text = percentage != -100
				? string.Concat(percentage.ToString(CultureInfo.InvariantCulture), "%")
				: string.Empty;

			ProgressValue.BringToFront();
		}
	}
}
