using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.SG.Access.GUI
{
	partial class AsycudaItemSelectionDialog
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		protected new void InitializeComponent()
		{
			this.zPanelCycleFileds = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.zDropEditCycleNumber = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zDateEditCycleDate = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.ItemsGrid)).BeginInit();
			this.ItemsGrid.SuspendLayout();
			this.ItemsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zPanelCycleFileds.SuspendLayout();
			this.zDropEditCycleNumber.SuspendLayout();
			this.zDateEditCycleDate.SuspendLayout();
			this.SuspendLayout();
			// 
			// ItemsGrid
			// 
			this.ItemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(461, 209, true);
			// 
			// ItemsGroupBox
			// 
			this.ItemsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 34, true);
			this.ItemsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(465, 225, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.SG.Access.Business.MessageChooser);
			// 
			// zPanelCycleFileds
			// 
			this.zPanelCycleFileds.Controls.Add(this.zDropEditCycleNumber);
			this.zPanelCycleFileds.Controls.Add(this.zDateEditCycleDate);
			this.zPanelCycleFileds.Dock = System.Windows.Forms.DockStyle.Top;
			this.zPanelCycleFileds.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zPanelCycleFileds.Name = "zPanelCycleFileds";
			this.zPanelCycleFileds.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(465, 34, true);
			this.zPanelCycleFileds.TabIndex = 3;
			// 
			// zDropEditCycleNumber
			// 
			this.zDropEditCycleNumber.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEditCycleNumber, "CycleNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.SG.Access.Business.MessageChooser)(null)).CycleNumber)));
			this.zDropEditCycleNumber.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(249, 7, true);
			this.zDropEditCycleNumber.Name = "zDropEditCycleNumber";
			this.zDropEditCycleNumber.ShowDescriptionBox = false;
			this.zDropEditCycleNumber.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 17, true);
			this.zDropEditCycleNumber.TabIndex = 1;
			// 
			// zDateEditCycleDate
			// 
			this.zDateEditCycleDate.AllowDrop = true;
			this.zDateEditCycleDate.AutoCompleteMonthThreshold = 1;
			this.zDateEditCycleDate.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.zDateEditCycleDate, "CycleDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.SG.Access.Business.MessageChooser)(null)).CycleDate)));
			this.zDateEditCycleDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 7, true);
			this.zDateEditCycleDate.Name = "zDateEditCycleDate";
			this.zDateEditCycleDate.TabIndex = 0;
			// 
			// AsycudaItemSelectionDialog
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(465, 318, true);
			this.Controls.Add(this.zPanelCycleFileds);
			this.DataSourceAssemblyName = "Enterprise.Customs.SG.Access.Business";
			this.DataSourceType = typeof(Enterprise.Customs.SG.Access.Business.MessageChooser);
			this.DataSourceTypeName = "Enterprise.Customs.SG.Access.Business.MessageChooser";
			this.Name = "AsycudaItemSelectionDialog";
			this.Controls.SetChildIndex(this.zPanelCycleFileds, 0);
			this.Controls.SetChildIndex(this.ItemsGroupBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.ItemsGrid)).EndInit();
			this.ItemsGrid.ResumeLayout(false);
			this.ItemsGrid.PerformLayout();
			this.ItemsGroupBox.ResumeLayout(false);
			this.ItemsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zPanelCycleFileds.ResumeLayout(false);
			this.zPanelCycleFileds.PerformLayout();
			this.zDropEditCycleNumber.ResumeLayout(true);
			this.zDropEditCycleNumber.PerformLayout();
			this.zDateEditCycleDate.ResumeLayout(true);
			this.zDateEditCycleDate.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZPanel zPanelCycleFileds;
		private ZDropEdit zDropEditCycleNumber;
		private ZDateEdit zDateEditCycleDate;
	}
}
