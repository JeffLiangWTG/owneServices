using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.LocalCartage.GUI
{
	public partial class ContainerDetailsControl : ZUserControl
	{
		public ContainerDetailsControl()
		{
			InitializeComponent();
			AddContainerContextMenuItems();
		}

		public override IBusiness DataSourceForBinding
		{
			get { return (IBusiness)BindingSource.DataSource; }
		}

		void AddContainerContextMenuItems()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				ContainersGrid.ContextMenu.MenuItems.Add(new ZMenuItem("-"));
				ContainersGrid.ContextMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("022F5A0E-4762-4013-AD0B-01BFDC1DEE8F", "Create Load List from selected containers"), new EventHandler(CreateLoadList_Click)));
				ContainersGrid.ContextMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("9520172F-58FE-4A12-B1E0-6C346DD5F56B", "View Load List of selected container"), new EventHandler(ViewLoadList_Click)));
			}
		}

		void CreateLoadList_Click(object sender, EventArgs e)
		{
			if (Cartage.HasChanges || !Cartage.IsInDatabase)
			{
				Globals.Message.Show(Res.GetString("1deb6cf1-96a2-4c59-a05a-2cea971ecde4", "Please save before creating a load list."), Res.GetString("9629145f-6a6a-40b2-91c0-fd5fc3a15c50", "Create Load List"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
			else if (Cartage.HasParent)
			{
				Globals.Message.Show(Res.GetString("d8bb59f3-167f-4c19-bab8-7ca7180870ff", "Cannot create a Load List for a Related Port Transport."), Res.GetString("b341b274-cc5f-4ca2-a2ac-fef5267371b6", "Create Load List"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
			else if (ContainersGrid.SelectedElements.Length == 0)
			{
				Globals.Message.Show(Res.GetString("ec7ebf6f-6e07-41c9-bd8f-81f35d332456", "Please selected some containers."), Res.GetString("33f9858c-1b15-4f42-ba29-78b75a653a7d", "Create Load List"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
			else
			{
				ZStringBuilder containersWithConsol = new ZStringBuilder();
				foreach (CommonBookedCtgMove move in ContainersGrid.SelectedElements)
				{
					var container = move.Container;
					if (container.Consol != null)
					{
						containersWithConsol.Append(Res.GetString("7826fe00-7586-4ff4-8cb5-235a174597e4", "Load List #: {0} / Container #: {1}", container.Consol.JK_UniqueConsignRef, container.JC_ContainerNum));
					}
				}

				if (containersWithConsol.Length > 0)
				{
					Globals.Message.Show(Res.GetString("56e78789-8030-4f6b-ae67-c2143490953b", "The following selected containers are already attached to a Load List. Please deselected them.\r\n{0}", containersWithConsol.ToStringWithNewLineBetweenAppends()), Res.GetString("6dc6c2cc-5385-431c-9eea-09e4e6cbb698", "Create Load List"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}
				else
				{
					CreateLoadList(ContainersGrid.SelectedElements.Cast<CommonBookedCtgMove>().Select(m => m.Container).ToArray());
				}
			}
		}

		void ViewLoadList_Click(object sender, EventArgs e)
		{
			if (Cartage.HasChanges || !Cartage.IsInDatabase)
			{
				Globals.Message.Show(Res.GetString("14fa89f7-1d8a-41a5-be97-6afc9d5b170f", "Please save before viewing a load list."), Res.GetString("fbf8b520-fdef-4b62-a8c4-8238cb6ba254", "View Load List"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
			else if (ContainersGrid.SelectedElements.Length == 0)
			{
				Globals.Message.Show(Res.GetString("be2a0a7b-ee61-4b97-96f5-95ae849c72b4", "Please select a container."), Res.GetString("eeead328-a7e3-4923-9154-2728d7761afa", "View Load List"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
			else if (ContainersGrid.SelectedElements.Length > 1)
			{
				Globals.Message.Show(Res.GetString("1fbec8ef-5b29-4d3e-b6c8-a5bdcf680e68", "Please only select 1 container."), Res.GetString("8b1b27b9-8199-4d5e-82df-6171809c196a", "View Load List"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
			else
			{
				var container = ((CommonBookedCtgMove)ContainersGrid.SelectedElements[0]).Container;
				if (container.Consol == null || !container.Consol.JK_IsCFS)
				{
					Globals.Message.Show(Res.GetString("5470973c-cdc6-4278-b7e9-b18787027088", "Selected container is not attached to a Load List."), Res.GetString("cd800a43-7b24-4f53-9909-d1ff3ba85476", "View Load List"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}
				else
				{
					ViewLoadList(container.Consol);
				}
			}
		}

		void CreateLoadList(CommonContainer[] selectedContainers)
		{
			ZController loadListController = ZControllerFactory.Create(ControllerIDs.LoadListConsol);
			loadListController.SetFormsModalTo(ParentForm);
			loadListController.ShowNewForm();
			CommonConsol loadList_NewFactory = ((ZForm)loadListController.LastShownForm).BusinessEntity as CommonConsol;

			if (loadList_NewFactory != null)
			{
				loadList_NewFactory.PopulateFromCartage(Cartage, selectedContainers);
			}
		}

		void ViewLoadList(CommonConsol loadList)
		{
			ZController loadListController = ZControllerFactory.Create(ControllerIDs.LoadListConsol);
			BusinessObject loadList_newFactory = (BusinessObject)loadListController.Factory.Load<Freight.Integration.CFS.ICFSLoadListConsol>(loadList.PK);
			loadListController.ShowEditForm(loadList_newFactory);
		}

		CommonCartage Cartage
		{
			get { return (CommonCartage)((ZForm)ParentForm).BusinessEntity; }
		}

		public void SelectCartageLeg(CommonCartageLeg leg)
		{
			ContainersGrid.SelectSingleElement(leg.BookedCtgMove);
			ContainerMovesControl.SelectCartageLeg(leg);
		}
	}
}