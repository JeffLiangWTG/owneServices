using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class PackingSynchroniserBASEONLYTest : TestCaseWithFactory
	{
		public void TestSynchroniseCoreBaseBehaviourIncPackageMarks()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var shipment = Factory.New<ForwardingShipment>();
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_JS = shipment.PK;
				shipment.JS_HouseBill = "HBL1";
				declaration.JE_JS = shipment.PK;
				declaration.ShipmentSynchroniser.SetEnabled(true, false);
				declaration.ShipmentSynchroniser.Synchronise(true);
				PackLine packLine = shipment.OuterPackLines.AddNew();
				packLine.JL_PackageCount = 17;
				packLine.JL_MarksAndNumbers = "RED BLUE GREEN";

				var bill = declaration.PrimaryMasterBill;
				AssertEquals(17, declaration.Packages[0].CW_PackQty);
				AssertEquals("RED BLUE GREEN", declaration.Packages[0].CW_MarksAndNos);

				packLine.JL_MarksAndNumbers = "";
				shipment.JS_MarksAndNumbers = "PINK PURPLE YELLOW";
				AssertEquals("PINK PURPLE YELLOW", declaration.Packages[0].CW_MarksAndNos);

				shipment.JS_MarksAndNumbers = "BLACK WHITE";
				AssertEquals("BLACK WHITE", declaration.Packages[0].CW_MarksAndNos);

				declaration.ShipmentSynchroniser.SetEnabled(true, true);
				shipment.JS_MarksAndNumbers = "BLUE RED";
				AssertEquals("BLACK WHITE", declaration.Packages[0].CW_MarksAndNos);

				declaration.ShipmentSynchroniser.SetEnabled(true, false);
				shipment.JS_MarksAndNumbers = "YELLOW BROWN";
				AssertEquals("YELLOW BROWN", declaration.Packages[0].CW_MarksAndNos);
			}
		}
	}
}
