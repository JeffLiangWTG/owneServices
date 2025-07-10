using System;
using CargoWise.Windows.UI;
using Enterprise.ProcessManagement.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ProcessManagement.GUI
{
	public partial class ProjectStatusControl : ZUserControl
	{
		public ProjectStatusControl()
		{
			InitializeComponent();

			OpportunitySalesPersonCodeFindBox.ReadOnly = true;

			if (!DesignModeFinder.IsDesigning)
			{
				var colorTheme = SystemDataRegistry.Instance.ColorTheme;
				TaskStatusBox.ColorChanger.ForceBackColor(colorTheme.TabBackgroundColor);
				TaskAssignedStaffBox.ColorChanger.ForceBackColor(colorTheme.TabBackgroundColor);
				ProjectCreatedBox.ColorChanger.ForceBackColor(colorTheme.TabBackgroundColor);
				ProjectClosedOrDeferredBox.ColorChanger.ForceBackColor(colorTheme.TabBackgroundColor);
			}
		}

		Project Project
		{
			get { return (Project)CurrentDataItem; }
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (Project != null)
			{
				UpdateProjectClosedOrDeferredBox();
				Project.ClosedOrDeferredDateAsTextInfo.ValueChanged += new EventHandler(delegate
				{ UpdateProjectClosedOrDeferredBox(); });
			}
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			if (Project != null)
			{
				Project.ClosedOrDeferredDateAsTextInfo.ValueChanged -= new EventHandler(delegate
				{ UpdateProjectClosedOrDeferredBox(); });
			}
		}

		void UpdateProjectClosedOrDeferredBox()
		{
			if (Project != null && !Project.IsDeleted)
			{
				ProjectClosedOrDeferredBox.GetExtension<LabelCaptionRenderer>().Caption = Project.ClosedOrDeferredLabel;
				if (Project.HasDeferred)
				{
					if (Project.DeferredDateMet)
					{
						ProjectClosedOrDeferredBox.ForeColor = System.Drawing.Color.DarkGreen;
					}
					else
					{
						ProjectClosedOrDeferredBox.ForeColor = System.Drawing.Color.Red;
					}
				}
				else
				{
					ProjectClosedOrDeferredBox.ForeColor = System.Drawing.SystemColors.ControlText;
				}
			}
		}

#if DEBUG
		public ZArchitecture.ZTextBox GetProjectClosedOrDeferredBox()
		{
			return ProjectClosedOrDeferredBox;
		}
#endif
	}
}
