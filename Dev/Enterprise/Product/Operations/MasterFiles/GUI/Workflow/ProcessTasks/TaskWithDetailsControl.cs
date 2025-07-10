using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Tools;

namespace Enterprise.MasterFiles.GUI
{
	public partial class TaskWithDetailsControl : ZUserControl, IBindTo
	{
		public TaskWithDetailsControl()
		{
			InitializeComponent();

			var processTaskStatusChangeModeTracker = ObjectFactory.Get<ProcessTaskStatusChangeModeTracker>();

			SimpleStatusDropEdit.Enter += (_, __) => processTaskStatusChangeModeTracker.SetCurrent(ProcessTaskStatusChangeModeCodeList.Codes.TasksTabDetailsSimpleView);
			SimpleStatusDropEdit.Leave += (_, __) => processTaskStatusChangeModeTracker.Clear();

			StatusDropEdit.Enter += (_, __) => processTaskStatusChangeModeTracker.SetCurrent(ProcessTaskStatusChangeModeCodeList.Codes.TasksTabDetailsAdvancedView);
			StatusDropEdit.Leave += (_, __) => processTaskStatusChangeModeTracker.Clear();

			ScheduleGroupBox.AllowOutsideOfParent();
			DetailsGroupBox.AllowOutsideOfParent();
			NotesGroupBox.AllowOutsideOfParent();
			ExtraResourcesGroupBox.AllowOutsideOfParent();
			ReminderCheckBox.AllowOutsideOfParent();
			LastEstTimeToCompleteTimeEditEx.AllowOutsideOfParent();
			EstimateVariationFactorCalcEdit.AllowOutsideOfParent();

			TasksSplitContainer.Panel1.AllowOutsideOfParent();
			TasksSplitContainer.Panel2.AllowOutsideOfParent();

			CustomiseTabsForWorkflowManagementMode();
		}

		protected override void OnLoad(EventArgs e)
		{
			SpellChecker.InitialiseSpellcheck(SimpleNotesRichTextBox, nameof(TaskWithDetailsControl) + nameof(SimpleNotesRichTextBox));
			SpellChecker.InitialiseSpellcheck(NotesTextBox, nameof(TaskWithDetailsControl) + nameof(NotesTextBox));

			base.OnLoad(e);
		}

		void CustomiseTabsForWorkflowManagementMode()
		{
			if (DesignMode || DesignModeFinder.IsDesigning)
			{
				return;
			}
		}

		void DetailsTabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			SetWorkflowDropEditWidths();
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			SetWorkflowDropEditWidths();
		}

		void SetWorkflowDropEditWidths()
		{
			((ZDropEditInternals)ProcessHeaderDropEdit).SetControlWidth(Math.Min(ProcessHeaderDropEdit.MaximumSize.Width, ScheduleGroupBox.Width - ProcessHeaderDropEdit.Left - ControlDpiScalingHelper.ScaleToCurrentDpiX(20)));
			((ZDropEditInternals)SimpleWorkflowGuidDropEdit).SetControlWidth(Math.Min(SimpleWorkflowGuidDropEdit.MaximumSize.Width, SimpleScheduleGroupBox.Width - SimpleWorkflowGuidDropEdit.Left - ControlDpiScalingHelper.ScaleToCurrentDpiX(20)));
		}

		#region Binding

		[Browsable(true), Category(ZGUIConstants.DesignerCategory), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue("")]
		public string BindTo
		{
			get { return TasksControl.BindTo; }
			set
			{
				TasksControl.BindTo = value;

				var groupBoxes = DetailsTabControl.TabPages.Cast<ZTabPage>().SelectMany(page => page.Controls.OfType<ZGroupBox>());

				foreach (var bindableControl in groupBoxes.SelectMany(p => p.Controls.OfType<IBindTo>()))
				{
					bindableControl.BindTo = value + "." + bindableControl.BindTo;
				}
			}
		}

		#endregion
	}
}
