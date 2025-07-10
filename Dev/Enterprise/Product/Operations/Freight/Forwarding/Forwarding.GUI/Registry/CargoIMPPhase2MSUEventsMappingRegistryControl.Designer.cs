namespace Enterprise.Freight.Forwarding.GUI
{
	partial class CargoIMPPhase2MSUEventsMappingRegistryControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.TriggerConditionValueColumnStyleInfo triggerConditionValueColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.TriggerConditionValueColumnStyleInfo();
			this.CargoIMPPhase2MSUEventsMappingGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CargoIMPPhase2MSUEventsMappingGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// CargoIMPPhase2MSUEventsMappingGrid
			// 
			this.CargoIMPPhase2MSUEventsMappingGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CargoIMPPhase2MSUEventsMappingGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.CargoIMPPhase2MSUEventsMappingGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("CargoIMPPhase2MSUEventsMappingRegistryControl|0a6ee716-9b02-45c7-b650-b28a7ad7e1a4", "CargoIMP Event");
			zDropEditColumnStyleInfo1.ColumnName = "CargoIMPPhase2MSUEvent";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("CargoIMPPhase2MSUEventsMappingRegistryControl|09d28f91-7d0d-4e63-b110-ec95d3377588", "CargoIMP Event Desc.");
			zTextBoxColumnStyleInfo1.ColumnName = "CargoIMPPhase2MSUEventDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("CargoIMPPhase2MSUEventsMappingRegistryControl|7c31d1dc-fd3e-4938-8136-fcfb9cbcca4d", "CW1 Event", "The Current Application Event");
			zDropEditColumnStyleInfo2.ColumnName = "EnterpriseEvent";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("CargoIMPPhase2MSUEventsMappingRegistryControl|8b742594-5556-4b0d-a544-c552d3365fb0", "CW1 Event Desc.", "The Current Application Event Description", "");
			zTextBoxColumnStyleInfo2.ColumnName = "EnterpriseEventDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			triggerConditionValueColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("CargoIMPPhase2MSUEventsMappingRegistryControl|263560d2-1da4-4524-9f9f-494f5680c2cb", "CW1 Event Reference.", "The Current Application Event Reference", "");
			triggerConditionValueColumnStyleInfo1.ColumnName = "EnterpriseEventReference";
			triggerConditionValueColumnStyleInfo1.FieldTypeColumnName = "EnterpriseEventReferenceType";
			triggerConditionValueColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.CargoIMPPhase2MSUEventsMappingGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.CargoIMPPhase2MSUEventsMappingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CargoIMPPhase2MSUEventsMappingGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.CargoIMPPhase2MSUEventsMappingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.CargoIMPPhase2MSUEventsMappingGrid.ColumnStyles.Add(triggerConditionValueColumnStyleInfo1);
			this.CargoIMPPhase2MSUEventsMappingGrid.GridId = "c5a2af16-b862-4d63-80d7-979aec4d62fe";
			this.CargoIMPPhase2MSUEventsMappingGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CargoIMPPhase2MSUEventsMappingGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CargoIMPPhase2MSUEventsMappingGrid.LayoutKey = "CargoIMPPhase2MSUEventsMappingGrid";
			this.CargoIMPPhase2MSUEventsMappingGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CargoIMPPhase2MSUEventsMappingGrid.Name = "CargoIMPPhase2MSUEventsMappingGrid";
			this.CargoIMPPhase2MSUEventsMappingGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(553, 396, true);
			this.CargoIMPPhase2MSUEventsMappingGrid.TabIndex = 0;
			// 
			// CargoIMPPhase2MSUEventsMappingRegistryControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CargoIMPPhase2MSUEventsMappingGrid);
			this.Name = "CargoIMPPhase2MSUEventsMappingRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(553, 396, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CargoIMPPhase2MSUEventsMappingGrid)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

		internal Enterprise.ZArchitecture.ZGrid CargoIMPPhase2MSUEventsMappingGrid;
	}
}
