using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.MasterFiles.Module
{
	public partial class WorkflowTriggersFilterControl
	{
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo7 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo8 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.MasterFiles.GUI.TriggerConditionValueColumnStyleInfo triggerConditionValueColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.TriggerConditionValueColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.EventContextColumnStyleInfo eventContextColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.EventContextColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo9 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// FilteredGrid
			// 
			zTextBoxColumnStyleInfo2.ColumnName = "P9_Description";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(220);
			zDropEditColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo5.ColumnName = "TriggerConditions+TriggerEventCode";
			zDropEditColumnStyleInfo5.IsMandatory = true;
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo6.ColumnName = "TriggerConditions+TriggerFieldName";
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo7.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zDropEditColumnStyleInfo7.ColumnName = "ReferenceCode";
			zDropEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zGuidFindBoxColumnStyleInfo4.ColumnName = "P9_GC";
			zGuidFindBoxColumnStyleInfo4.IsMandatory = true;
			zGuidFindBoxColumnStyleInfo4.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("WorkflowTriggersFilterControl|F37A668D-AEE7-4B14-A26C-B24ED868C1C9", "Event Date");
			zDateEditColumnStyleInfo1.ColumnName = "P9_ActualDateForBinding";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDropEditColumnStyleInfo8.ColumnName = "TriggerConditions.TriggerCondition";
			zDropEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			triggerConditionValueColumnStyleInfo1.ColumnName = "TriggerConditions.TriggerConditionValue";
			triggerConditionValueColumnStyleInfo1.FieldTypeColumnName = "TriggerConditions.TriggerConditionValueFieldType";
			triggerConditionValueColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo2.ColumnName = "P9_RespondToCascadedEvents";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			eventContextColumnStyleInfo1.ColumnName = "P9_CascadedEventsContext";
			eventContextColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zGuidFindBoxColumnStyleInfo5.ColumnName = "SourceTemplatePK";
			zGuidFindBoxColumnStyleInfo5.IsVisible = false;
			zGuidFindBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo9.ColumnName = "P9_LineTriggerType";
			zDropEditColumnStyleInfo9.IsVisible = false;
			zDropEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.FilteredGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.FilteredGrid.ColumnStyles.Add(zDropEditColumnStyleInfo7);
			this.FilteredGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo4);
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zDropEditColumnStyleInfo8);
			this.FilteredGrid.ColumnStyles.Add(triggerConditionValueColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(eventContextColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo5);
			this.FilteredGrid.ColumnStyles.Add(zDropEditColumnStyleInfo9);
			this.FilteredGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(738, 264, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.TriggerCollectionView);
			// 
			// WorkflowTriggersFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "WorkflowTriggersFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(738, 416, true);
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
