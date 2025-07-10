using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class AirlineConnectValidationTest : TestCaseWithFactory
	{
		public void TestValidateAirlineConnectValidatePreRequisites_PreAllocationHasValue()
		{
			var consol = Factory.New<ForwardingConsol>();

			AssertEquals(ConsolPacklineHelper.NoPreAllocationShipmentAndContainerErrorMessage, consol.ValidateAirlineConnectPreRequisites());
			consol.JK_TotalShipmentCountCheck = 1;
			AssertEquals(ConsolPacklineHelper.MissingPreRequisitesErrorMessage, consol.ValidateAirlineConnectPreRequisites());
			consol.JK_TotalShipmentActWeightCheck = 1;
			AssertEquals(ConsolPacklineHelper.MissingPreRequisitesErrorMessage, consol.ValidateAirlineConnectPreRequisites());
			consol.JK_TotalShipmentActVolumeCheck = 1;
			AssertEquals(ConsolPacklineHelper.MissingPreRequisitesErrorMessage, consol.ValidateAirlineConnectPreRequisites());
			consol.JK_MaximumAllowablePackageHeight = 1;
			AssertEquals(ConsolPacklineHelper.MissingPreRequisitesErrorMessage, consol.ValidateAirlineConnectPreRequisites());
			consol.JK_MaximumAllowablePackageLength = 1;
			AssertEquals(ConsolPacklineHelper.MissingPreRequisitesErrorMessage, consol.ValidateAirlineConnectPreRequisites());
			consol.JK_MaximumAllowablePackageWidth = 1;
			AssertEquals(ZString.Empty, consol.ValidateAirlineConnectPreRequisites());

			var shipment = consol.Shipments.AddNew();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_Length = 1;
			packLine.JL_Width = 1;
			packLine.JL_Height = 1;
			packLine.JL_ActualWeight = 1;
			packLine.JL_ActualVolume = 1;
			packLine.JL_PackageCount = 1;
			AssertEquals(ZString.Empty, consol.ValidateAirlineConnectPreRequisites());
		}

		public void TestValidateAirMarketPlacePreRequisites_ThereAreLoosePacklinesOrULDPacklinesWhenPreAllocationIsFalse()
		{
			var consol = Factory.New<ForwardingConsol>();

			var shipment1 = consol.Shipments.AddNew();
			shipment1.OuterPackLines.AddNew();
			shipment1.OuterPackLines[0].JL_Length = 1;
			shipment1.OuterPackLines[0].JL_Width = 1;
			shipment1.OuterPackLines[0].JL_Height = 1;
			shipment1.OuterPackLines[0].JL_ActualWeight = 1;
			shipment1.OuterPackLines[0].JL_ActualVolume = 1;
			shipment1.OuterPackLines[0].JL_PackageCount = 1;
			shipment1.JS_ActualWeight = 1;
			var shipment2 = consol.Shipments.AddNew();
			shipment2.OuterPackLines.AddNew();
			shipment2.OuterPackLines[0].JL_Length = 1;
			shipment2.OuterPackLines[0].JL_Width = 1;
			shipment2.OuterPackLines[0].JL_Height = 1;
			shipment2.OuterPackLines[0].JL_ActualWeight = 1;
			shipment2.OuterPackLines[0].JL_ActualVolume = 1;
			shipment2.OuterPackLines[0].JL_PackageCount = 1;
			shipment2.JS_ActualWeight = 1;

			Factory.Save();
			AssertEquals("Shipment prerequisites met", ZString.Empty, consol.ValidateAirlineConnectPreRequisites());

			shipment1.OuterPackLines[0].JL_Height = 0;
			shipment2.OuterPackLines[0].JL_Height = 0;
			Factory.Save();

			AssertNull("Precondition", shipment1.OuterPackLines[0].GetContainer(consol));
			AssertNull("Precondition", shipment2.OuterPackLines[0].GetContainer(consol));
			var expectedPackLinesErrorMessage = ConsolPacklineHelper.PackLinesErrorMessage + "\r\n- " + shipment1.JS_UniqueConsignRef + "\r\n- " + shipment2.JS_UniqueConsignRef;
			AssertEquals("Shipment prerequisites not met", expectedPackLinesErrorMessage, (string)consol.ValidateAirlineConnectPreRequisites());

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "AAD12345AU";
			container1.JC_ContainerCount = 1;
			container1.JC_GrossWeight = 0;
			container1.JC_TotalWidth = 0;
			container1.JC_GrossWeightUQ = "TL";
			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "AKE12354NZ";
			container2.JC_ContainerCount = 1;
			container2.JC_GrossWeight = 0;
			container2.JC_TotalWidth = 0;
			container2.JC_GrossWeightUQ = "TL";
			Factory.Save();
			AssertEquals("Shipment and container prerequisites not met", expectedPackLinesErrorMessage, consol.ValidateAirlineConnectPreRequisites());

			shipment1.OuterPackLines[0].SetContainer(consol, container1);
			shipment2.OuterPackLines[0].SetContainer(consol, container2);
			container1.JC_GrossWeight = 0;
			container2.JC_GrossWeight = 0;
			AssertEquals("Shipment and container prerequisites not met", expectedPackLinesErrorMessage, consol.ValidateAirlineConnectPreRequisites());

			shipment1.OuterPackLines[0].JL_Height = 1;
			shipment2.OuterPackLines[0].JL_Height = 1;
			AssertEquals("Shipment prerequisites met but container prerequisites not met", ConsolPacklineHelper.ContainersErrorMessage, consol.ValidateAirlineConnectPreRequisites());

			container1.JC_GrossWeight = 1;
			container2.JC_GrossWeight = 1;
			container1.JC_TotalWidth = 1;
			container2.JC_TotalWidth = 1;
			container1.JC_TotalLength = 1m;
			container2.JC_TotalLength = 1m;
			container1.JC_TotalHeight = 1m;
			container2.JC_TotalHeight = 1m;
			AssertEquals("All prerequisites met", ZString.Empty, consol.ValidateAirlineConnectPreRequisites());
		}
	}
}
