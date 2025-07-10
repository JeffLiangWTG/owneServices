using System.Linq;
using System.Windows.Forms;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.GUI.Testing
{
	[TestedType(typeof(ShowEDocsForm))]
	class ShowEDocsFormTest : ZFormBasherTest
	{
		#region TestFormCaption

		public void TestFormCaption()
		{
			var packageState = CreatePackageStateForTesting();
			using (var form = new ShowEDocsForm(packageState))
			{
				AssertEquals("View eDocs - PKG2", form.FormCaption);
			}
		}

		#endregion

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var packageState = CreatePackageStateForTesting();
			return new ShowEDocsForm(packageState);
		}

		WhsItemPackageState CreatePackageStateForTesting()
		{
			var parent = Factory.New<DummyBizOWithPackLines>();
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var packageState = Helper.CreatePackageState(rcn, 1, "PLT", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);
			Factory.Save();

			return packageState;
		}

		WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory));
		WhsTransitTestHelper helper;

		#endregion
	}
}
