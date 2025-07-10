using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class RefShippingLineForm : ZTemplateForm
	{
		public RefShippingLineForm(RefShippingLine shippingLine)
			: base(shippingLine)
		{
			InitializeComponent();
			this.BindingContextChanged += RefShippingLineForm_BindingContextChanged;
			MainTabPage.RunWhenBindingOrFirstShown(new EventHandler(SetTextToolTip));
		}

		void RSL_OceanCarrierMessagingAvailableCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			if ((BusinessEntity as RefShippingLine)?.RSL_OceanCarrierMessagingAvailable ?? false)
			{
				MessagingRequirementsTabPage.TabVisible = true;
			}
			else
			{
				MessagingRequirementsTabPage.TabVisible = false;
			}
		}

		void RefShippingLineForm_BindingContextChanged(object sender, EventArgs e)
		{
			if (!(BusinessEntity as RefShippingLine)?.RSL_OceanCarrierMessagingAvailable ?? true)
			{
				MessagingRequirementsTabPage.TabVisible = false;
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1046", Justification = "The control is not a button")]
		void SetTextToolTip(object sender, EventArgs e)
		{
			ToolTipService.SetToolTip(RSL_StandardCarrierAlphaCodeTextBox, Res.GetString("9506EC20-549C-46CB-90A2-196665EDAD44", "Standard Carrier Alpha Code"));
			ToolTipService.SetToolTip(RSL_CargoWiseOneCodeTextBox, Res.GetString("0BFD4A98-EB30-4F43-A327-2A92CBC88EDC", "{0} Code", "CargoWise"));
		}

		protected void RSL_IsActiveCheckBox_Click(object sender, EventArgs e)
		{
			var refShippingLine = (RefShippingLine)BusinessEntity;
			if (!refShippingLine.RSL_IsSystem && refShippingLine.RSL_IsActive)
			{
				Globals.Message.ShowError(RefShippingLineValidation.IsActiveErrorMessage);
				refShippingLine.RSL_IsActive = false;
			}
		}

		void AvailableIntegrationsTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.IntegrationsControl = new RefShippingLineIntegrationsControl();
			this.IntegrationsControl.SuspendLayout();
			this.AvailableIntegrationsTabPage.Controls.Add(this.IntegrationsControl);
			// 
			// IntegrationsControl
			// 
			this.IntegrationsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.IntegrationsControl, ".");
			this.IntegrationsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.IntegrationsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.IntegrationsControl.Name = "IntegrationsControl";
			this.IntegrationsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(541, 136, true);
			this.IntegrationsControl.TabIndex = 0;
			this.IntegrationsControl.ResumeLayout(true);
			this.IntegrationsControl.PerformLayout();
			this.IntegrationsControl.RSL_OceanCarrierMessagingAvailableCheckBox.CheckedChanged += RSL_OceanCarrierMessagingAvailableCheckBox_CheckedChanged;
		}

		void MessagingRequirementsTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.MessagingRequirementsControl = new RefShippingLineMessagingRequirementsControl();
			this.MessagingRequirementsControl.SuspendLayout();
			this.MessagingRequirementsTabPage.Controls.Add(this.MessagingRequirementsControl);
			// 
			// MessagingRequirementsControl
			// 
			this.MessagingRequirementsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessagingRequirementsControl, ".");
			this.MessagingRequirementsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessagingRequirementsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MessagingRequirementsControl.Name = "MessagingRequirementsControl";
			this.MessagingRequirementsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(541, 202, true);
			this.MessagingRequirementsControl.TabIndex = 0;
			this.MessagingRequirementsControl.ResumeLayout(true);
			this.MessagingRequirementsControl.PerformLayout();
		}

		void EBLProviderTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.EBLProviderControl = new RefShippingLineEBLProviderControl();
			this.EBLProviderControl.SuspendLayout();
			this.EBLProviderTabPage.Controls.Add(this.EBLProviderControl);
			// 
			// EBLProviderControl
			// 
			this.EBLProviderControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EBLProviderControl, ".");
			this.EBLProviderControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EBLProviderControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.EBLProviderControl.Name = "EBLProviderControl";
			this.EBLProviderControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(541, 202, true);
			this.EBLProviderControl.TabIndex = 0;
			this.EBLProviderControl.ResumeLayout(true);
			this.EBLProviderControl.PerformLayout();
		}
	}
}
