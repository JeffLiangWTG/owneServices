using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.GUI.Testing
{
	class CartageZoneDistanceCalculatorControlTest : BaseCombinedCalculatorControlTest<CartageZoneDistanceControl>
	{
		public override string CalculatorCode => CartageZoneDistanceCalculator.Code;

		public void TestBindingToCartageZones()
		{
			var helper = new TestHelper(Factory);
			helper.CreateRateTransportZoneSet(null, CountryCodes.Australia, zoneNames: new ZString[] { "ZoneA", "ZoneB" });

			var ratingHeader = helper.NewClientRate(helper.NewOrgHeader());
			var entry = ratingHeader.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LCL, "AUSYD", "");
			var rateLine = entry.AddRateLine("OCART", CartageZoneDistanceCalculator.Code, QuantityUnit.KG);
			Factory.Save();

			var calculatorZones = rateLine.Calculator.CartageZones.Cast<CartageZone>().Select(x => x.Description).ToArray();
			AssertEquals("Pre-condition: cartage zones exist", 3, calculatorZones.Length);
			AssertContainsExactElementsInAnyOrder(new[] { "ZoneA", "ZoneB", "Standard" }, calculatorZones);

			using (var form = new Form())
			using (var control = new CartageZoneDistanceControl())
			{
				control.SetDataBinding(ratingHeader, "");
				control.ViewCalculatorForBinding = rateLine.ViewCalculatorForBinding;
				control.BindTo = "ORGRateEntriesForBinding.RateLines.ViewCalculator";

				form.Controls.Add(control);
				form.Show();

				var cartageZoneList = control.CartagePanelForTest.CartageZonesGrid.List;

				AssertNotNull(cartageZoneList);
				Assert(cartageZoneList is CartageZoneCollection);
				AssertEquals(3, cartageZoneList.Count);
			}
		}
	}
}
