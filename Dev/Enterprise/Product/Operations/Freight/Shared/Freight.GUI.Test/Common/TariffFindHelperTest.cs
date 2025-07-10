using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.GUI;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.GUI.Testing
{
	sealed class TariffFindHelperTest : TestCaseWithFactory
	{
		public void TestAddDefaultPropertyToTariffControl_TariffColumn_EffectiveDate()
		{
			var control = new TariffColumnStyleInfo();

			TariffFindHelper.AddDefaultPropertyToTariffControlWithEffectiveDate(control, new ZDateTime(2024, 1, 5));

			AssertEquals(Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO, control.GetDataGrouping());
			AssertEquals(Customs.Universal.Constants.TariffTypes.HarmonizedSystem, control.TariffType);
			AssertEquals(new ZDateTime(2024, 1, 5), control.GetEffectiveDate());
		}

		public void TestAddDefaultPropertyToTariffControl_EffectiveDate()
		{
			var shipment = (CommonShipment)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			using (var control = new TariffFindBox())
			{
				TariffFindHelper.AddDefaultPropertyToTariffControlWithEffectiveDate(control, new ZDateTime(2024, 1, 5));

				AssertEquals(Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO, control.GetDataGrouping());
				AssertEquals(Customs.Universal.Constants.TariffTypes.HarmonizedSystem, control.TariffType);
				AssertEquals(new ZDateTime(2024, 1, 5), control.GetEffectiveDate());
			}
		}

		public void TestAddDefaultPropertyToTariffControl_TariffColumn()
		{
			var shipment = (CommonShipment)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();

			var control = new TariffColumnStyleInfo();
			TariffFindHelper.AddDefaultPropertyToTariffControl(control, shipment);

			AssertEquals(Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO, control.GetDataGrouping());
			AssertEquals(Customs.Universal.Constants.TariffTypes.HarmonizedSystem, control.TariffType);
			AssertEquals(ZDateTime.Today, control.GetEffectiveDate());
		}

		public void TestAddDefaultPropertyToTariffControl_TariffFindBox()
		{
			var shipment = (CommonShipment)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			using (var control = new TariffFindBox())
			{
				TariffFindHelper.AddDefaultPropertyToTariffControl(control, shipment);

				AssertEquals(Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO, control.GetDataGrouping());
				AssertEquals(Customs.Universal.Constants.TariffTypes.HarmonizedSystem, control.TariffType);
				AssertEquals(ZDateTime.Today, control.GetEffectiveDate());
			}
		}
	}
}
