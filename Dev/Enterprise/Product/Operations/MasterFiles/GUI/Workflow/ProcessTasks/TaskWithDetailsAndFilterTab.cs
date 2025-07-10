using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public class TaskWithDetailsAndFilterTab : ZBindingTabPage
	{
		public TaskWithDetailsAndFilterControl FilterControl
		{
			get
			{
				if (filterControl == null)
				{
					filterControl = GetNewFilterControl();
					filterControl.CreateTasksFromTemplateLinkVisible = createTasksFromTemplateLinkVisible;
				}
				return filterControl;
			}
		}
		TaskWithDetailsAndFilterControl filterControl;

		protected virtual TaskWithDetailsAndFilterControl GetNewFilterControl()
		{
			return new TaskWithDetailsAndFilterControl();
		}

		[Browsable(true), Category(ZGUIConstants.DesignerCategory), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public bool CreateTasksFromTemplateLinkVisible
		{
			get { return createTasksFromTemplateLinkVisible; }
			set
			{
				if (filterControl != null)
				{
					filterControl.CreateTasksFromTemplateLinkVisible = value;
				}
				createTasksFromTemplateLinkVisible = value;
			}
		}
		protected bool createTasksFromTemplateLinkVisible = true;

		protected override void SetDataBindingCore(object dataSource, string dataMember)
		{
			if (Controls.Count == 0)
			{
				FilterControl.Dock = DockStyle.Fill;
				Controls.Add(FilterControl);
			}
			base.SetDataBindingCore(dataSource, dataMember);
		}
	}
}
