using System;
using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.MasterFiles.GUI
{
	#region EventContextColumnStyleInfo

#if DEBUG
	[SuppressCheckControlModuleId]
	[SuppressCheckControlLookupList]
#endif
	public class EventContextColumnStyleInfo : ZCodeFindBoxColumnStyleInfo
	{
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override Type ColumnStyleType
		{
			get { return typeof(EventContextColumnStyle); }
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public WorkflowDescriptor WorkflowDescriptor { get; set; }
	}

	#endregion

	#region EventContextColumnStyle

	public class EventContextColumnStyle : ZCodeFindBoxColumnStyle
	{
		public EventContextColumnStyle(EventContextColumnStyleInfo columnInfo) : this(() => new EventContextFindBox(), columnInfo) { }

		protected EventContextColumnStyle(Func<EventContextFindBox> gridFindBox, EventContextColumnStyleInfo columnInfo) : base(gridFindBox, columnInfo) { }

		protected new EventContextColumnStyleInfo ColumnInfo
		{
			get { return (EventContextColumnStyleInfo)base.ColumnInfo; }
		}

		protected new EventContextFindBox FindBox
		{
			get { return (EventContextFindBox)base.FindBox; }
		}

		protected override void Edit(CurrencyManager source, int rowNum, System.Drawing.Rectangle bounds, bool readOnly, string instantText, bool cellVisible)
		{
			if (!IsEditing)
			{
				if (ColumnInfo.WorkflowDescriptor != null)
				{
					FindBox.WorkflowDescriptor = ColumnInfo.WorkflowDescriptor;
				}
				else if (source.List != null && source.List.Count > rowNum && rowNum >= 0)
				{
					FindBox.WorkflowDescriptor = GetWorkflowDescriptor(source.List[rowNum]);
				}
			}

			base.Edit(source, rowNum, bounds, readOnly, instantText, cellVisible);
		}

#if DEBUG
		internal
#endif
		WorkflowDescriptor GetWorkflowDescriptor(object component)
		{
			IWorkflowProvider workflowProvider;
			IWorkflowItem workflowItem;

			if ((workflowItem = component as IWorkflowItem) != null)
			{
				return workflowItem.GetWorkflowDescriptor();
			}
			else if ((workflowProvider = component as IWorkflowProvider) != null)
			{
				return WorkflowDescriptors.Instance.TryGetValueSafe(workflowProvider.WorkflowType);
			}

			return null;
		}
	}

	#endregion

	#region EventContextFindBox

	public class EventContextFindBox : ZGridFindBox
	{
		public WorkflowDescriptor WorkflowDescriptor { get; set; }

		public override void SelectFromPopupForm(bool autoSelect = false)
		{
			eventContextBizo = new WorkflowEventContextBizo(WorkflowDescriptor);

			try
			{
				eventContextBizo.ContextCollection.Load(WorkflowDescriptor.GetContextPathFromString(Code, false));
			}
			catch (InvalidOperationException ex)
			{
				Globals.Message.Show(ex.Message);
			}

			var form = new EventContextForm(eventContextBizo);
			form.Closed += ContextFormClosed;
			ZFormModaliser.ShowDialogAndDispose(form, FindForm());
		}

		void ContextFormClosed(object sender, EventArgs e)
		{
			var form = sender as EventContextForm;
			if (form != null)
			{
				form.Closed -= ContextFormClosed;
				if (form.DialogResult == DialogResult.OK && eventContextBizo != null)
				{
					Code = eventContextBizo.ContextPathCode;
				}
				form.Dispose();
			}
			eventContextBizo = null;
		}

		WorkflowEventContextBizo eventContextBizo;

		protected override System.Collections.IList GetList(object dataSource, string listMember, string dataMemberForErrorReporting)
		{
			return null;
		}
	}

	#endregion
}
