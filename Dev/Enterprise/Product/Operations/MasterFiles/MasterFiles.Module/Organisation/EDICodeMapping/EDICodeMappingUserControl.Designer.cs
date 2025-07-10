using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.MasterFiles.Module
{
	public partial class EDICodeMappingUserControl
	{
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.grid.SuspendLayout();
			this.AddStripButton.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			this.BindingSource.SetBindingMember(this.grid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgPatternMatchOverride)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgPatternMatchOverride)(null)).OO_OH)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPatternMatchOverride)(null)).OO_ForeignCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPatternMatchOverride)(null)).OO_Relationship)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPatternMatchOverride)(null)).OrgCoNameorGuid)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPatternMatchOverride)(null)).OO_Context)));
			zGuidFindBoxColumnStyleInfo.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("EDICodeMappingUserControl|1944e679-e7eb-49cf-99bb-ae60ada2b6ba", "Organization Code");
			zGuidFindBoxColumnStyleInfo.ColumnName = "OO_OH";
			zGuidFindBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zMultiControlColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("EDICodeMappingUserControl|d4bc0c94-6440-436b-a5c2-554c7f5fa216", "Foreign Code");
			zMultiControlColumnStyleInfo1.BindToDecimalPlaces = null;
			zMultiControlColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zMultiControlColumnStyleInfo1.ColumnName = "OO_ForeignCode";
			zMultiControlColumnStyleInfo1.FieldTypeColumnName = "OO_ForeignCodeFieldType";
			zMultiControlColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(275);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("EDICodeMappingUserControl|146901ed-3f47-41d3-bf59-7c8a43331fd5", "Relationship");
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "OO_Relationship";
			zTextBoxColumnStyleInfo2.ToolTip = "Type of relationship that is to be mapped to the EDI codes.";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zMultiControlColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("EDICodeMappingUserControl|d75e6a5e-e585-4639-9d27-9a94d88edeb5", "Local Code");
			zMultiControlColumnStyleInfo3.BindToDecimalPlaces = null;
			zMultiControlColumnStyleInfo3.ColumnName = "OrgCoNameorGuid";
			zMultiControlColumnStyleInfo3.FieldTypeColumnName = "OrgCoFieldType";
			zMultiControlColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("EDICodeMappingUserControl|3f7b89a8-eee0-45b0-8e3c-144e1da0ed37", "Context");
			zTextBoxColumnStyleInfo4.ColumnName = "OO_Context";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.grid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo);
			this.grid.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zMultiControlColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgPatternMatchOverride);
			// 
			// EDICodeMappingUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "EDICodeMappingUserControl";
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			this.grid.ResumeLayout(false);
			this.grid.PerformLayout();
			this.AddStripButton.ResumeLayout(true);
			this.AddStripButton.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
