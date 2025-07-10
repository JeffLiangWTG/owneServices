using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI
{
	partial class EntryInstructionDetailBasicUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.OtherPartiesDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.OtherPartiesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FromWarehouseGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FromWarehouseCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FromWarehouseAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.ToWarehouseGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ToWarehouseCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ToWarehouseAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.DetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OtherPartiesDetailsPanel.SuspendLayout();
			this.OtherPartiesGroupBox.SuspendLayout();
			this.FromWarehouseGroupBox.SuspendLayout();
			this.FromWarehouseAddressControl.SuspendLayout();
			this.ToWarehouseGroupBox.SuspendLayout();
			this.ToWarehouseAddressControl.SuspendLayout();
			this.DetailsPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.PL.Business.Declaration.JobDeclaration);
			// 
			// OtherPartiesDetailsPanel
			// 
			this.OtherPartiesDetailsPanel.AutoScroll = true;
			this.OtherPartiesDetailsPanel.Controls.Add(this.OtherPartiesGroupBox);
			this.OtherPartiesDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OtherPartiesDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 113, true);
			this.OtherPartiesDetailsPanel.Name = "OtherPartiesDetailsPanel";
			this.OtherPartiesDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1097, 185, true);
			this.OtherPartiesDetailsPanel.TabIndex = 6;
			// 
			// OtherPartiesGroupBox
			// 
			this.OtherPartiesGroupBox.CaptionResourceString = Enterprise.Customs.PL.GUI.Res.GetData("2BC1F44E-7495-4C58-849E-01BD248BAD0C", "Other Parties");
			this.OtherPartiesGroupBox.Controls.Add(this.FromWarehouseGroupBox);
			this.OtherPartiesGroupBox.Controls.Add(this.ToWarehouseGroupBox);
			this.OtherPartiesGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.OtherPartiesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OtherPartiesGroupBox.Name = "OtherPartiesGroupBox";
			this.OtherPartiesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(510, 185, true);
			this.OtherPartiesGroupBox.TabIndex = 1;
			this.OtherPartiesGroupBox.TabStop = false;
			// 
			// FromWarehouseGroupBox
			// 
			this.FromWarehouseGroupBox.CaptionResourceString = Enterprise.Customs.PL.GUI.Res.GetData("7D4EE5FA-92A4-423A-A8E7-CF8D9B6A4006", "From Warehouse");
			this.FromWarehouseGroupBox.Controls.Add(this.FromWarehouseCodeTextBox);
			this.FromWarehouseGroupBox.Controls.Add(this.FromWarehouseAddressControl);
			this.FromWarehouseGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.FromWarehouseGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 55, true);
			this.FromWarehouseGroupBox.Name = "FromWarehouseGroupBox";
			this.FromWarehouseGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(505, 41, true);
			this.FromWarehouseGroupBox.TabIndex = 4;
			this.FromWarehouseGroupBox.TabStop = false;
			// 
			// FromWarehouseCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.FromWarehouseCodeTextBox, "CustomsEntryInstructions.FromWarehouseCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.PL.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.PL.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).FromWarehouseCode)));
			this.FromWarehouseCodeTextBox.CaptionResourceString = Enterprise.Customs.PL.GUI.Res.GetData("D0456A6A-2032-4C8E-8FA0-4EBA9A0C4DDF", "From Warehouse Code");
			this.FromWarehouseCodeTextBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.FromWarehouseCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(322, 14, true);
			this.FromWarehouseCodeTextBox.Name = "FromWarehouseCodeTextBox";
			this.FromWarehouseCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 18, true);
			this.FromWarehouseCodeTextBox.TabIndex = 3;
			// 
			// FromWarehouseAddressControl
			// 
			this.FromWarehouseAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FromWarehouseAddressControl, "CustomsEntryInstructions.CEI_OA_Warehouse");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.PL.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.PL.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_OA_Warehouse)));
			this.FromWarehouseAddressControl.BindToOrgList = "Lookups.BondedWarehouseCollection";
			this.FromWarehouseAddressControl.Dock = System.Windows.Forms.DockStyle.Left;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.FromWarehouseAddressControl, false);
			this.FromWarehouseAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.FromWarehouseAddressControl.Name = "FromWarehouseAddressControl";
			this.FromWarehouseAddressControl.PopupCaption = "";
			this.FromWarehouseAddressControl.ShowAddress = false;
			this.FromWarehouseAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 24, true);
			this.FromWarehouseAddressControl.TabIndex = 0;
			// 
			// ToWarehouseGroupBox
			// 
			this.ToWarehouseGroupBox.CaptionResourceString = Enterprise.Customs.PL.GUI.Res.GetData("A3ADB719-3FF6-4792-AF98-9DDD9C74003B", "To Warehouse");
			this.ToWarehouseGroupBox.Controls.Add(this.ToWarehouseCodeTextBox);
			this.ToWarehouseGroupBox.Controls.Add(this.ToWarehouseAddressControl);
			this.ToWarehouseGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.ToWarehouseGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.ToWarehouseGroupBox.Name = "ToWarehouseGroupBox";
			this.ToWarehouseGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(505, 41, true);
			this.ToWarehouseGroupBox.TabIndex = 5;
			this.ToWarehouseGroupBox.TabStop = false;
			// 
			// ToWarehouseCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.ToWarehouseCodeTextBox, "CustomsEntryInstructions.ToWarehouseCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.PL.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.PL.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).ToWarehouseCode)));
			this.ToWarehouseCodeTextBox.CaptionResourceString = Enterprise.Customs.PL.GUI.Res.GetData("A116DF89-B075-4E4C-B19A-494E5804E302", "To Warehouse Code");
			this.ToWarehouseCodeTextBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.ToWarehouseCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(322, 14, true);
			this.ToWarehouseCodeTextBox.Name = "ToWarehouseCodeTextBox";
			this.ToWarehouseCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 18, true);
			this.ToWarehouseCodeTextBox.TabIndex = 3;
			// 
			// ToWarehouseAddressControl
			// 
			this.ToWarehouseAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ToWarehouseAddressControl, "CustomsEntryInstructions.CEI_OA_Warehouse2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.PL.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.PL.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_OA_Warehouse2)));
			this.ToWarehouseAddressControl.BindToOrgList = "Lookups.BondedWarehouseCollection";
			this.ToWarehouseAddressControl.Dock = System.Windows.Forms.DockStyle.Left;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ToWarehouseAddressControl, false);
			this.ToWarehouseAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.ToWarehouseAddressControl.Name = "ToWarehouseAddressControl";
			this.ToWarehouseAddressControl.PopupCaption = "";
			this.ToWarehouseAddressControl.ShowAddress = false;
			this.ToWarehouseAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 24, true);
			this.ToWarehouseAddressControl.TabIndex = 0;
			// 
			// DetailsPanel
			// 
			this.DetailsPanel.AutoScroll = true;
			this.DetailsPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.DetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsPanel.Name = "DetailsPanel";
			this.DetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1097, 113, true);
			this.DetailsPanel.TabIndex = 6;
			// 
			// EntryInstructionDetailBasicUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.OtherPartiesDetailsPanel);
			this.Controls.Add(this.DetailsPanel);
			this.Name = "EntryInstructionDetailBasicUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1097, 298, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OtherPartiesDetailsPanel.ResumeLayout(false);
			this.OtherPartiesDetailsPanel.PerformLayout();
			this.OtherPartiesGroupBox.ResumeLayout(false);
			this.OtherPartiesGroupBox.PerformLayout();
			this.FromWarehouseGroupBox.ResumeLayout(false);
			this.FromWarehouseGroupBox.PerformLayout();
			this.FromWarehouseAddressControl.ResumeLayout(true);
			this.FromWarehouseAddressControl.PerformLayout();
			this.ToWarehouseGroupBox.ResumeLayout(false);
			this.ToWarehouseGroupBox.PerformLayout();
			this.ToWarehouseAddressControl.ResumeLayout(true);
			this.ToWarehouseAddressControl.PerformLayout();
			this.DetailsPanel.ResumeLayout(false);
			this.DetailsPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal ZArchitecture.GUI.ZPanel DetailsPanel;
		internal ZArchitecture.GUI.ZPanel OtherPartiesDetailsPanel;
		internal ZArchitecture.ZTextBox ToWarehouseCodeTextBox;
		internal ZArchitecture.ZTextBox FromWarehouseCodeTextBox;
		internal ZArchitecture.GUI.ZGroupBox FromWarehouseGroupBox;

		#endregion

		internal ZGroupBox OtherPartiesGroupBox;
		internal ZGroupBox ToWarehouseGroupBox;
		internal ZAddressControl ToWarehouseAddressControl;
		internal ZAddressControl FromWarehouseAddressControl;
	}
}
