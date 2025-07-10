using System;
using System.Reflection;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Integration;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.DataTransfer.Testing
{
	class EUICS2ManifestConverterTest : BaseAsycudaManifestConverterTest<EUICS2ManifestConverter>
	{
		public void TestExportedUniversalShipment_HaveApplicationCodeAndManifestType()
		{
			var universalShipment = SetupTestShipment(TransportModes.Air);
			var exportedDataObject = GetExportedDataObject(universalShipment);

			CombineAssertions("Application code and manifest type should be populated in exported data object", () =>
			{
				AssertEquals("Application code", "NVC", exportedDataObject.MessagingApplicationCode.Code);
				AssertEquals("Manifest type", "ENS", exportedDataObject.MessageType.Code);
			});
		}

		public new void TestNonWesternEuropeanCharactersRemovalService()
		{
			var shouldStripNonWesternEuropeanCharactersProperty = typeof(EUICS2ManifestConverter).GetProperty("ShouldStripNonWesternEuropeanCharacters", BindingFlags.Instance | BindingFlags.NonPublic);
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var converter = new EUICS2ManifestConverter(new EUICS2ManifestCommand(shipment));

			using (HVLVDataRegistry.Instance.RemoveNonWesternEuropeanCharactersEUICS2Manifest.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("ShouldStripNonWesternEuropeanCharacters should be true when registry is set", true, shouldStripNonWesternEuropeanCharactersProperty.GetValue(converter, null));
			}

			using (HVLVDataRegistry.Instance.RemoveNonWesternEuropeanCharactersEUICS2Manifest.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("ShouldStripNonWesternEuropeanCharacters should be false when registry is not set", false, shouldStripNonWesternEuropeanCharactersProperty.GetValue(converter, null));
			}
		}

		protected override ZString LoginCountry => CountryCodes.Germany;

		protected override string[] SupportedTransportModes => new[] { TransportModes.Air, TransportModes.Sea, TransportModes.Road };

		protected override RecipientRoleType GetExpectedPickupOrDeliveryRole(Directions shipmentDirection) => RecipientRoleType.DCA;

		protected override bool ShouldEntryHeaderContainDestinationCountryInsteadOfCurrentLoginCountry => true;

		protected override string GetExpectedMessageTypeCode(ForwardingShipment shipment) => ObjectFactory.Get<Enterprise.Integration.Customs.EUICS2.IEUICS2ManifestTypes>().ENSCodeDescription.Code;

		protected override BaseHVLVRelatedJobCommand GetRelatedJobCommand(ForwardingShipment shipment) => new EUICS2ManifestCommand(shipment);

		protected override BusinessObject SetupExistingRelatedCustomsJob(ForwardingShipment shipment)
		{
			var result = (Enterprise.Integration.Customs.ASYCUDA.EUICS2.IAsycudaManifestHeader)Factory.NewWithValidTestData(ObjectFactory.GetType<Enterprise.Integration.Customs.ASYCUDA.EUICS2.IAsycudaManifestHeader>());
			result.AMA_JobReference = "TestReference" + shipment.JS_TransportMode;
			result.AMA_ManifestType = "ENS";
			return (BusinessObject)result;
		}

		protected override ZString GetExistingRelatedCustomsJobReference(BusinessObject existingJob) => ((Enterprise.Integration.Customs.ASYCUDA.EUICS2.IAsycudaManifestHeader)existingJob).AMA_JobReference;
	}
}
