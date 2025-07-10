using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Organisation.UserControls
{
	partial class FeesAndChargesUserControl
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
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.schemaBindingSource = new System.Windows.Forms.BindingSource(this.components);
			this.Amount2TypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.Amount1TypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.FeeAndChargesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.NotesLabel = new Enterprise.ZArchitecture.ZLabel();
			this.NoteTextRichTextBox = new Enterprise.ZArchitecture.GUI.ZRichTextBox();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ORF_Amount1CalcFindBox = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			this.ORF_Amount2CalcFindBox1 = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.schemaBindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.FeeAndChargesGrid)).BeginInit();
			this.zGroupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgRateFeeChargeLevelCollection);
			// 
			// Amount2TypeDropEdit
			// 
			this.Amount2TypeDropEdit.AllowDrop = true;
			this.Amount2TypeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.Amount2TypeDropEdit, "ORF_Amount2Type");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgRateFeeChargeLevel)(null)).ORF_Amount2Type)));
			this.Amount2TypeDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.Amount2TypeDropEdit, false);
			this.Amount2TypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(433, 19, true);
			this.Amount2TypeDropEdit.Name = "Amount2TypeDropEdit";
			this.Amount2TypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(131, 20, true);
			this.Amount2TypeDropEdit.TabIndex = 13;
			// 
			// Amount1TypeDropEdit
			// 
			this.Amount1TypeDropEdit.AllowDrop = true;
			this.Amount1TypeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.Amount1TypeDropEdit, "ORF_Amount1Type");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgRateFeeChargeLevel)(null)).ORF_Amount1Type)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.Amount1TypeDropEdit, false);
			this.Amount1TypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.Amount1TypeDropEdit.Name = "Amount1TypeDropEdit";
			this.Amount1TypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(147, 20, true);
			this.Amount1TypeDropEdit.TabIndex = 7;
			// 
			// FeeAndChargesGrid
			// 
			this.FeeAndChargesGrid.AllowNavigation = false;
			this.FeeAndChargesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.FeeAndChargesGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgRateFeeChargeLevel)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgRateFeeChargeLevel)(null)).ORF_ServiceType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgRateFeeChargeLevel)(null)).ServiceDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgRateFeeChargeLevel)(null)).ORF_Level)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgRateFeeChargeLevel)(null)).LevelDescription)));
			this.FeeAndChargesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo3.ColumnName = "ORF_ServiceType";
			zTextBoxColumnStyleInfo3.ColumnName = "ServiceDescription";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo4.ColumnName = "ORF_Level";
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("c4361242-c7b6-4e76-befb-368e705fdb16", "Level Desc.", "Level Description", "");
			zTextBoxColumnStyleInfo4.ColumnName = "LevelDescription";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.FeeAndChargesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.FeeAndChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.FeeAndChargesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.FeeAndChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.FeeAndChargesGrid.CopySelectedRowsAllowed = true;
			this.FeeAndChargesGrid.GridId = "f7eca40d-9938-4edc-9cc8-76ddcb2bbe5b";
			this.FeeAndChargesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.FeeAndChargesGrid.LayoutKey = "zGrid1";
			this.FeeAndChargesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FeeAndChargesGrid.Name = "FeeAndChargesGrid";
			this.FeeAndChargesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(725, 371, true);
			this.FeeAndChargesGrid.TabIndex = 0;
			// 
			// NotesLabel
			// 
			this.NotesLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.NotesLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("4dd285c6-a1f5-4f25-8fa9-e996c40ce9b0", "Notes");
			this.NotesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 57, true);
			this.NotesLabel.Name = "NotesLabel";
			this.NotesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 23, true);
			this.NotesLabel.TabIndex = 14;
			// 
			// NoteTextRichTextBox
			// 
			this.NoteTextRichTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.NoteTextRichTextBox, "ORF_NoteData");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBlob)(((Enterprise.MasterFiles.Business.OrgRateFeeChargeLevel)(null)).ORF_NoteData)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.NoteTextRichTextBox, false);
			this.NoteTextRichTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(49, 45, true);
			this.NoteTextRichTextBox.MaxLength = 10000000;
			this.NoteTextRichTextBox.Name = "NoteTextRichTextBox";
			this.NoteTextRichTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(664, 88, true);
			this.NoteTextRichTextBox.TabIndex = 18;
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.zGroupBox1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("d4894e9e-b937-4b94-8dc8-df18f543e638", "Amounts And Notes");
			this.zGroupBox1.Controls.Add(this.ORF_Amount2CalcFindBox1);
			this.zGroupBox1.Controls.Add(this.ORF_Amount1CalcFindBox);
			this.zGroupBox1.Controls.Add(this.Amount1TypeDropEdit);
			this.zGroupBox1.Controls.Add(this.NotesLabel);
			this.zGroupBox1.Controls.Add(this.NoteTextRichTextBox);
			this.zGroupBox1.Controls.Add(this.Amount2TypeDropEdit);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 377, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(719, 139, true);
			this.zGroupBox1.TabIndex = 19;
			this.zGroupBox1.TabStop = false;
			// 
			// ORF_Amount1CalcFindBox
			// 
			this.ORF_Amount1CalcFindBox.AllowDrop = true;
			this.ORF_Amount1CalcFindBox.BindToAmount = "ORF_Amount1";
			this.ORF_Amount1CalcFindBox.BindToUnit = "ORF_RX_NKAmount1Currency";
			this.ORF_Amount1CalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.ORF_Amount1CalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(159, 19, true);
			this.ORF_Amount1CalcFindBox.Name = "ORF_Amount1CalcFindBox";
			this.ORF_Amount1CalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(124, 20, true);
			this.ORF_Amount1CalcFindBox.TabIndex = 8;
			// 
			// ORF_Amount2CalcFindBox1
			// 
			this.ORF_Amount2CalcFindBox1.AllowDrop = true;
			this.ORF_Amount2CalcFindBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ORF_Amount2CalcFindBox1.BindToAmount = "ORF_Amount2";
			this.ORF_Amount2CalcFindBox1.BindToUnit = "ORF_RX_NKAmount2Currency";
			this.ORF_Amount2CalcFindBox1.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.ORF_Amount2CalcFindBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(570, 19, true);
			this.ORF_Amount2CalcFindBox1.Name = "ORF_Amount2CalcFindBox1";
			this.ORF_Amount2CalcFindBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(143, 20, true);
			this.ORF_Amount2CalcFindBox1.TabIndex = 14;
			// 
			// FeesAndChargesUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zGroupBox1);
			this.Controls.Add(this.FeeAndChargesGrid);
			this.Name = "FeesAndChargesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(725, 519, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.schemaBindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.FeeAndChargesGrid)).EndInit();
			this.zGroupBox1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.BindingSource schemaBindingSource;
		private ZArchitecture.ZGrid FeeAndChargesGrid;
		private ZArchitecture.GUI.ZDropEdit Amount2TypeDropEdit;
		private ZArchitecture.GUI.ZDropEdit Amount1TypeDropEdit;
		private ZArchitecture.ZLabel NotesLabel;
		private ZArchitecture.GUI.ZRichTextBox NoteTextRichTextBox;
		private ZArchitecture.GUI.ZGroupBox zGroupBox1;
		private ZCalcFindBox ORF_Amount2CalcFindBox1;
		private ZCalcFindBox ORF_Amount1CalcFindBox;

	}
}
