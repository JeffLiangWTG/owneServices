using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Freight.Business;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Freight.QuotedBookings.GUI
{
	/// <summary>
	/// Summary description for BuildConsolForm.
	/// </summary>
	public partial class BuildConsolForm : ZChildForm
	{
		public BuildConsolForm(PackContainerHelper sailingHelper)
			: base(sailingHelper)
		{
			InitializeComponent();
			this.Sailing = sailingHelper.Sailing;
		}

		readonly JobSailing Sailing;
		private void BuildConsolButton_Click(object sender, EventArgs e)
		{
			if (ContainersGrid.SelectedElements.Length > 0)
			{
				var helper = new BuildConsolHelper();
				var selectedContainers = ContainersGrid.SelectedElements.OfType<CommonContainer>().ToArray();
				if (!helper.CheckHasConsolidatedContainer(selectedContainers))
				{
					var controller = ZControllerFactory.Create(ControllerIDs.JobConsol);
					controller.SetFormsModalTo(this);

					controller.ShowNewForm();
					var newConsol = ((CommonConsol)((ZForm)controller.LastShownForm).BusinessEntity);
					helper.BuildConsolFromPackContainers(newConsol, selectedContainers, Sailing);
					((Form)controller.LastShownForm).Closed += new EventHandler(BuildConsolForm_Closed);
				}
				else
				{
					AllocatedContainersSelected();
				}
			}
			else
			{
				Globals.Message.Show(Res.GetString("d154dc36-b196-4bd8-ba56-ad36f4046fbd", "Please select containers to consolidate."), Res.GetString("14e40827-0538-4c14-b5de-bcdb6486853e", "Build Consol"), MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}

		protected void AllocatedContainersSelected()
		{
			Globals.Message.Show(Res.GetString("a4354ba8-c121-44c8-b945-b305ae1a991b", "Some containers selected are already consolidated."), Res.GetString("3a8baa53-0ebb-4240-8ffc-df7091766a22", "Consolidated Containers"), MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		private void BuildConsolForm_Closed(object sender, EventArgs e)
		{
			this.Close();
		}

		private void CancelButton_Click(object sender, EventArgs e)
		{
			this.Close();
		}
	}
}
