using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	public class CustomsTemplateCopyableProviderTest : TestCaseWithFactory
	{
		public void TestCloneCountrySpecificData()
		{
			#region SetUp Data
			var provider = new CustomsTemplateCopyableProvider();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipmentInBondHeader = Factory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
			shipmentInBondHeader.BH_ParentID = shipment.PK;
			shipmentInBondHeader.BH_ParentTableCode = shipment.TablePrefix;
			shipmentInBondHeader.BH_CarrierSCAC = "AAA";
			var clonedShipment = Factory.New<ForwardingShipment>();
			var clonedShipment2 = Factory.New<ForwardingShipment>();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var declarationInBondHeader = Factory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
			declarationInBondHeader.BH_ParentID = declaration.PK;
			declarationInBondHeader.BH_ParentTableCode = declaration.TablePrefix;
			declarationInBondHeader.BH_CarrierSCAC = "XXX";
			#endregion

			provider.CloneCountrySpecificData(shipment, clonedShipment);
			var clonedInBondHeader = (CusInBondHeader)clonedShipment.InBondHeader;
			AssertEquals(shipmentInBondHeader.BH_CarrierSCAC, clonedInBondHeader.BH_CarrierSCAC);

			declaration.JE_JS = shipment.PK;
			provider.CloneCountrySpecificData(shipment, clonedShipment2);
			var clonedInBondHeader2 = (CusInBondHeader)clonedShipment2.InBondHeader;
			AssertEquals(shipmentInBondHeader.BH_CarrierSCAC, clonedInBondHeader2.BH_CarrierSCAC);
		}
	}
}
