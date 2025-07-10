using CargoWise.EntityFramework.Testing;
using CargoWise.Macros;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	sealed class ForwardingShipmentLibraryTest : TestCaseWithFactory
	{
		#region GetChargesDisplayOption

		public void TestGetChargesDisplayOption()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HBLAWBChargesDisplay = "SHW";

			const string macro = "GetChargesDisplayOption()";

			var context = new IMacroLibrary[]
			{
				new ForwardingShipmentLibrary(shipment)
			}
			.CreateContext();

			var expr = macro
				.With(context)
				.CreateExpression();

			using (var scope = new MacroScope())
			{
				var result = expr.Evaluate(scope);

				AssertMultilineASCIIEquals("no errors", "", expr.ToFormatString());
				AssertEquals("result", shipment.JS_HBLAWBChargesDisplay, result);
			}
		}

		#endregion
	}
}
