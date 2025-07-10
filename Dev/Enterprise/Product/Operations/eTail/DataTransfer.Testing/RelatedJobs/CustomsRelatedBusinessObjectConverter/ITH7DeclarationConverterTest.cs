using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.DataTransfer.Testing
{
	class ITH7DeclarationConverterTest : BaseH7DeclarationConverterTest<ITH7DeclarationConverter>
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

		protected override ZString LoginCountry => CountryCodes.Italy;

		protected override string GetExpectedMessageTypeCode(ForwardingShipment shipment) => "EH7";

		protected override bool ShouldExportAdditionalReferenceCollection => true;

		protected override BaseHVLVRelatedJobCommand GetRelatedJobCommand(ForwardingShipment shipment) => new ITH7DeclarationCommand(shipment);

		protected override BusinessObject SetupExistingRelatedCustomsJob(ForwardingShipment shipment)
		{
			var result = (Enterprise.Integration.Customs.ITH7.IAsycudaManifestHeader)Factory.NewWithValidTestData(ObjectFactory.GetType<Enterprise.Integration.Customs.ITH7.IAsycudaManifestHeader>());
			result.AMA_JobReference = "TestReference" + shipment.JS_TransportMode;
			return (BusinessObject)result;
		}

		protected override ZString GetExistingRelatedCustomsJobReference(BusinessObject existingJob) => ((Enterprise.Integration.Customs.ITH7.IAsycudaManifestHeader)existingJob).AMA_JobReference;
	}
}
