using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.Freight.PortHubs.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Freight.PortHubs.GUI
{
	public partial class PortHubFilterStripControl : ZUserControl
	{
		public PortHubFilterStripControl()
		{
			InitializeComponent();
		}

		PortHubStripControl portHubStripControl;

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			DataBindings.Clear();
			base.SetDataBinding(dataSource, dataMember);
			var portHubSelection = dataSource as PortHubSelectionCollectionWrapper;

			if (portHubSelection != null)
			{
				InitializeFilterControl(portHubSelection);
			}
		}

		void InitializeFilterControl(PortHubSelectionCollectionWrapper portHubSelection)
		{
			portHubStripControl = new PortHubStripControl(portHubSelection);
			portHubStripControl.Name = "portHubStripControl";
			portHubStripControl.Dock = DockStyle.Fill;
			portHubStripControl.AutoSize = true;
			filterStripPanel.Controls.Add(portHubStripControl);
		}

		public void ClearAllFilterStripValuesAndReload()
		{
			if (portHubStripControl != null)
			{
				var filterBusinessObject = portHubStripControl.FilterBusinessObject;
				filterBusinessObject.FilterStrips.ClearValues();

				var collection = ((PortHubSelectionCollectionWrapper)CurrentDataItem).Collection;
				collection.Load();
			}
		}

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<PortHubFilterStripControl>().Result;
		}
	}
}
