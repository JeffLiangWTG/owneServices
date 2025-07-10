namespace Enterprise.Customs.TW.GUI
{
	partial class CusCustomsOfficeRegistryItemUserControl
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
			this.CustomsOfficeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CustomsOfficeCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TW.Business.CusCustomsOffice);
			// 
			// CustomsOfficeCodeFindBox
			// 
			this.CustomsOfficeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsOfficeCodeFindBox, "CustomsOfficeCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.CusCustomsOffice)(null)).CustomsOfficeCode)));
			this.CustomsOfficeCodeFindBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("E7E38BCF-ABF0-424D-93D1-FC308FD79EFD", "Customs Office");
			this.CustomsOfficeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 3, true);
			this.CustomsOfficeCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			this.CustomsOfficeCodeFindBox.Name = "CustomsOfficeCodeFindBox";
			this.CustomsOfficeCodeFindBox.PreBoundMaxLength = 3;
			this.CustomsOfficeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			this.CustomsOfficeCodeFindBox.TabIndex = 0;
			// 
			// CusCustomsOfficeRegistryItemUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CustomsOfficeCodeFindBox);
			this.Name = "CusCustomsOfficeRegistryItemUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(319, 150, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CustomsOfficeCodeFindBox.ResumeLayout(true);
			this.CustomsOfficeCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZCodeFindBox CustomsOfficeCodeFindBox;
	}
}
