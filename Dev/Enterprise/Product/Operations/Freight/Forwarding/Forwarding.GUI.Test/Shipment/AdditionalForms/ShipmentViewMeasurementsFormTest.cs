using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(ShipmentViewMeasurementsForm))]
	public class ShipmentViewMeasurementsFormTest : ZFormBasherTest
	{
		protected override bool ShouldTestFormIsFullyTranslatable => false;

		protected override Form GetFormToBashCore()
		{
			return new ShipmentViewMeasurementsForm();
		}
	}
}
