using System;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class TemperatureControlBlock : ZUserControl
	{
		public TemperatureControlBlock()
		{
			InitializeComponent();
		}

		public void Configure(ITemperatureControlConfiguration configuration)
		{
			TemperatureControlMaxTempDropEdit.Location = configuration.MaxTemperatureControlLocation;
			TemperatureControlMinTempDropEdit.Location = configuration.MinTemperatureControlLocation;
			TemperatureControlMaxButton.Location = configuration.SetMaxTemperatureButtonLocation;
			TemperatureControlMinButton.Location = configuration.SetMinTemperatureButtonLocation;
			Size = configuration.TotalControlSize;
		}

		public void TemperatureControlButton_SetVisibility(bool visible)
		{
			TemperatureControlMinButton.Visible = visible;
			TemperatureControlMaxButton.Visible = visible;
		}

		void TemperatureControlMinButton_Click(object sender, EventArgs e)
		{
			if (Temperature != null)
			{
				Temperature.RequiredTemperatureMinimum = Temperature.RequiredTemperatureUnit == Constants.Temperature.Centigrade
					? new ZDecimal(Constants.Temperature.Convert(0, Constants.Temperature.Kelvin, Constants.Temperature.Centigrade)).Truncate(1)
					: new ZDecimal(Constants.Temperature.Convert(0, Constants.Temperature.Kelvin, Constants.Temperature.Fahrenheit)).Truncate(1);
			}
		}

		void TemperatureControlMaxButton_Click(object sender, EventArgs e)
		{
			if (Temperature != null)
			{
				Temperature.RequiredTemperatureMaximum = 999.9;
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (CurrentDataItem != null)
			{
				TemperatureControlMaxButton.Enabled = true;
				TemperatureControlMinButton.Enabled = true;
			}
			else
			{
				TemperatureControlMaxButton.Enabled = false;
				TemperatureControlMinButton.Enabled = false;
			}
		}

		public IRequiredTemperature Temperature
		{
			get { return (IRequiredTemperature)CurrentDataItem; }
		}
	}
}
