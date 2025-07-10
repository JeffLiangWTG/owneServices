using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.GUI
{
	public partial class PackingDetailsUserControl : ZUserControl
	{
		public PackingDetailsUserControl()
		{
			InitializeComponent();
			new UNDGDataItemFormManager(ContainerDetailsGrid, "", UNDGDataItemFormManagerConfig.ShowPSAGroup()).Initialize();
			new HarmonisedCodeFormManager(ContainerDetailsGrid, containersGrid: ContainersModuleButtonGrid.InnerGrid, addColumns: false).Initialize();
		}

		#region Implementation

		public void SetupPlugIn()
		{
			ContainerDetailsTabControl.PlugIns.AddCurrentDependentPlugIn(ControllerIDs.AUContainerMessaging, ContainersModuleButtonGrid.InnerGrid);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		public ResourceStringData ArrivalResourceData
		{
			get { return Enterprise.Freight.GUI.Res.GetData("ZModuleButtonGrid|801387d3-c632-487b-8494-44a62d91b662", "Arrival"); }
		}

		#endregion
	}
}

