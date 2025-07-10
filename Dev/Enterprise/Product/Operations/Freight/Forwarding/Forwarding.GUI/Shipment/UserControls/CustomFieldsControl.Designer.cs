using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.GUI
{
	partial class CustomFieldsControl
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.CustomFieldsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ShipmentCustomFields = new Enterprise.ZArchitecture.GUI.ProcessTemplateCustomFieldsControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CustomFieldsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.Business.ForwardingShipment);
			// 
			// CustomFieldsGroupBox
			// 
			this.CustomFieldsGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("CustomFieldsControl|9f171a80-a4ea-422f-ade3-921efdce2041", "Custom Fields");
			this.CustomFieldsGroupBox.Controls.Add(this.ShipmentCustomFields);
			this.CustomFieldsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CustomFieldsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CustomFieldsGroupBox.Name = "CustomFieldsGroupBox";
			this.CustomFieldsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 308, true);
			this.CustomFieldsGroupBox.TabIndex = 37;
			this.CustomFieldsGroupBox.TabStop = false;
			// 
			// ShipmentCustomFields
			// 
			this.ShipmentCustomFields.AllowDrop = true;
			this.ShipmentCustomFields.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ShipmentCustomFields.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ShipmentCustomFields.Name = "ShipmentCustomFields";
			this.ShipmentCustomFields.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(284, 289, true);
			this.ShipmentCustomFields.TabIndex = 20;
			// 
			// CustomFieldsControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CustomFieldsGroupBox);
			this.Name = "CustomFieldsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 308, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CustomFieldsGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox CustomFieldsGroupBox;
		private Enterprise.ZArchitecture.GUI.ProcessTemplateCustomFieldsControl ShipmentCustomFields;



	}
}
