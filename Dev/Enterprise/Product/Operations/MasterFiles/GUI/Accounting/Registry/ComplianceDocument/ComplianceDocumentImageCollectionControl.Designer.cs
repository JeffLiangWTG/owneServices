namespace Enterprise.MasterFiles.GUI
{
	public partial class ComplianceDocumentImageCollectionControl
	{
		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			this.ReceiptGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ReceiptImageSelectionControl = new Enterprise.ZArchitecture.GUI.ImageSelectionControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ReceiptGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.ComplianceDocumentImage);
			// 
			// ComplianceDocumentGrid
			// 
			this.ReceiptGrid.AllowNavigation = false;
			this.ReceiptGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ReceiptGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ComplianceDocumentImage)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ComplianceDocumentImage)(null)).Country)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ComplianceDocumentImage)(null)).ComplianceSubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ComplianceDocumentImage)(null)).Remark)));
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ComplianceDocumentImageCollectionControl|94F23A9F-37F0-4B94-8A5D-A9E36846E3AB", "Country/Region");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "Country";
			zCodeFindBoxColumnStyleInfo1.IsMandatory = true;
			zCodeFindBoxColumnStyleInfo1.PopupCaption = "Select Country";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ComplianceDocumentImageCollectionControl|1A4AC1C7-9CBB-4FA0-B8E0-81CD08F5D9EB", "Compliance Sub Type");
			zDropEditColumnStyleInfo1.ColumnName = "ComplianceSubType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zMultiLineTextBoxColumnInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ComplianceDocumentImageCollectionControl|6BACFABE-C57E-4D00-975D-1B5250C513DF", "Remark");
			zMultiLineTextBoxColumnInfo1.ColumnName = "Remark";
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			this.ReceiptGrid.CaptionVisible = false;
			this.ReceiptGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.ReceiptGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ReceiptGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.ReceiptGrid.GridId = "7E3FF2DA-7C38-4098-ACF9-05BFA1F6FB7A";
			this.ReceiptGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ReceiptGrid.LayoutKey = "Grid";
			this.ReceiptGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReceiptGrid.Name = "ReceiptGrid";
			this.ReceiptGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 440, true);
			this.ReceiptGrid.TabIndex = 0;
			// 
			// ComplianceDocumentImageSelectionControl
			// 
			this.ReceiptImageSelectionControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ReceiptImageSelectionControl, "Image");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Drawing.Image)(((Enterprise.MasterFiles.Business.ComplianceDocumentImage)(null)).Image)));
			this.ReceiptImageSelectionControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 8, true);
			this.ReceiptImageSelectionControl.Name = "ReceiptImageSelectionControl";
			this.ReceiptImageSelectionControl.ReadOnly = true;
			this.ReceiptImageSelectionControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(176, 136, true);
			this.ReceiptImageSelectionControl.TabIndex = 1;
			// 
			// ComplianceDocumentImageCollectionControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ReceiptImageSelectionControl);
			this.Controls.Add(this.ReceiptGrid);
			this.Name = "ComplianceDocumentImageCollectionControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(424, 440, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ReceiptGrid)).EndInit();
			this.ResumeLayout(false);
		}
		#endregion

		protected internal Enterprise.ZArchitecture.ZGrid ReceiptGrid;
		protected Enterprise.ZArchitecture.GUI.ImageSelectionControl ReceiptImageSelectionControl;
	}
}
