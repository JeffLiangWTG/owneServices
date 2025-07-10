using System;
using System.Linq;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.GUI
{
	public partial class VoyageRelatedJobsControl : ZUserControl
	{
		public VoyageRelatedJobsControl()
		{
			InitializeComponent();
		}

		void OpenJobButton_Click(object sender, EventArgs e)
		{
			EditSelectedJob();
		}

		void RelatedJobsGrid_DoubleClick(object sender, EventArgs e)
		{
			EditSelectedJob();
		}

		void EditSelectedJob()
		{
			var selectedJob = RelatedJobsGrid.SelectedElements.FirstOrDefault() as VoyageRelatedJob;
			if (selectedJob != null)
			{
				if (selectedJob.CanBeEditedByCurrentCompany)
				{
					if (selectedJob.JobType != null)
					{
						var controller = ZControllerFactory.Create(selectedJob.JobType.ControllerID);
						controller.SetFormsModalTo(ParentForm);
						controller.ShowEditForm(selectedJob.BizObj);
					}
				}
				else
				{
					Globals.Message.ShowInformation(ResString.GetMultilingualString("70543c94-265e-4695-b709-a71108867727", "The selected job belongs to another Company so cannot be edited."));
				}
			}
			else
			{
				Globals.Message.ShowInformation(ResString.GetMultilingualString("97f8a609-7f09-4b76-b6b8-a9a1ada4988d", "Please select an item in the grid."));
			}
		}
	}
}
