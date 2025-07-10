namespace Enterprise.Customs.TR.NCTS.GUI
{
	partial class SecurityTabUserControl
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
			this.PlaceOfloadingFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.SecurityDetailsGroupBox.SuspendLayout();
			this.PlaceOfUnloadingFindBox.SuspendLayout();
			this.TransportChargesMoPDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// SecurityDetailsGroupBox
			// 
			this.SecurityDetailsGroupBox.Controls.Add(this.PlaceOfloadingFindBox);
			this.SecurityDetailsGroupBox.Controls.SetChildIndex(this.PlaceOfloadingFindBox, 0);
			this.SecurityDetailsGroupBox.Controls.SetChildIndex(this.CommercialReferenceNumberTextBox, 0);
			this.SecurityDetailsGroupBox.Controls.SetChildIndex(this.TransportChargesMoPDropEdit, 0);
			this.SecurityDetailsGroupBox.Controls.SetChildIndex(this.PlaceOfUnloadingFindBox, 0);
			// 
			// TransportChargesMoPDropEdit
			// 
			this.TransportChargesMoPDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(222, 122, true);
			this.TransportChargesMoPDropEdit.TabIndex = 5;
			// 
			// CommercialReferenceNumberTextBox
			// 
			this.CommercialReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(222, 148, true);
			this.CommercialReferenceNumberTextBox.TabIndex = 6;
			// 
			// PlaceOfUnloadingTextBox
			// 
			this.PlaceOfUnloadingFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(222, 96, true);
			this.PlaceOfUnloadingFindBox.TabIndex = 4;
			this.PlaceOfUnloadingFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			this.PlaceOfUnloadingFindBox.ShouldResize = false;
			this.PlaceOfUnloadingFindBox.CodeBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(45, 20, true);

			// 
			// PlaceOfloadingTextBox
			//
			this.PlaceOfloadingFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PlaceOfloadingFindBox, "MovementHeader.BM_PlaceOfLoading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).MovementHeader.BM_PlaceOfLoading)));
			this.PlaceOfloadingFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(222, 70, true);
			this.PlaceOfloadingFindBox.Name = "PlaceOfloadingFindBox";
			this.PlaceOfloadingFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(196, 20, true);
			this.PlaceOfloadingFindBox.ShouldResize = false;
			this.PlaceOfloadingFindBox.CodeBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(45, 20, true);
			this.PlaceOfloadingFindBox.TabIndex = 3;
			// 
			// SecurityTabUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "SecurityTabUserControl";
			this.SecurityDetailsGroupBox.ResumeLayout(false);
			this.SecurityDetailsGroupBox.PerformLayout();
			this.PlaceOfUnloadingFindBox.ResumeLayout(true);
			this.PlaceOfUnloadingFindBox.PerformLayout();
			this.TransportChargesMoPDropEdit.ResumeLayout(true);
			this.TransportChargesMoPDropEdit.PerformLayout();
			this.PlaceOfloadingFindBox.ResumeLayout(true);
			this.PlaceOfloadingFindBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected ZArchitecture.GUI.ZCodeFindBox PlaceOfloadingFindBox;
	}
}
