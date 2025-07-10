using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.GUI.Testing;

public class NLValueIndicatorsUserControlTest : TestCaseWithFactory
{
	public void TestControls()
	{
		using (var control = new NLValueIndicatorsUserControl())
		{
			CombineAssertions(() =>
			{
				var partyRelationShipCheckBox = control.FindSingleOrDefault<ZCheckBox>("PartyRelationShipCheckBox");
				AssertNotNull("PartyRelationShipCheckBox", partyRelationShipCheckBox);
				AssertEquals("PartyRelationShipCheckBox visible", true, partyRelationShipCheckBox.Visible);
				AssertEquals("PartyRelationShipCheckBox caption", "Party relationship, whether there is price influence", partyRelationShipCheckBox.CaptionResourceString.Caption);

				var restrictionsShipCheckBox = control.FindSingleOrDefault<ZCheckBox>("RestrictionsShipCheckBox");
				AssertNotNull("RestrictionsShipCheckBox", restrictionsShipCheckBox);
				AssertEquals("RestrictionsShipCheckBox visible", true, restrictionsShipCheckBox.Visible);
				AssertEquals("RestrictionsShipCheckBox caption", "Restrictions as to the disposal or use of the goods by the buyer in accordance with Article 70(3)(a) of the Code", restrictionsShipCheckBox.CaptionResourceString.Caption);

				var saleConditionsShipCheckBox = control.FindSingleOrDefault<ZCheckBox>("SaleConditionsShipCheckBox");
				AssertNotNull("SaleConditionsShipCheckBox", saleConditionsShipCheckBox);
				AssertEquals("SaleConditionsShipCheckBox visible", true, saleConditionsShipCheckBox.Visible);
				AssertEquals("SaleConditionsShipCheckBox caption", "Sale or price is subject to some condition or consideration in accordance with Article 70(3)(b) of the Code", saleConditionsShipCheckBox.CaptionResourceString.Caption);

				var disposalAccuralShipCheckBox = control.FindSingleOrDefault<ZCheckBox>("DisposalAccrualShipCheckBox");
				AssertNotNull("DisposalAccrualShipCheckBox", disposalAccuralShipCheckBox);
				AssertEquals("DisposalAccrualShipCheckBox visible", true, disposalAccuralShipCheckBox.Visible);
				AssertEquals("DisposalAccrualShipCheckBox caption", "The sale is subject to an arrangement under which part of the proceeds of any subsequent resale, disposal or use accrues directly or indirectly to the seller", disposalAccuralShipCheckBox.CaptionResourceString.Caption);
			});
		}
	}
}
