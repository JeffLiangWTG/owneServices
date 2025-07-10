namespace Enterprise.Customs.NL.GUI
{
	partial class AdditionalDocumentsUserControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfoCsiType = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfoCsiReferenceNumber = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();

			this.KindDropBox = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.KindDropBox.SuspendLayout();
			this.ReferenceTextBox.SuspendLayout();

			// 
			// AdditionalDocumentsGrid
			// 
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.NL.Business.Declaration.JobDeclaration)(null)).AdditionalInfos)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NL.Business.Declaration.AdditionalInfo)(((System.Collections.IList)(((Enterprise.Customs.NL.Business.Declaration.JobDeclaration)(null)).AdditionalInfos)).SyncRoot)).CSI_SubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NL.Business.Declaration.AdditionalInfo)(((System.Collections.IList)(((Enterprise.Customs.NL.Business.Declaration.JobDeclaration)(null)).AdditionalInfos)).SyncRoot)).CSI_ReferenceNumber)));
			zDropEditColumnStyleInfoCsiType.ColumnName = "CSI_SubType";
			zDropEditColumnStyleInfoCsiType.IsCustomColumn = false;
			zDropEditColumnStyleInfoCsiType.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfoCsiReferenceNumber.ColumnName = "CSI_ReferenceNumber";
			zTextBoxColumnStyleInfoCsiReferenceNumber.IsCustomColumn = false;
			zTextBoxColumnStyleInfoCsiReferenceNumber.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
			this.AdditionalInfosGrid.ColumnStyles.Add(zDropEditColumnStyleInfoCsiType);
			this.AdditionalInfosGrid.ColumnStyles.Add(zTextBoxColumnStyleInfoCsiReferenceNumber);
			this.AdditionalInfosGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(750, 100, true);
			this.AdditionalInfosGrid.Dock = System.Windows.Forms.DockStyle.None;
			this.AdditionalInfosGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top)
				| System.Windows.Forms.AnchorStyles.Left)
				| System.Windows.Forms.AnchorStyles.Right)));

			// 
			// KindDropBox
			// 
			this.KindDropBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.KindDropBox, "FilteredInvoiceLines.AdditionalInfos.CSI_SubType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NL.Business.Declaration.AdditionalInfo)(null)).CSI_SubType)));
			this.KindDropBox.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("A59DDB5E-2C65-4DB8-BBCB-4AA7608C231E", "Kind");
			this.KindDropBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 110, true);
			this.KindDropBox.Name = "KindDropBox";
			this.KindDropBox.ShouldResizeByMaxLength = true;
			this.KindDropBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(314, 20, true);
			this.KindDropBox.TabIndex = 1;
			this.KindDropBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
										| System.Windows.Forms.AnchorStyles.Left)
										| System.Windows.Forms.AnchorStyles.Right)));

			// 
			// AddInfoTypeCodeDropEdit
			//
			this.AddInfoTypeCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 135, true);
			this.AddInfoTypeCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(314, 20, true);
			this.AddInfoTypeCodeDropEdit.TabIndex = 2;

			// 
			// ReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReferenceTextBox, "FilteredInvoiceLines.AdditionalInfos.CSI_ReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NL.Business.Declaration.AdditionalInfo)(null)).CSI_ReferenceNumber)));
			this.ReferenceTextBox.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("3E71DF62-1CD7-4B2A-8306-223941F40563", "Reference");
			this.ReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 160, true);
			this.ReferenceTextBox.Name = "ReferenceTextBox";
			this.ReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(314, 18, true);
			this.ReferenceTextBox.TabIndex = 3;
			this.ReferenceTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
							| System.Windows.Forms.AnchorStyles.Left)
							| System.Windows.Forms.AnchorStyles.Right)));

			// 
			// AdditionalInfosGroupBox
			//
			this.AdditionalInfosGroupBox.Visible = false;

			// 
			// AddiInfoDescriptionTextBox
			//
			this.AddiInfoDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 185, true);
			this.AddiInfoDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(314, 18, true);
			this.AddiInfoDescriptionTextBox.Multiline = false;
			this.AddiInfoDescriptionTextBox.TabIndex = 4;
			this.AddiInfoDescriptionTextBox.Visible = true;

			// 
			// AdditionalDocumentsUserControl
			//
			this.Controls.Add(AddInfoTypeCodeDropEdit);
			this.Controls.Add(AddiInfoDescriptionTextBox);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.KindDropBox);
			this.Controls.Add(this.ReferenceTextBox);
			this.Name = "AdditionalDocumentsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(624, 151, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.KindDropBox.ResumeLayout(true);
			this.KindDropBox.PerformLayout();
			this.ReferenceTextBox.ResumeLayout(true);
			this.ReferenceTextBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		ZArchitecture.GUI.ZDropEdit KindDropBox;
		Enterprise.ZArchitecture.ZTextBox ReferenceTextBox;
		#endregion
	}
}
