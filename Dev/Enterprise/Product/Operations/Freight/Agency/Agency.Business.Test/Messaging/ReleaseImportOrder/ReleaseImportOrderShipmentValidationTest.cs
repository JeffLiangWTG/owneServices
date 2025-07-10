using Enterprise.Freight.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal abstract class ReleaseImportOrderShipmentValidationTest : BaseAgencyTest
	{
		public void TestJS_NKLoadPort()
		{
			var shipment = Factory.New<BillOfLading>();
			var validation = new NZReleaseOrderShipmentValidation(shipment);

			validation.ValidateJS_NKLoadPort();
			AssertHasMessageError("JS_NKLoadPort", shipment.JS_NKLoadPortInfo, "No sailing selected.");

			var voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "UAIEV";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUSYD";
			voyage.GenerateSailings();
			shipment.JS_JX = voyage.Sailings[0].PK;

			validation.ValidateJS_NKLoadPort();
			AssertNoMessageError("JS_NKLoadPort", shipment.JS_NKLoadPortInfo, "No sailing selected.");
		}

		public void TestJS_NKDischargePort()
		{
			var shipment = Factory.New<BillOfLading>();

			var validation = new NZReleaseOrderShipmentValidation(shipment);
			validation.ValidateJS_NKDischargePort();
			AssertHasMessageError("JS_NKDischargePort", shipment.JS_NKDischargePortInfo, "No sailing selected.");

			var voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "UAIEV";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUSYD";
			voyage.GenerateSailings();
			shipment.JS_JX = voyage.Sailings[0].PK;

			validation.ValidateJS_NKDischargePort();
			AssertNoMessageError("JS_NKDischargePort", shipment.JS_NKDischargePortInfo, "No sailing selected.");
		}
	}
}
