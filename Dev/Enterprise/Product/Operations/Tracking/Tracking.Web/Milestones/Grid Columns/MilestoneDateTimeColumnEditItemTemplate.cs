using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	/// <summary>
	/// Summary description for ZDateTimeColumnEditItemTemplate.
	/// </summary>
	public class MilestoneDateTimeColumnEditItemTemplate : ZDateTimeColumnEditItemTemplate
	{
		public MilestoneDateTimeColumnEditItemTemplate(MilestoneDateTimeColumn column)
			: base(column)
		{
		}

		protected override ISelfBindingWebControl GetControlCore()
		{
			ZDateEdit editControl = (ZDateEdit)base.GetControlCore();
			return editControl;
		}

		protected override void container_DataBinding(object sender, EventArgs e)
		{
			bool readOnly = true;

			TableCell cell = sender as TableCell;

			if (!Column.ReadOnly)
			{
				DataGridItem item = cell.NamingContainer as DataGridItem;

				IUpdatableMilestoneEventsProvider milestoneProvider = null;
				TrackingMilestone milestone = null;

				if (Column != null && Column.ZOwner != null)
				{
					if (Column.ZOwner.DataSource is TrackingMilestoneCollection)
					{
						milestoneProvider = ((TrackingMilestoneCollection)Column.ZOwner.DataSource).MilestoneEventsProvider;
					}
				}
				if (item != null && item.DataItem != null)
				{
					milestone = item.DataItem as TrackingMilestone;
				}

				if (milestone != null & milestoneProvider != null)
				{
					if (milestoneProvider.UpdatableMilestoneEventCodes.Contains(milestone.EventCode))
					{
						readOnly = Column.ReadOnly;
					}
				}
			}

			foreach (Control control in cell.Controls)
			{
				if (control is ZDateEdit)
				{
					((ZDateEdit)control).ReadOnly = readOnly;
				}
			}

			base.container_DataBinding(sender, e);
		}
	}
}
