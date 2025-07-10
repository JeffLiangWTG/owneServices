using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.GUI.Testing
{
	[TestedType(typeof(LocationTypeEntryForm))]
	class LocationTypeEntryFormBasherTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var locationType = Helper.CreateLocationType("TST");
			Factory.Save();
			return new LocationTypeEntryForm(new BusinessObjectFactory().Load<WhsLocationType>(locationType.PK)) { ControllerID = ControllerIDs.WhsConfigLocationType };
		}

		WhsTestHelperFunctionsEnv Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctionsEnv(Factory)); }
		}

		WhsTestHelperFunctionsEnv helper;

		#endregion
	}
}
