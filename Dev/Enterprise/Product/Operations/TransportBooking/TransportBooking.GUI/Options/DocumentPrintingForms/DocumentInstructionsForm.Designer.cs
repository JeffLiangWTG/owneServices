using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.TransportBookings.GUI
{
	public partial class DocumentInstructionsForm
	{
		System.ComponentModel.Container components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo instructionTypeColumn = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo orgTypeColumn = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo orgCodeColumn = new ZMultiControlColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo zAddressDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo printCheckbox = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.SelectInstructionsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.instructionsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CancelPrintButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PrintButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.instructionsGrid)).BeginInit();
			this.instructionsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 314, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(434, 22, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.TransportBookings.Business.DocumentInstructions);
			// 
			// SelectInstructionsLabel
			// 
			this.SelectInstructionsLabel.AutoSize = true;
			this.SelectInstructionsLabel.CaptionResourceString = Res.GetData("DocumentInstructionsForm|d978cd77-fd6e-11ef-b70d-939686df6273", "Select Instruction");
			this.SelectInstructionsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 9, true);
			this.SelectInstructionsLabel.Name = "SelectInstructionsLabel";
			this.SelectInstructionsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(91, 13, true);
			this.SelectInstructionsLabel.TabIndex = 1;
			// 
			// instructionsGrid
			// 
			this.instructionsGrid.AllowNavigation = false;
			this.instructionsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.instructionsGrid, "InstructionsToSelectFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DocumentInstructions)(null)).InstructionsToSelectFrom)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.InstructionToSelectFromForPrinting)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DocumentInstructions)(null)).InstructionsToSelectFrom)).SyncRoot)).Instruction.KN_InstructionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.InstructionToSelectFromForPrinting)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DocumentInstructions)(null)).InstructionsToSelectFrom)).SyncRoot)).Instruction.OrganisationType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.InstructionToSelectFromForPrinting)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DocumentInstructions)(null)).InstructionsToSelectFrom)).SyncRoot)).Instruction.Address.OrganisationNameOrPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.TransportBookings.Business.InstructionToSelectFromForPrinting)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DocumentInstructions)(null)).InstructionsToSelectFrom)).SyncRoot)).Instruction.Address.E2_OA_Address)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.TransportBookings.Business.InstructionToSelectFromForPrinting)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DocumentInstructions)(null)).InstructionsToSelectFrom)).SyncRoot)).KN_Calc_PrintDocumentForInstruction)));
			this.instructionsGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.instructionsGrid.CaptionVisible = false;
			instructionTypeColumn.ColumnName = "Instruction+KN_InstructionType";
			instructionTypeColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			instructionTypeColumn.IsReadOnly = true;
			orgTypeColumn.ColumnName = "Instruction+OrganisationType";
			orgTypeColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			orgTypeColumn.IsReadOnly = true;
			orgCodeColumn.CaptionResourceString = Res.GetData("DocumentInstructionsForm|4591fbbc-fdfd-11ef-bc3b-be3d16e92d21", "Org. Code", "Organization Code", "");
			orgCodeColumn.ColumnName = "Instruction+Address+OrganisationNameOrPK";
			orgCodeColumn.FieldTypeColumnName = "Instruction+Address+OrganisationDataFieldType";
			orgCodeColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			orgCodeColumn.IsReadOnly = true;
			zAddressDropEditColumnStyleInfo1.ColumnName = "Instruction+Address+E2_OA_Address";
			zAddressDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zAddressDropEditColumnStyleInfo1.IsReadOnly = true;
			printCheckbox.CaptionResourceString = Res.GetData("DocumentInstructionsForm|ec2525cd-fd6e-11ef-8d65-d38d8abd9a39", "Print?");
			printCheckbox.ColumnName = "KN_Calc_PrintDocumentForInstruction";
			printCheckbox.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			this.instructionsGrid.ColumnStyles.Add(instructionTypeColumn);
			this.instructionsGrid.ColumnStyles.Add(orgTypeColumn);
			this.instructionsGrid.ColumnStyles.Add(orgCodeColumn);
			this.instructionsGrid.ColumnStyles.Add(zAddressDropEditColumnStyleInfo1);
			this.instructionsGrid.ColumnStyles.Add(printCheckbox);
			this.instructionsGrid.GridId = "6ae22175-fe02-11ef-b561-44045af458e4";
			this.instructionsGrid.CopySelectedRowsAllowed = false;
			this.instructionsGrid.IsCustomiseMenuVisible = false;
			this.instructionsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.instructionsGrid.LayoutKey = "instructionsGrid";
			this.instructionsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 30, true);
			this.instructionsGrid.Name = "instructionsGrid";
			this.instructionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(520, 244, true);
			this.instructionsGrid.TabIndex = 2;
			// 
			// PrintButton
			// 
			this.PrintButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PrintButton.CaptionResourceString = Res.GetData("DocumentInstructionsForm|f990bf30-fd6e-11ef-b0ff-5988bee5cf3d", "Print");
			this.PrintButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(361, 287, true);
			this.PrintButton.Name = "PrintButton";
			this.PrintButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.PrintButton.TabIndex = 3;
			// 
			// CancelPrintButton
			// 
			this.CancelPrintButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelPrintButton.CaptionResourceString = Res.GetData("DocumentInstructionsForm|f33b8066-fd6e-11ef-a6c0-0209b67eefe9", "Cancel");
			this.CancelPrintButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelPrintButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(442, 287, true);
			this.CancelPrintButton.Name = "CancelPrintButton";
			this.CancelPrintButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.CancelPrintButton.TabIndex = 4;
			// 
			// DocumentInstructionsForm
			// 
			this.AcceptButton = this.PrintButton;
			this.CancelButton = this.CancelPrintButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 336, true);
			this.Text = Res.GetString("775fc671-fe12-11ef-b106-be14a69c85da", "Select instruction to print");
			this.Controls.Add(this.PrintButton);
			this.Controls.Add(this.CancelPrintButton);
			this.Controls.Add(this.instructionsGrid);
			this.Controls.Add(this.SelectInstructionsLabel);
			this.DataSourceAssemblyName = "Enterprise.TransportBookings.Business";
			this.DataSourceType = typeof(Enterprise.TransportBookings.Business.DocumentInstructions);
			this.DataSourceTypeName = "Enterprise.TransportBookings.Business.DocumentInstructions";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 364, true);
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(540, 364, true);
			this.Name = "DocumentInstructionsForm";
			this.RememberFormPosition = false;
			this.RememberFormSize = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.SelectInstructionsLabel, 0);
			this.Controls.SetChildIndex(this.instructionsGrid, 0);
			this.Controls.SetChildIndex(this.CancelPrintButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.PrintButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.instructionsGrid)).EndInit();
			this.instructionsGrid.ResumeLayout(false);
			this.instructionsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		Enterprise.ZArchitecture.ZLabel SelectInstructionsLabel;
		Enterprise.ZArchitecture.ZGrid instructionsGrid;
		Enterprise.ZArchitecture.GUI.ZButton CancelPrintButton;
		Enterprise.ZArchitecture.GUI.ZButton PrintButton;
	}
}
