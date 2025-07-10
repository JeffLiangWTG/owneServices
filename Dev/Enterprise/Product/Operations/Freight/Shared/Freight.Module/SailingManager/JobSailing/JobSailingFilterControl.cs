using System;
using System.Text;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Module
{
	public partial class JobSailingFilterControl : ZFilterStripControl
	{
		public JobSailingFilterControl()
		{
			InitializeComponent();
		}

		public JobSailingFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitialiseFilterControl();
			if (HideStowPlanColumn)
			{
				FilteredGrid.RemoveFromAvailableColumns(JobSailing.Schema.StowPlanMessageStatus, JobSailing.Schema.StowPlanMessageStatusDescription);
			}
		}

		protected override ZFilterStrip NewZFilterStrip()
			=> new JobSeaSailingModuleStrip();

		protected virtual bool HideStowPlanColumn { get { return true; } }

		void InitialiseFilterControl()
		{
			MenuItem buildConsol = new ZMenuItem(ResString.GetMultilingualString("Freight.Sailing.BuildConsol", "Build Consol"));
			buildConsol.Click += new EventHandler(BuildConsolMenuItemClick);
			FilteredGrid.ContextMenu.MenuItems.Add(0, buildConsol);

			MenuItem loadList = new ZMenuItem(ResString.GetMultilingualString("Freight.Sailing.ViewLoadList", "View Load List"));
			loadList.Click += new EventHandler(LoadList_Click);
			FilteredGrid.ContextMenu.MenuItems.Add(1, loadList);

			MenuItem divider = new ZMenuItem("-");
			FilteredGrid.ContextMenu.MenuItems.Add(2, divider);
			this.InitializeComponent();
		}

		public virtual void SetColumns()
		{
		}

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}

			base.Dispose(disposing);
		}

		#endregion

		void LoadList_Click(object sender, EventArgs e)
		{
			BusinessObject[] selectedSailings = GetSelectedSailings();
			int maximumAllowedViews = 5;
			ZController controller = ZControllerFactory.Create(ControllerIDs.LoadList);

			if (selectedSailings.Length > maximumAllowedViews)
			{
				Globals.Message.Show(Res.GetString("ea48bbe1-8c5a-40ef-99af-669d0726f0d8", "Please select no more than {0} sailings to view load lists. Viewing more at the same time can put unnecessary strain on the system.", maximumAllowedViews));
			}
			else
			{
				foreach (BusinessObject element in selectedSailings)
				{
					controller.ShowEditForm(element);
				}
			}
		}

		void BuildConsolMenuItemClick(object sender, EventArgs e)
		{
			BuildConsols();
		}

		protected virtual void BuildConsols()
		{
			const int maximumAllowedBuilds = 5;
			var selectedSailings = GetSelectedSailings();

			if (selectedSailings.Length <= maximumAllowedBuilds)
			{
				var businessObjectFactory = new BusinessObjectFactory();
				var errorMessage = new StringBuilder();

				foreach (BusinessObject element in selectedSailings)
				{
					var sailing = businessObjectFactory.Load<JobSailing>(element.PK);
					var helper = new BuildConsolHelper();

					if (helper.CheckCanBuildConsolFromSailing(sailing))
					{
						var controller = ZControllerFactory.Create(ControllerIDs.JobConsol);
						var consolForm = controller.ShowNewForm();
						if (consolForm != null)
						{
							var newConsol = ((ZForm)consolForm).BusinessEntity as CommonConsol;
							if (newConsol != null)
							{
								helper.BuildConsolFromSailing(newConsol, sailing);
							}
						}
					}
					else
					{
						errorMessage.AppendLine("- " + Res.GetString("b30cec2e-e50e-47ff-bca3-30b0a339b573", "{0} already has bookings packed for it. Please use Pack Containers.", sailing.HumanReadableName));
					}
				}

				if (errorMessage.Length > 0)
				{
					errorMessage.Insert(0, Res.GetString("c3a679bf-0272-4071-a676-09c7cd166779", "Can not build consols for all selected sailings:") + System.Environment.NewLine);
					Globals.Message.Show(errorMessage.ToString().Trim(), Res.GetString("b0d58533-d59a-4206-9a08-9fcc68cc674c", "Build Consol"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}
			}
			else
			{
				Globals.Message.Show(Res.GetString("41201711-12a8-4cdb-bd21-fb0b4a53a636", "Please select no more than {0} sailings to build consols. Building more at the same time can put unnecessary strain on the system.", maximumAllowedBuilds));
			}
		}

		protected BusinessObject[] GetSelectedSailings()
		{
			return FilteredGrid.SelectedElements;
		}
	}
}
