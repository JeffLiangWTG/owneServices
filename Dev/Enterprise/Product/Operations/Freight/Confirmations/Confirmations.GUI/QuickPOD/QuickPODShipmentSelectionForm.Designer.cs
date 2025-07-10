using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
namespace Enterprise.Freight.Confirmations.GUI
{
	partial class QuickPODShipmentSelectionForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.msgLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ShipmentsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.okButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ShipmentsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 178, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(516, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(CommonShipment);
			// 
			// msgLabel
			// 
			this.msgLabel.AutoSize = true;
			this.msgLabel.CaptionResourceString = Enterprise.Freight.Confirmations.GUI.Res.GetData("QuickPODShipmentSelectionForm|1c2713f5-d1bd-40d3-8011-a7b33bfb35d4", "There are multiple Shipments with house bill \'\'. Please choose from the list below and select OK.");
			this.msgLabel.ForeColor = System.Drawing.Color.Green;
			this.msgLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.msgLabel.Name = "msgLabel";
			this.msgLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(461, 13, true);
			this.msgLabel.TabIndex = 1;
			// 
			// ShipmentsGrid
			// 
			this.ShipmentsGrid.AllowBeginDrag = false;
			this.ShipmentsGrid.AllowDragDropWithChanges = false;
			this.ShipmentsGrid.AllowNavigation = false;
			this.ShipmentsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ShipmentsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((CommonShipment)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CommonShipment)(null)).JS_UniqueConsignRef)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CommonShipment)(null)).JS_TransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CommonShipment)(null)).JS_PackingMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CommonShipment)(null)).ConsignorNameOrPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CommonShipment)(null)).ConsignorFieldType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CommonShipment)(null)).ConsigneeNameOrPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CommonShipment)(null)).ConsigneeFieldType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CommonShipment)(null)).JS_RL_NKOrigin)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CommonShipment)(null)).JS_RL_NKDestination)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((CommonShipment)(null)).JS_E_DEP)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((CommonShipment)(null)).JS_E_ARV)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((CommonShipment)(null)).JS_OuterPacks)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CommonShipment)(null)).JS_F3_NKPackType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((CommonShipment)(null)).JS_ActualWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CommonShipment)(null)).JS_UnitOfWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((CommonShipment)(null)).JS_ActualVolume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CommonShipment)(null)).JS_UnitOfVolume)));
			this.ShipmentsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "JS_UniqueConsignRef";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.ColumnName = "JS_TransportMode";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo3.ColumnName = "JS_PackingMode";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zMultiControlColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Confirmations.GUI.Res.GetData("QuickPODShipmentSelectionForm|673fb99a-d264-419b-80bd-0b661015ab4a", "Consignor", "This represents the shipments consignor.");
			zMultiControlColumnStyleInfo1.ColumnName = "ConsignorNameOrPK";
			zMultiControlColumnStyleInfo1.FieldTypeColumnName = "ConsignorFieldType";
			zMultiControlColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Confirmations.GUI.Res.GetData("QuickPODShipmentSelectionForm|fe2a055e-c119-4a7d-868a-0499d93b0245", "Consignee", "This represents the shipments consignee.");
			zMultiControlColumnStyleInfo2.ColumnName = "ConsigneeNameOrPK";
			zMultiControlColumnStyleInfo2.FieldTypeColumnName = "ConsigneeFieldType";
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.ColumnName = "JS_RL_NKOrigin";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo5.ColumnName = "JS_RL_NKDestination";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zDateEditColumnStyleInfo1.ColumnName = "JS_E_DEP";
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo2.ColumnName = "JS_E_ARV";
			zDateEditColumnStyleInfo2.IsReadOnly = true;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "JS_OuterPacks";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.GroupName = Enterprise.Freight.Confirmations.GUI.Res.GetData("QuickPODShipmentSelectionForm|7abc1ec7-b941-4b98-803c-ea60907637e6", "Packages");
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo6.ColumnName = "JS_F3_NKPackType";
			zTextBoxColumnStyleInfo6.GroupName = Enterprise.Freight.Confirmations.GUI.Res.GetData("QuickPODShipmentSelectionForm|7abc1ec7-b941-4b98-803c-ea60907637e6", "Packages");
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "JS_ActualWeight";
			zCalcEditColumnStyleInfo2.Decimals = 3;
			zCalcEditColumnStyleInfo2.GroupName = Enterprise.Freight.Confirmations.GUI.Res.GetData("QuickPODShipmentSelectionForm|a24177f2-a85d-4f8c-aa73-5e6649eb964f", "Weight");
			zCalcEditColumnStyleInfo2.IsReadOnly = true;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo7.ColumnName = "JS_UnitOfWeight";
			zTextBoxColumnStyleInfo7.GroupName = Enterprise.Freight.Confirmations.GUI.Res.GetData("QuickPODShipmentSelectionForm|a24177f2-a85d-4f8c-aa73-5e6649eb964f", "Weight");
			zTextBoxColumnStyleInfo7.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "JS_ActualVolume";
			zCalcEditColumnStyleInfo3.Decimals = 3;
			zCalcEditColumnStyleInfo3.GroupName = Enterprise.Freight.Confirmations.GUI.Res.GetData("QuickPODShipmentSelectionForm|e5c86f1b-0ca2-4ed0-b890-a654877a8cef", "Volume");
			zCalcEditColumnStyleInfo3.IsReadOnly = true;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo8.ColumnName = "JS_UnitOfVolume";
			zTextBoxColumnStyleInfo8.GroupName = Enterprise.Freight.Confirmations.GUI.Res.GetData("QuickPODShipmentSelectionForm|e5c86f1b-0ca2-4ed0-b890-a654877a8cef", "Volume");
			zTextBoxColumnStyleInfo8.IsReadOnly = true;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			this.ShipmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ShipmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ShipmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ShipmentsGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
			this.ShipmentsGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo2);
			this.ShipmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ShipmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.ShipmentsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ShipmentsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.ShipmentsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ShipmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.ShipmentsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ShipmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.ShipmentsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.ShipmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.ShipmentsGrid.GridId = "7d0477eb-4796-41b7-ac44-2a315c20541b";
			this.ShipmentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ShipmentsGrid.IsCustomiseMenuVisible = false;
			this.ShipmentsGrid.IsWholeRowSelectedOnClick = true;
			this.ShipmentsGrid.LayoutKey = "ShipmentsGrid";
			this.ShipmentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 31, true);
			this.ShipmentsGrid.Name = "ShipmentsGrid";
			this.ShipmentsGrid.ReadOnly = true;
			this.ShipmentsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.ShipmentsGrid.ShouldSetErrorsOnTabPage = false;
			this.ShipmentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(516, 103, true);
			this.ShipmentsGrid.TabIndex = 2;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.CaptionResourceString = Enterprise.Freight.Confirmations.GUI.Res.GetData("QuickPODShipmentSelectionForm|84d102c0-012e-4a19-923a-def27be5336e", "OK");
			this.okButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(400, 135, true);
			this.okButton.Name = "okButton";
			this.okButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 23, true);
			this.okButton.TabIndex = 3;
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// QuickPODShipmentSelectionForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(516, 202, true);
			this.ControlBox = false;
			this.CaptionResourceString = Enterprise.Freight.Confirmations.GUI.Res.GetData("QuickPODShipmentSelectionForm|df980094-f8dd-473b-9bad-bf2b54484775", "Quick POD - Select Shipment");
			this.Controls.Add(this.ShipmentsGrid);
			this.Controls.Add(this.msgLabel);
			this.Controls.Add(this.okButton);
			this.DataSourceAssemblyName = "Enterprise.Freight.Forwarding.Business";
			this.DataSourceType = typeof(CommonShipment);
			this.DataSourceTypeName = "CommonShipment";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(524, 218, true);
			this.Name = "QuickPODShipmentSelectionForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.okButton, 0);
			this.Controls.SetChildIndex(this.msgLabel, 0);
			this.Controls.SetChildIndex(this.ShipmentsGrid, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ShipmentsGrid)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZLabel msgLabel;
		private Enterprise.ZArchitecture.ZGrid ShipmentsGrid;
		private Enterprise.ZArchitecture.GUI.ZButton okButton;
	}
}
