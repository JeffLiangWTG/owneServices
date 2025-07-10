using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.DataTransfer.Testing
{
	class AUAirCargoReportConverterTest : CustomsRelatedBusinessObjectConverterBaseTest<AirManifestConverter>
	{
		public void TestConvertShipmentToCargoReport_EndToEnd_Australia()
		{
			AssertConvertShipmentToCargoReport_EndToEnd(TransportModes.Air, CusHAWBSchema.Constants.Prefix);
		}

		public void TestConvertShipmentToCargoReport_Cancel_Australia()
		{
			AssertConvertShipmentToCargoReport_Cancel(TransportModes.Air, CusMAWBSchema.Constants.Prefix, CusHAWBSchema.Constants.Prefix);
		}

		protected override BaseHVLVRelatedJobCommand GetRelatedJobCommand(ForwardingShipment shipment) => new AUAirCargoReportCommand(shipment);

		protected override ZString LoginCountry => CountryCodes.Australia;

		protected override string[] SupportedTransportModes => new[] { TransportModes.Air };

		protected override BusinessObject SetupExistingRelatedCustomsJob(ForwardingShipment shipment)
		{
			var result = Factory.NewWithValidTestData<CusMAWB>();
			result.CM_MessageReference = "TestReference";
			return result;
		}

		protected override ZString GetExistingRelatedCustomsJobReference(BusinessObject existingJob) => ((CusMAWB)existingJob).CM_MessageReference;
	}
}
