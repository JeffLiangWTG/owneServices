using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture;

namespace Enterprise.TransportCommon.GUI.Registry
{
	public partial class DateAndReferenceControl
	{
		public ZGrid DateAndReferenceGrid;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZCheckBoxColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new ZCheckBoxColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new ZCheckBoxColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new ZCheckBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo5 = new ZCheckBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZTextBoxColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo6 = new ZCheckBoxColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo7 = new ZCheckBoxColumnStyleInfo();
			this.DateAndReferenceGrid = new ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DateAndReferenceGrid)).BeginInit();
			this.DateAndReferenceGrid.SuspendLayout();
			this.SuspendLayout();
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(TransportCommon.Registry.DateAndReferenceCollection);
			//
			// DateAndReferenceGrid
			//
			this.DateAndReferenceGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.DateAndReferenceGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(null);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((TransportCommon.Registry.DateAndReference)(null)).Code);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((TransportCommon.Registry.DateAndReference)(null)).EnglishDescription);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((TransportCommon.Registry.DateAndReference)(null)).BookingDirection);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((TransportCommon.Registry.DateAndReference)(null)).InstructionType);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((TransportCommon.Registry.DateAndReference)(null)).OrganisationType);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((TransportCommon.Registry.DateAndReference)(null)).ContainerMode);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((TransportCommon.Registry.DateAndReference)(null)).AutoAdd);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((TransportCommon.Registry.DateAndReference)(null)).AllowActualDate);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((TransportCommon.Registry.DateAndReference)(null)).AllowEstimatedDate);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((TransportCommon.Registry.DateAndReference)(null)).AllowRequiredFromDate);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((TransportCommon.Registry.DateAndReference)(null)).EnglishAllowRequiredFromLabel);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((TransportCommon.Registry.DateAndReference)(null)).AllowRequiredToDate);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((TransportCommon.Registry.DateAndReference)(null)).EnglishAllowRequiredToLabel);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((TransportCommon.Registry.DateAndReference)(null)).AllowReference);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((TransportCommon.Registry.DateAndReference)(null)).AllowReceivedBy);
			this.DateAndReferenceGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo2.ColumnName = "EnglishDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.Caption = "";
			zDropEditColumnStyleInfo1.ColumnName = "BookingDirection";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDropEditColumnStyleInfo2.Caption = "";
			zDropEditColumnStyleInfo2.ColumnName = "InstructionType";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDropEditColumnStyleInfo3.ColumnName = "OrganisationType";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDropEditColumnStyleInfo4.ColumnName = "ContainerMode";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCheckBoxColumnStyleInfo1.Caption = "";
			zCheckBoxColumnStyleInfo1.ColumnName = "AutoAdd";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			zCheckBoxColumnStyleInfo2.ColumnName = "AllowActualDate";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(58);
			zCheckBoxColumnStyleInfo3.ColumnName = "AllowEstimatedDate";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(58);
			zCheckBoxColumnStyleInfo4.ColumnName = "AllowRequiredFromDate";
			zCheckBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(58);
			zTextBoxColumnStyleInfo3.ColumnName = "EnglishAllowRequiredFromLabel";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			zCheckBoxColumnStyleInfo5.ColumnName = "AllowRequiredToDate";
			zCheckBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(58);
			zTextBoxColumnStyleInfo4.ColumnName = "EnglishAllowRequiredToLabel";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			zCheckBoxColumnStyleInfo6.ColumnName = "AllowReference";
			zCheckBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(58);
			zCheckBoxColumnStyleInfo7.ColumnName = "AllowReceivedBy";
			zCheckBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(58);
			this.DateAndReferenceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.DateAndReferenceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.DateAndReferenceGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.DateAndReferenceGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.DateAndReferenceGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.DateAndReferenceGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.DateAndReferenceGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.DateAndReferenceGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.DateAndReferenceGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.DateAndReferenceGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.DateAndReferenceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.DateAndReferenceGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo5);
			this.DateAndReferenceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.DateAndReferenceGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo6);
			this.DateAndReferenceGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo7);
			this.DateAndReferenceGrid.CopySelectedRowsAllowed = true;
			this.DateAndReferenceGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DateAndReferenceGrid.GridId = "80af5ce3-8ef6-41b7-8b24-523b14c32dc0";
			this.DateAndReferenceGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DateAndReferenceGrid.LayoutKey = "DateAndReferenceGrid";
			this.DateAndReferenceGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DateAndReferenceGrid.Name = "DateAndReferenceGrid";
			this.DateAndReferenceGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(457, 240, true);
			this.DateAndReferenceGrid.TabIndex = 0;
			//
			// DateAndReferenceControl
			//
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DateAndReferenceGrid);
			this.Name = "DateAndReferenceControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(457, 240, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.DateAndReferenceGrid)).EndInit();
			this.DateAndReferenceGrid.ResumeLayout(false);
			this.DateAndReferenceGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
