using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Freight.Common.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Freight.Module
{
	public partial class CO2eStatusAndCO2eKgRangeNumberFilterControl : ZNumberRangeControl
	{
		public CO2eStatusAndCO2eKgRangeNumberFilterControl(ZFilterStrip filterStripControl)
			: base(filterStripControl)
		{
			InitializeComponent();
			ControlDpiScalingHelper.SetLeft(ref CO2eStatusDropEdit, filterStripControl.FilterControlsBox2Start(CO2eStatusDropEdit), true);
			SetUpEventHandlers();
		}

		void SetUpEventHandlers()
		{
			CO2eStatusDropEdit.SelectedIndexChanged += CO2eStatusChanged;
		}

		void CO2eStatusChanged(object sender, EventArgs e)
		{
			SetNumberRangeControlsReadOnly(CO2eStatusDropEdit.Text != CO2eStatusList.Codes.Current);
		}

		void SetNumberRangeControlsReadOnly(bool value)
		{
			foreach (Control control in Controls)
			{
				if (control.Name != nameof(CO2eStatusDropEdit))
				{
					control.SetReadOnly(value);
				}
			}
		}
	}
}
