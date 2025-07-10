using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.GUI
{
	partial class FreightContainersUserControl
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.JC_AdditionalSealNumTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JC_Additional2SealNumTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JC_SealPartyDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.JC_AdditionalSealPartyDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.JC_Additional2SealPartyDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DetailTabControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			//
			//edSealNum
			//
			this.edSealNum.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 52, true);
			this.edSealNum.TabIndex = 4;
			//
			//DeliveryModeDropEdit
			//
			this.DeliveryModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 25, true);
			this.DeliveryModeDropEdit.TabIndex = 2;
			// WeightsGroupBox
			// 
			this.WeightsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 120, true);
			this.WeightsGroupBox.TabIndex = 10;
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 258, true);
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 110, true);
			this.DetailsGroupBox.TabIndex = 11;
			// 
			// ExportContainerModeDropEdit
			// 
			this.ExportContainerModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(233, 4, true);
			// 
			// ExportContainerTypeGuidFindBox
			// 
			this.ExportContainerTypeGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(233, 25, true);
			this.ExportContainerTypeGuidFindBox.TabIndex = 3;
			// 
			// DetailTabControl
			// 
			this.DetailTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(333, 2, true);
			this.DetailTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(477, 397, true);
			this.DetailTabControl.TabIndex = 12;
			// 
			// OutturnTabPage
			// 
			this.OutturnTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(469, 370, true);
			// 
			// JC_AdditionalSealNumTextBox
			// 
			this.BindingSource.SetBindingMember(this.JC_AdditionalSealNumTextBox, "JC_AdditionalSealNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonContainer)(null)).JC_AdditionalSealNum)));
			this.JC_AdditionalSealNumTextBox.CaptionResourceString = null;
			this.JC_AdditionalSealNumTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 73, true);
			this.JC_AdditionalSealNumTextBox.Name = "JC_AdditionalSealNumTextBox";
			this.JC_AdditionalSealNumTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 20, true);
			this.JC_AdditionalSealNumTextBox.TabIndex = 6;
			// 
			// JC_Additional2SealNumTextBox
			// 
			this.BindingSource.SetBindingMember(this.JC_Additional2SealNumTextBox, "JC_Additional2SealNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonContainer)(null)).JC_Additional2SealNum)));
			this.JC_Additional2SealNumTextBox.CaptionResourceString = null;
			this.JC_Additional2SealNumTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 94, true);
			this.JC_Additional2SealNumTextBox.Name = "JC_Additional2SealNumTextBox";
			this.JC_Additional2SealNumTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 20, true);
			this.JC_Additional2SealNumTextBox.TabIndex = 8;
			// 
			// JC_SealPartyDropEdit
			// 
			this.JC_SealPartyDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JC_SealPartyDropEdit, "JC_SealParty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.CommonContainer)(null)).JC_SealParty)));
			this.JC_SealPartyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(242, 52, true);
			this.JC_SealPartyDropEdit.CaptionResourceString = Res.GetData("6acf1e45-53f7-4441-951b-0d5edd9ff533", "Sealed By");
			this.JC_SealPartyDropEdit.Name = "JC_SealPartyDropEdit";
			this.JC_SealPartyDropEdit.PreBoundMaxLength = 3;
			this.JC_SealPartyDropEdit.ShowDescriptionBox = false;
			this.JC_SealPartyDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.JC_SealPartyDropEdit.TabIndex = 5;
			// 
			// JC_AdditionalSealPartyDropEdit
			// 
			this.JC_AdditionalSealPartyDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JC_AdditionalSealPartyDropEdit, "JC_AdditionalSealParty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.CommonContainer)(null)).JC_AdditionalSealParty)));
			this.JC_AdditionalSealPartyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(242, 73, true);
			this.JC_AdditionalSealPartyDropEdit.CaptionResourceString = Res.GetData("c6c816c0-eaff-4bde-9182-cd4cc8766648", "Sealed By");
			this.JC_AdditionalSealPartyDropEdit.Name = "JC_AdditionalSealPartyDropEdit";
			this.JC_AdditionalSealPartyDropEdit.PreBoundMaxLength = 3;
			this.JC_AdditionalSealPartyDropEdit.ShowDescriptionBox = false;
			this.JC_AdditionalSealPartyDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.JC_AdditionalSealPartyDropEdit.TabIndex = 7;
			// 
			// JC_Additional2SealPartyDropEdit
			// 
			this.JC_Additional2SealPartyDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JC_Additional2SealPartyDropEdit, "JC_Additional2SealParty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.CommonContainer)(null)).JC_Additional2SealParty)));
			this.JC_Additional2SealPartyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(242, 94, true);
			this.JC_Additional2SealPartyDropEdit.CaptionResourceString = Res.GetData("d53414b6-3146-4d5c-aec0-222d6bec57a9", "Sealed By");
			this.JC_Additional2SealPartyDropEdit.Name = "JC_Additional2SealPartyDropEdit";
			this.JC_Additional2SealPartyDropEdit.PreBoundMaxLength = 3;
			this.JC_Additional2SealPartyDropEdit.ShowDescriptionBox = false;
			this.JC_Additional2SealPartyDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.JC_Additional2SealPartyDropEdit.TabIndex = 9;
			// 
			// FreightContainersUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.Controls.Add(this.JC_Additional2SealNumTextBox);
			this.Controls.Add(this.JC_AdditionalSealNumTextBox);
			this.Controls.Add(this.JC_SealPartyDropEdit);
			this.Controls.Add(this.JC_AdditionalSealPartyDropEdit);
			this.Controls.Add(this.JC_Additional2SealPartyDropEdit);
			this.Name = "FreightContainersUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 404, true);
			this.Controls.SetChildIndex(this.DetailsGroupBox, 0);
			this.Controls.SetChildIndex(this.WeightsGroupBox, 0);
			this.Controls.SetChildIndex(this.DetailTabControl, 0);
			this.Controls.SetChildIndex(this.JC_AdditionalSealNumTextBox, 0);
			this.Controls.SetChildIndex(this.JC_Additional2SealNumTextBox, 0);
			this.Controls.SetChildIndex(this.JC_SealPartyDropEdit, 0);
			this.Controls.SetChildIndex(this.JC_AdditionalSealPartyDropEdit, 0);
			this.Controls.SetChildIndex(this.JC_Additional2SealPartyDropEdit, 0);
			this.DetailTabControl.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZArchitecture.ZTextBox JC_AdditionalSealNumTextBox;
		ZArchitecture.ZTextBox JC_Additional2SealNumTextBox;
		ZArchitecture.GUI.ZDropEdit JC_SealPartyDropEdit;
		ZArchitecture.GUI.ZDropEdit JC_AdditionalSealPartyDropEdit;
		ZArchitecture.GUI.ZDropEdit JC_Additional2SealPartyDropEdit;
	}
}
