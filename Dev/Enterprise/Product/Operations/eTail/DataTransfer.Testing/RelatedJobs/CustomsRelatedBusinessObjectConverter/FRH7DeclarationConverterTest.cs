using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.DataTransfer.Testing
{
	class FRH7DeclarationConverterTest : BaseH7DeclarationConverterTest<FRH7DeclarationConverter>
	{
		public void TestExportedUniversalShipment_HaveApplicationCodeAndManifestType()
		{
			var universalShipment = SetupTestShipment(TransportModes.Air);
			var exportedDataObject = GetExportedDataObject(universalShipment);

			CombineAssertions("Application code and manifest type should be populated in exported data object", () =>
			{
				AssertEquals("Application code", "LVC", exportedDataObject.MessagingApplicationCode.Code);
				AssertEquals("Manifest type", "EH7", exportedDataObject.MessageType.Code);
			});
		}

		protected override ZString LoginCountry => CountryCodes.France;

		protected override string GetExpectedMessageTypeCode(ForwardingShipment shipment) => ObjectFactory.Get<Enterprise.Integration.Customs.EUH7.IEUH7ManifestTypes>().EH7CodeDescription.Code;

		protected override bool ShouldExportAdditionalReferenceCollection => true;

		protected override BaseHVLVRelatedJobCommand GetRelatedJobCommand(ForwardingShipment shipment) => new FRH7DeclarationCommand(shipment);

		protected override BusinessObject SetupExistingRelatedCustomsJob(ForwardingShipment shipment)
		{
			var result = (Enterprise.Integration.Customs.ASYCUDA.EUH7.IAsycudaManifestHeader)Factory.NewWithValidTestData(ObjectFactory.GetType<Enterprise.Integration.Customs.ASYCUDA.EUH7.IAsycudaManifestHeader>());
			result.AMA_JobReference = "TestReference" + shipment.JS_TransportMode;
			return (BusinessObject)result;
		}

		protected override ZString GetExistingRelatedCustomsJobReference(BusinessObject existingJob) => ((Enterprise.Integration.Customs.ASYCUDA.EUH7.IAsycudaManifestHeader)existingJob).AMA_JobReference;
	}
}
