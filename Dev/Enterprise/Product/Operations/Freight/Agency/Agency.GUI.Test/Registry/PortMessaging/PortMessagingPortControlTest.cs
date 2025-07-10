using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Freight.Agency.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	[TestedType(typeof(PortMessagingPortControl))]
	internal class PortMessagingPortControlTest : RegistryZUserControlTestCase
	{
		public void TestPortGridRemoveAction()
		{
			using (var portMessagingPortControl = new PortMessagingPortControl())
			{
				var portGrid = portMessagingPortControl.Controls.Find("portGrid", true).First() as ZGrid;
				AssertEquals(RemoveAction.RemoveAndDelete, portGrid.RemoveAction);
			}
		}

		#region Implementation

		protected override IBusiness GetNewBusinessEntity()
		{
			var collection = new PortMessagingPortCollection();
			var port1 = collection.AddNew();
			port1.Port = "AUSYD";
			port1.SenderID = "DFGH";
			port1.Enabled = true;
			var port2 = collection.AddNew();
			port2.Port = "CNSHA";
			port2.SenderID = "KIUY";
			port2.Enabled = true;
			return collection;
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((PortMessagingPortCollection)businessEntity).ReadOnly;
		}

		#endregion
	}
}
