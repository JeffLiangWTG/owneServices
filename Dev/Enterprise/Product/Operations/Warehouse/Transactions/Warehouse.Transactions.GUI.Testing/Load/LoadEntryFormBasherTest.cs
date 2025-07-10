using System.Windows.Forms;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	[TestedType(typeof(LoadEntryForm))]
	class LoadEntryFormBasherTest : ZFormBasherTest
	{
		#region Implementation

		protected override bool AllowSaveOnFormForTestHasChanges => false;

		protected override Form GetFormToBashCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;

			var transportCompany = Helper.CreateClient("TC1");
			var carrierServicelevel = transportCompany.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";

			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);

			var load = Helper.CreateWhsLoad(transportCompany, dockDoorLocation, "WL00000001", carrierServicelevel.PL_Code, truck);
			Factory.Save();

			var form = new LoadEntryForm(load);
			return form;
		}

		WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		#endregion
	}
}
