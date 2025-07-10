namespace Enterprise.Customs.TR.NCTS.GUI
{
	partial class PreviousDocumentsUserControl
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
			this.AmountCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.CountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.PaymentTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.NatureOfBussinessDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PrevDocsTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TR.NCTS.Business.NctsDepartureCargoDesc);
			// 
			// AmountCalcDropEdit
			// 
			this.AmountCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AmountCalcDropEdit, "CSI_Value");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.NCTS.Business.NctsPreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureCargoDesc)(null)).PreviousDocuments)).SyncRoot)).CSI_Value)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.NCTS.Business.NctsPreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureCargoDesc)(null)).PreviousDocuments)).SyncRoot)).CSI_RX_NKCurrency)));
			this.AmountCalcDropEdit.CaptionResourceString = Res.GetData("A2D14BF6-C365-4D9D-83A2-FBC96E7DB008", "Amount");
			this.AmountCalcDropEdit.BindToAmount = "CSI_Value";
			this.AmountCalcDropEdit.BindToUnit = "CSI_RX_NKCurrency";
			this.AmountCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 88, true);
			this.AmountCalcDropEdit.Name = "AmountCalcDropEdit";
			this.AmountCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(163, 20, true);
			this.AmountCalcDropEdit.TabIndex = 3;
			// 
			// CountryCodeFindBox
			// 
			this.CountryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryCodeFindBox, "CSI_RN_NKCountryCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.NCTS.Business.NctsPreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureCargoDesc)(null)).PreviousDocuments)).SyncRoot)).CSI_RN_NKCountryCode)));
			this.CountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 114, true);
			this.CountryCodeFindBox.CaptionResourceString = Res.GetData("A2D14BF6-C365-4D9D-83A2-FBC96E7DB111", "Trade Country");
			this.CountryCodeFindBox.Name = "CountryCodeFindBox";
			this.CountryCodeFindBox.PreBoundMaxLength = 4;
			this.CountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(163, 20, true);
			this.CountryCodeFindBox.TabIndex = 10;
			// 
			// PrevDocsTypeDropEdit
			//
			this.PrevDocsTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PrevDocsTypeDropEdit, "Incoterm");
			this.PrevDocsTypeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.NCTS.Business.NctsPreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureCargoDesc)(null)).PreviousDocuments)).SyncRoot)).Incoterm)));
			this.PrevDocsTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 36, true);
			this.PrevDocsTypeDropEdit.CaptionResourceString = Res.GetData("A2D14BF6-C365-4D9D-83A2-FBC96E7DB222S", "Incoterm");
			this.PrevDocsTypeDropEdit.Name = "PrevDocsTypeDropEdit";
			this.PrevDocsTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(163, 20, true);
			this.PrevDocsTypeDropEdit.TabIndex = 7;
			// 
			// PaymentTypeDropEdit
			// 
			this.PaymentTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PaymentTypeDropEdit, "CSI_SubType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.NCTS.Business.NctsPreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureCargoDesc)(null)).PreviousDocuments)).SyncRoot)).CSI_SubType)));
			this.PaymentTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 62, true);
			this.PaymentTypeDropEdit.CaptionResourceString = Res.GetData("A2D14BF6-C365-4D9D-83A2-FBC96E7DB233", "Payment Type");
			this.PaymentTypeDropEdit.Name = "PaymentTypeDropEdit";
			this.PaymentTypeDropEdit.PreBoundMaxLength = 4;
			this.PaymentTypeDropEdit.ShouldResizeByMaxLength = true;
			this.PaymentTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(163, 20, true);
			this.PaymentTypeDropEdit.TabIndex = 8;
			// 
			// NatureOfBussinessDropEdit
			// 
			this.NatureOfBussinessDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NatureOfBussinessDropEdit, "CSI_Procedure");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.NCTS.Business.NctsPreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureCargoDesc)(null)).PreviousDocuments)).SyncRoot)).CSI_Procedure)));
			this.NatureOfBussinessDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 88, true);
			this.NatureOfBussinessDropEdit.CaptionResourceString = Res.GetData("A2D14BF6-C365-4D9D-83A2-FBC96E7DB879", "Nature of Business");
			this.NatureOfBussinessDropEdit.Name = "NatureOfBussinessDropEdit";
			this.NatureOfBussinessDropEdit.PreBoundMaxLength = 4;
			this.NatureOfBussinessDropEdit.ShouldResizeByMaxLength = true;
			this.NatureOfBussinessDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(163, 20, true);
			this.NatureOfBussinessDropEdit.TabIndex = 9;
			// 
			// PreviousDocumentsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "PreviousDocumentsUserControl";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.Controls.Add(this.AmountCalcDropEdit);
			this.AmountCalcDropEdit.ResumeLayout(true);
			this.AmountCalcDropEdit.PerformLayout();
			this.Controls.Add(this.CountryCodeFindBox);
			this.CountryCodeFindBox.ResumeLayout(true);
			this.CountryCodeFindBox.PerformLayout();
			this.Controls.Add(this.PrevDocsTypeDropEdit);
			this.PrevDocsTypeDropEdit.ResumeLayout(true);
			this.PrevDocsTypeDropEdit.PerformLayout();
			this.Controls.Add(this.PaymentTypeDropEdit);
			this.PaymentTypeDropEdit.ResumeLayout(true);
			this.PaymentTypeDropEdit.PerformLayout();
			this.Controls.Add(this.NatureOfBussinessDropEdit);
			this.NatureOfBussinessDropEdit.ResumeLayout(true);
			this.NatureOfBussinessDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		public Enterprise.ZArchitecture.GUI.ZCalcDropEdit AmountCalcDropEdit;
		public Enterprise.ZArchitecture.GUI.ZCodeFindBox CountryCodeFindBox;
		public Enterprise.ZArchitecture.GUI.ZDropEdit PrevDocsTypeDropEdit;
		public Enterprise.ZArchitecture.GUI.ZDropEdit PaymentTypeDropEdit;
		public Enterprise.ZArchitecture.GUI.ZDropEdit NatureOfBussinessDropEdit;
	}
}
