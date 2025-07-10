using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	class ProcessTaskFilterStrip : ZFilterStrip
	{
		protected override ZCodeFindBox CreateFindBox(IModuleFilterWithModuleID moduleFilter)
		{
			var workflowFilter = moduleFilter as WorkflowJobModuleFilter;

			if (workflowFilter != null)
			{
				return new FindBoxForWorkflowJobFilterStrip(workflowFilter) { ModuleID = moduleFilter.ModuleId };
			}

			return base.CreateFindBox(moduleFilter);
		}

		protected override Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
		{
			var controls = base.GetCurrentFilterControls(currentModuleFilter);

			if (currentModuleFilter is TasksStaffCanDoModuleFilter)
			{
				controls = AddTasksStaffCanDoControls(controls);
				var bottomOfControls = controls.MaxBySafe(c => c.Bottom).Bottom;
				PreferredHeight = bottomOfControls + ControlDpiScalingHelper.ScaleToCurrentDpiY(1);
			}

			return controls;
		}

		Control[] AddTasksStaffCanDoControls(Control[] controls)
		{
			var comparisonOperatorControl = controls.OfType<ZDropEdit>().Single(x => x.BindTo == ComparisonOperatorBindToIdentifier);
			var findBox = controls.OfType<ZCodeFindBox>().Single(x => x.Name == "PropertyFindBox");
			var modeDropEditLeft = FilterControlsBox1Start;
			var top = comparisonOperatorControl.Top + comparisonOperatorControl.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(2);
			var lastTabIndex = controls.Max(x => x.TabIndex);

			var label = new ZLabel();
			label.AutoSize = false;
			label.CaptionResourceString = Res.GetData("c8d57b7a-e3dd-4b6a-91c0-536a0d2e8334", "Filtering mode:");
			label.TextAlign = ContentAlignment.MiddleRight;
			label.TabIndex = lastTabIndex++;
			ControlDpiScalingHelper.SetTop(ref label, top, false);
			ControlDpiScalingHelper.SetWidth(ref label, modeDropEditLeft, true);
			ControlDpiScalingHelper.SetHeight(ref label, findBox.Height, false);

			var modeDropEdit = new ZDropEdit();
			modeDropEdit.Name = "TasksStaffCanDoModeDropEdit";
			modeDropEdit.TabIndex = lastTabIndex;
			ControlDpiScalingHelper.SetTop(ref modeDropEdit, top, false);
			ControlDpiScalingHelper.SetLeft(ref modeDropEdit, modeDropEditLeft, true);
			ControlDpiScalingHelper.SetWidth(ref modeDropEdit, findBox.Width, false);

			modeDropEdit.BindTo = nameof(TasksStaffCanDoModuleFilter.Mode);
			FilterControlBindingSource.SetBindingMember(modeDropEdit, modeDropEdit.BindTo);

			return controls.Concat(new Control[] { label, modeDropEdit }).ToArray();
		}

		internal class FindBoxForWorkflowJobFilterStrip : ZGuidFindBox
		{
			public FindBoxForWorkflowJobFilterStrip(WorkflowJobModuleFilter filter)
			{
				this.filter = filter;
			}

			readonly WorkflowJobModuleFilter filter;

			protected override IEnumerable<BusinessObject> GetBizObjsToEditOrView()
			{
				if (filter.Property.IsEmpty)
				{
					return Enumerable.Empty<BusinessObject>();
				}

				var workflow = IFindBox.ListProvider.List.Factory.Load<IProcessHeader>(filter.Property);
				return new[] { (BusinessObject)workflow };
			}

			protected override void OnSingleBusinessObjectSelectedInPopup(BusinessObject selectedObject)
			{
				base.OnSingleBusinessObjectSelectedInPopup(selectedObject);

				filter.Property = selectedObject.PK;
			}

#if DEBUG
			internal void ShowEditForm_ForTest()
			{
				ShowEditOrViewForm();
			}
#endif
		}
	}
}
