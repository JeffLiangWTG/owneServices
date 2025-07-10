using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Freight.Business;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Freight.QuotedBookings.GUI
{
	/// <summary>
	/// Summary description for BuildConsolForm.
	/// </summary>
	public partial class BuildConsolForm : ZChildForm
	{
		private ZGroupBox ContainersGroupBox;
		private ZArchitecture.ZGrid ContainersGrid;
		private new ZButton CancelButton;
		private ZButton BuildConsolButton;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new ZDropEditColumnStyleInfo();
			this.ContainersGroupBox = new ZGroupBox();
			this.ContainersGrid = new ZArchitecture.ZGrid();
			this.CancelButton = new ZButton();
			this.BuildConsolButton = new ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ContainersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ContainersGrid)).BeginInit();
			this.SuspendLayout();
			//
			// MainStatusBar
			//
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 221, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 23, true);
			this.MainStatusBar.SizingGrip = false;
			//
			// MessageStatusBarPanel
			//
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			//
			// ErrorStatusBarPanel
			//
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(PackContainerHelper);
			//
			// ContainersGroupBox
			//
			this.ContainersGroupBox.CaptionResourceString = Res.GetData("BuildConsolForm|e881edf0-5c37-448b-a12f-f501fa53da48", "Containers");
			this.ContainersGroupBox.Controls.Add(this.ContainersGrid);
			this.ContainersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 7, true);
			this.ContainersGroupBox.Name = "ContainersGroupBox";
			this.ContainersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 164, true);
			this.ContainersGroupBox.TabIndex = 1;
			this.ContainersGroupBox.TabStop = false;
			//
			// ContainersGrid
			//
			this.ContainersGrid.AllowNavigation = false;
			this.ContainersGrid.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
									| System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right);
			this.BindingSource.SetBindingMember(this.ContainersGrid, "Containers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PackContainerHelper)(null)).Containers);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Forwarding.Business.ForwardingContainer)(((System.Collections.IList)(((PackContainerHelper)(null)).Containers)).SyncRoot)).JC_ContainerNum);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Forwarding.Business.ForwardingContainer)(((System.Collections.IList)(((PackContainerHelper)(null)).Containers)).SyncRoot)).JC_Calc_ConsolID);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Forwarding.Business.ForwardingContainer)(((System.Collections.IList)(((PackContainerHelper)(null)).Containers)).SyncRoot)).JC_RC);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Forwarding.Business.ForwardingContainer)(((System.Collections.IList)(((PackContainerHelper)(null)).Containers)).SyncRoot)).JC_ContainerMode);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Forwarding.Business.ForwardingContainer)(((System.Collections.IList)(((PackContainerHelper)(null)).Containers)).SyncRoot)).JC_SealNum);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Forwarding.Business.ForwardingContainer)(((System.Collections.IList)(((PackContainerHelper)(null)).Containers)).SyncRoot)).JC_Calc_ContainerCapacity);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Forwarding.Business.ForwardingContainer)(((System.Collections.IList)(((PackContainerHelper)(null)).Containers)).SyncRoot)).JC_Calc_MaxGrossWeight);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Forwarding.Business.ForwardingContainer)(((System.Collections.IList)(((PackContainerHelper)(null)).Containers)).SyncRoot)).JC_SealParty);
			this.ContainersGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Res.GetData("BuildConsolForm|457f1dcd-4312-47c5-a7bc-2d1dcbdff68e", "Container Num.", "Container Number");
			zTextBoxColumnStyleInfo1.ColumnName = "JC_ContainerNum";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Res.GetData("BuildConsolForm|2c892646-4704-46e0-bbf8-ceda966f22c8", "Consol ID");
			zTextBoxColumnStyleInfo2.ColumnName = "JC_Calc_ConsolID";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Res.GetData("BuildConsolForm|ac224e29-29d3-4e9f-b823-8aebb21c8991", "Type");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "JC_RC";
			zGuidFindBoxColumnStyleInfo1.IsMandatory = true;
			zGuidFindBoxColumnStyleInfo1.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDropEditColumnStyleInfo1.CaptionResourceString = Res.GetData("BuildConsolForm|4e4ef02f-e0b4-4319-ac89-26cf507e2e0f", "Mode");
			zDropEditColumnStyleInfo1.ColumnName = "JC_ContainerMode";
			zDropEditColumnStyleInfo1.IsReadOnly = true;
			zDropEditColumnStyleInfo1.IsVisible = false;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Res.GetData("BuildConsolForm|18d2c7fe-f846-4846-a92a-71b84238e901", "Seal Num.", "Seal Number");
			zTextBoxColumnStyleInfo3.ColumnName = "JC_SealNum";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Res.GetData("BuildConsolForm|3934a111-3c1c-4a8d-b865-24bee5838a8b", "Capacity");
			zCalcEditColumnStyleInfo1.ColumnName = "JC_Calc_ContainerCapacity";
			zCalcEditColumnStyleInfo1.IsVisible = false;
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Res.GetData("BuildConsolForm|e8830260-a9e8-43be-95f9-e509dc68d2f4", "Max Gross Wt");
			zCalcEditColumnStyleInfo2.ColumnName = "JC_Calc_MaxGrossWeight";
			zCalcEditColumnStyleInfo2.IsVisible = false;
			zDropEditColumnStyleInfo2.ColumnName = "JC_SealParty";
			zDropEditColumnStyleInfo2.IsReadOnly = true;
			zDropEditColumnStyleInfo2.IsVisible = false;
			this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ContainersGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.ContainersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ContainersGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ContainersGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ContainersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ContainersGrid.GridId = "c0da2232-d9cc-43c4-85e1-b2a3ed226027";
			this.ContainersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ContainersGrid.IsWholeRowSelectedOnClick = true;
			this.ContainersGrid.LayoutKey = "ContainersGrid";
			this.ContainersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 15, true);
			this.ContainersGrid.Name = "ContainersGrid";
			this.ContainersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 141, true);
			this.ContainersGrid.TabIndex = 0;
			//
			// CancelButton
			//
			this.CancelButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.CancelButton.CaptionResourceString = Res.GetData("BuildConsolForm|5a5348c1-303e-4237-87b4-4016ce1deed9", "Cancel");
			this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 195, true);
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 22, true);
			this.CancelButton.TabIndex = 3;
			this.CancelButton.Click += new EventHandler(this.CancelButton_Click);
			//
			// BuildConsolButton
			//
			this.BuildConsolButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.BuildConsolButton.CaptionResourceString = Res.GetData("BuildConsolForm|479a4476-946d-458c-b63d-8ca30b07907c", "Build Consol");
			this.BuildConsolButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(184, 195, true);
			this.BuildConsolButton.Name = "BuildConsolButton";
			this.BuildConsolButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 22, true);
			this.BuildConsolButton.TabIndex = 2;
			this.BuildConsolButton.Click += new EventHandler(this.BuildConsolButton_Click);
			//
			// BuildConsolForm
			//

			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 244, true);
			this.CaptionResourceString = Res.GetData("BuildConsolForm|838aa809-e9aa-473c-99bd-eec9a9325769", "Build Consol");
			this.Controls.Add(this.BuildConsolButton);
			this.Controls.Add(this.CancelButton);
			this.Controls.Add(this.ContainersGroupBox);
			this.DataSourceAssemblyName = "Enterprise.Freight.QuotedBookings.Business";
			this.DataSourceType = typeof(PackContainerHelper);
			this.DataSourceTypeName = "Enterprise.Freight.QuotedBookings.Business.PackContainerHelper";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.Name = "BuildConsolForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ContainersGroupBox, 0);
			this.Controls.SetChildIndex(this.CancelButton, 0);
			this.Controls.SetChildIndex(this.BuildConsolButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ContainersGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ContainersGrid)).EndInit();
			this.ResumeLayout(false);
		}
		#endregion

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

	}
}
