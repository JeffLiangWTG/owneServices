using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.GUI
{
	partial class ECCNCodesForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.zGridECCNCode = new Enterprise.ZArchitecture.ZGrid();
			this.zButtonOK = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zButtonClose = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.zGridECCNCode)).BeginInit();
			this.zGridECCNCode.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 318, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(318, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(JobComInvoiceLine);
			// 
			// zGridECCNCode
			// 
			this.zGridECCNCode.AllowNavigation = false;
			this.zGridECCNCode.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.zGridECCNCode, "ECCNCodes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((JobComInvoiceLine)(null)).ECCNCodes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NL.Business.ECCNCode)(((System.Collections.IList)(((JobComInvoiceLine)(null)).ECCNCodes)).SyncRoot)).CY_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NL.Business.ECCNCode)(((System.Collections.IList)(((JobComInvoiceLine)(null)).ECCNCodes)).SyncRoot)).Description)));
			this.zGridECCNCode.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "CY_Code";
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("4246D1DC-0B59-4780-8902-D937710AE32C", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "Description";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.zGridECCNCode.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.zGridECCNCode.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.zGridECCNCode.GridId = "e39b0b5a-fe1d-4baf-a16b-f653cc61c81l";
			this.zGridECCNCode.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGridECCNCode.LayoutKey = "zGridECCNCode";
			this.zGridECCNCode.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGridECCNCode.Name = "zGridECCNCode";
			this.zGridECCNCode.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(318, 283, true);
			this.zGridECCNCode.TabIndex = 1;
			// 
			// zButtonOK
			// 
			this.zButtonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.zButtonOK.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("8E872012-03ED-4D17-9DC2-9533432AD114", "OK");
			this.zButtonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.zButtonOK.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 289, true);
			this.zButtonOK.Name = "zButtonOK";
			this.zButtonOK.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.zButtonOK.TabIndex = 2;
			this.zButtonOK.ToolTipCaption = null;
			this.zButtonOK.UseVisualStyleBackColor = true;
			this.zButtonOK.Click += new System.EventHandler(this.ZButtonOK_Click);
			// 
			// zButtonClose
			// 
			this.zButtonClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.zButtonClose.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("1B3A78A5-5891-4FE7-A016-629C854C10E9", "Cancel");
			this.zButtonClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.zButtonClose.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(195, 289, true);
			this.zButtonClose.Name = "zButtonClose";
			this.zButtonClose.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.zButtonClose.TabIndex = 3;
			this.zButtonClose.ToolTipCaption = null;
			this.zButtonClose.UseVisualStyleBackColor = true;
			this.zButtonClose.Click += new System.EventHandler(this.ZButtonClose_Click);
			// 
			// ECCNCodesForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("B41ED7ED-B4DB-483B-AB72-B45A6D3AD733", "ECCN - Dual Use Codes");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(318, 342, true);
			this.Controls.Add(this.zGridECCNCode);
			this.Controls.Add(this.zButtonClose);
			this.Controls.Add(this.zButtonOK);
			this.DataSourceType = typeof(JobComInvoiceLine);
			this.Name = "ECCNCodesForm";
			this.Text = "ECCN - Dual Use Codes";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.zButtonOK, 0);
			this.Controls.SetChildIndex(this.zButtonClose, 0);
			this.Controls.SetChildIndex(this.zGridECCNCode, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.zGridECCNCode)).EndInit();
			this.zGridECCNCode.ResumeLayout(false);
			this.zGridECCNCode.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid zGridECCNCode;
		private ZArchitecture.GUI.ZButton zButtonOK;
		private ZArchitecture.GUI.ZButton zButtonClose;
	}
}
