using System;
using Enterprise.Registry.GUI;
using Enterprise.Telematics.Business.Registry;

namespace Enterprise.Telematics.GUI.Registry
{
	public partial class OverspeedAlertConfigurationControl : RegistryZUserControl
	{
		public OverspeedAlertConfigurationControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			overspeedAlertType.ReadOnly = readOnly;
			durationInMinutes.ReadOnly = readOnly;
		}

		OverspeedAlertConfiguration BizObj => BoundBusinessObject as OverspeedAlertConfiguration;

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			EnableOrDisableDurationInMinutes();
		}

		void SelectionChanged(object sender, EventArgs e)
		{
			EnableOrDisableDurationInMinutes();
		}

		void EnableOrDisableDurationInMinutes()
		{
			if (BizObj.IsOverspeedAlertTypeShownForX)
			{
				durationInMinutes.Show();
			}
			else
			{
				durationInMinutes.Hide();
			}
		}
	}
}
