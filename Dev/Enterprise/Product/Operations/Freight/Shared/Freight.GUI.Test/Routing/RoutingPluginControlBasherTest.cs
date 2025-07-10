using System.Windows.Forms;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.GUI.Testing
{
	[TestedType(typeof(RoutingPluginControlTestForm))]
	sealed class RoutingPluginControlBasherTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.Consols.AddNew().JK_UniqueConsignRef = "Consol 1";
			shipment.Consols.AddNew().JK_UniqueConsignRef = "Consol 2";

			IRoutingSupport support = shipment;
			return new RoutingPluginControlTestForm(support.TransportsIncludingRelated);
		}

		#endregion
	}
}
