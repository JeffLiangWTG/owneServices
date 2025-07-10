using System.Windows.Forms;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.GUI.Testing
{
	[TestedType(typeof(LocationForm))]
	class LocationFormBasherTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore() => new LocationForm(Factory.New<WhsRow>());

		#endregion
	}
}
