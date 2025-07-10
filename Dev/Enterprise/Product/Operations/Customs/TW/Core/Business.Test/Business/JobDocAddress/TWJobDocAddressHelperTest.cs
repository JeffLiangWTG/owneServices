using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class TWJobDocAddressHelperTest : TestCaseWithFactory
	{
		public void TestIsSupplierDocumentaryAddress()
		{
			var jobDocAddress = Factory.New<TWJobDocAddress>();
			jobDocAddress.DocAddressType = DocAddressType.SupplierDocumentaryAddress;
			Assert(jobDocAddress.IsSupplierDocumentaryAddress());
			jobDocAddress.DocAddressType = DocAddressType.SupplierPickupDeliveryAddress;
			Assert(!jobDocAddress.IsSupplierDocumentaryAddress());
			jobDocAddress.DocAddressType = DocAddressType.SupplierTranslatedDocumentaryAddress;
			Assert(!jobDocAddress.IsSupplierDocumentaryAddress());
			jobDocAddress.DocAddressType = DocAddressType.ImporterDocumentaryAddress;
			Assert(!jobDocAddress.IsSupplierDocumentaryAddress());
			jobDocAddress.DocAddressType = DocAddressType.ImporterPickupDeliveryAddress;
			Assert(!jobDocAddress.IsSupplierDocumentaryAddress());
			jobDocAddress.DocAddressType = DocAddressType.ImporterTranslatedDocumentaryAddress;
			Assert(!jobDocAddress.IsSupplierDocumentaryAddress());
			jobDocAddress.DocAddressType = DocAddressType.ImporterTranslatedDocumentaryAddress;
			Assert(!jobDocAddress.IsSupplierDocumentaryAddress());
			jobDocAddress.DocAddressType = DocAddressType.LocalProcessorAddress;
			Assert(!jobDocAddress.IsSupplierDocumentaryAddress());
			jobDocAddress.DocAddressType = DocAddressType.LocalProcessorTranslatedDocAddress;
			Assert(!jobDocAddress.IsSupplierDocumentaryAddress());
			jobDocAddress.DocAddressType = DocAddressType.Manufacturer;
			Assert(!jobDocAddress.IsSupplierDocumentaryAddress());
		}

		public void TestIsSupplierDocumentaryOrPickupDeliveryAddress()
		{
			var jobDocAddress = Factory.New<TWJobDocAddress>();
			jobDocAddress.DocAddressType = DocAddressType.SupplierDocumentaryAddress;
			Assert(jobDocAddress.IsSupplierDocumentaryOrPickupDeliveryAddress());
			jobDocAddress.DocAddressType = DocAddressType.SupplierPickupDeliveryAddress;
			Assert(jobDocAddress.IsSupplierDocumentaryOrPickupDeliveryAddress());
			jobDocAddress.DocAddressType = DocAddressType.SupplierTranslatedDocumentaryAddress;
			Assert(!jobDocAddress.IsSupplierDocumentaryOrPickupDeliveryAddress());
			jobDocAddress.DocAddressType = DocAddressType.ImporterDocumentaryAddress;
			Assert(!jobDocAddress.IsSupplierDocumentaryOrPickupDeliveryAddress());
			jobDocAddress.DocAddressType = DocAddressType.ImporterPickupDeliveryAddress;
			Assert(!jobDocAddress.IsSupplierDocumentaryOrPickupDeliveryAddress());
			jobDocAddress.DocAddressType = DocAddressType.ImporterTranslatedDocumentaryAddress;
			Assert(!jobDocAddress.IsSupplierDocumentaryOrPickupDeliveryAddress());
			jobDocAddress.DocAddressType = DocAddressType.ImporterTranslatedDocumentaryAddress;
			Assert(!jobDocAddress.IsSupplierDocumentaryOrPickupDeliveryAddress());
			jobDocAddress.DocAddressType = DocAddressType.LocalProcessorAddress;
			Assert(!jobDocAddress.IsSupplierDocumentaryOrPickupDeliveryAddress());
			jobDocAddress.DocAddressType = DocAddressType.LocalProcessorTranslatedDocAddress;
			Assert(!jobDocAddress.IsSupplierDocumentaryOrPickupDeliveryAddress());
			jobDocAddress.DocAddressType = DocAddressType.Manufacturer;
			Assert(!jobDocAddress.IsSupplierDocumentaryOrPickupDeliveryAddress());
		}

		public void TestIsImporterDocumentaryAddress()
		{
			var jobDocAddress = Factory.New<TWJobDocAddress>();
			jobDocAddress.DocAddressType = DocAddressType.SupplierDocumentaryAddress;
			Assert(!jobDocAddress.IsImporterDocumentaryAddress());
			jobDocAddress.DocAddressType = DocAddressType.SupplierPickupDeliveryAddress;
			Assert(!jobDocAddress.IsImporterDocumentaryAddress());
			jobDocAddress.DocAddressType = DocAddressType.SupplierTranslatedDocumentaryAddress;
			Assert(!jobDocAddress.IsImporterDocumentaryAddress());
			jobDocAddress.DocAddressType = DocAddressType.ImporterDocumentaryAddress;
			Assert(jobDocAddress.IsImporterDocumentaryAddress());
			jobDocAddress.DocAddressType = DocAddressType.ImporterPickupDeliveryAddress;
			Assert(!jobDocAddress.IsImporterDocumentaryAddress());
			jobDocAddress.DocAddressType = DocAddressType.ImporterTranslatedDocumentaryAddress;
			Assert(!jobDocAddress.IsImporterDocumentaryAddress());
			jobDocAddress.DocAddressType = DocAddressType.LocalProcessorAddress;
			Assert(!jobDocAddress.IsImporterDocumentaryAddress());
			jobDocAddress.DocAddressType = DocAddressType.LocalProcessorTranslatedDocAddress;
			Assert(!jobDocAddress.IsImporterDocumentaryAddress());
			jobDocAddress.DocAddressType = DocAddressType.Manufacturer;
			Assert(!jobDocAddress.IsImporterDocumentaryAddress());
		}

		public void TestIsImporterDocumentaryOrImporterPickupDeliveryAddress()
		{
			var jobDocAddress = Factory.New<TWJobDocAddress>();
			jobDocAddress.DocAddressType = DocAddressType.SupplierDocumentaryAddress;
			Assert(!jobDocAddress.IsImporterDocumentaryOrImporterPickupDeliveryAddress());
			jobDocAddress.DocAddressType = DocAddressType.SupplierPickupDeliveryAddress;
			Assert(!jobDocAddress.IsImporterDocumentaryOrImporterPickupDeliveryAddress());
			jobDocAddress.DocAddressType = DocAddressType.SupplierTranslatedDocumentaryAddress;
			Assert(!jobDocAddress.IsImporterDocumentaryOrImporterPickupDeliveryAddress());
			jobDocAddress.DocAddressType = DocAddressType.ImporterDocumentaryAddress;
			Assert(jobDocAddress.IsImporterDocumentaryOrImporterPickupDeliveryAddress());
			jobDocAddress.DocAddressType = DocAddressType.ImporterPickupDeliveryAddress;
			Assert(jobDocAddress.IsImporterDocumentaryOrImporterPickupDeliveryAddress());
			jobDocAddress.DocAddressType = DocAddressType.ImporterTranslatedDocumentaryAddress;
			Assert(!jobDocAddress.IsImporterDocumentaryOrImporterPickupDeliveryAddress());
			jobDocAddress.DocAddressType = DocAddressType.LocalProcessorAddress;
			Assert(!jobDocAddress.IsImporterDocumentaryOrImporterPickupDeliveryAddress());
			jobDocAddress.DocAddressType = DocAddressType.LocalProcessorTranslatedDocAddress;
			Assert(!jobDocAddress.IsImporterDocumentaryOrImporterPickupDeliveryAddress());
			jobDocAddress.DocAddressType = DocAddressType.Manufacturer;
			Assert(!jobDocAddress.IsImporterDocumentaryOrImporterPickupDeliveryAddress());
		}

		public void TestIsSupplierOrImporterDocumentaryAddress()
		{
			var jobDocAddress = Factory.New<TWJobDocAddress>();
			jobDocAddress.DocAddressType = DocAddressType.SupplierDocumentaryAddress;
			Assert(jobDocAddress.IsSupplierOrImporterDocumentaryAddress());
			jobDocAddress.DocAddressType = DocAddressType.SupplierPickupDeliveryAddress;
			Assert(!jobDocAddress.IsSupplierOrImporterDocumentaryAddress());
			jobDocAddress.DocAddressType = DocAddressType.SupplierTranslatedDocumentaryAddress;
			Assert(!jobDocAddress.IsSupplierOrImporterDocumentaryAddress());
			jobDocAddress.DocAddressType = DocAddressType.ImporterDocumentaryAddress;
			Assert(jobDocAddress.IsSupplierOrImporterDocumentaryAddress());
			jobDocAddress.DocAddressType = DocAddressType.ImporterPickupDeliveryAddress;
			Assert(!jobDocAddress.IsSupplierOrImporterDocumentaryAddress());
			jobDocAddress.DocAddressType = DocAddressType.ImporterTranslatedDocumentaryAddress;
			Assert(!jobDocAddress.IsSupplierOrImporterDocumentaryAddress());
			jobDocAddress.DocAddressType = DocAddressType.LocalProcessorAddress;
			Assert(!jobDocAddress.IsSupplierOrImporterDocumentaryAddress());
			jobDocAddress.DocAddressType = DocAddressType.LocalProcessorTranslatedDocAddress;
			Assert(!jobDocAddress.IsSupplierOrImporterDocumentaryAddress());
			jobDocAddress.DocAddressType = DocAddressType.Manufacturer;
			Assert(!jobDocAddress.IsSupplierOrImporterDocumentaryAddress());
		}

		public void TestIsDocumentaryOrPickupDeliveryAddress()
		{
			var jobDocAddress = Factory.New<TWJobDocAddress>();
			jobDocAddress.DocAddressType = DocAddressType.SupplierDocumentaryAddress;
			Assert(jobDocAddress.IsDocumentaryOrPickupDeliveryAddress());
			jobDocAddress.DocAddressType = DocAddressType.SupplierPickupDeliveryAddress;
			Assert(jobDocAddress.IsDocumentaryOrPickupDeliveryAddress());
			jobDocAddress.DocAddressType = DocAddressType.SupplierTranslatedDocumentaryAddress;
			Assert(!jobDocAddress.IsDocumentaryOrPickupDeliveryAddress());
			jobDocAddress.DocAddressType = DocAddressType.ImporterDocumentaryAddress;
			Assert(jobDocAddress.IsDocumentaryOrPickupDeliveryAddress());
			jobDocAddress.DocAddressType = DocAddressType.ImporterPickupDeliveryAddress;
			Assert(jobDocAddress.IsDocumentaryOrPickupDeliveryAddress());
			jobDocAddress.DocAddressType = DocAddressType.ImporterTranslatedDocumentaryAddress;
			Assert(!jobDocAddress.IsDocumentaryOrPickupDeliveryAddress());
			jobDocAddress.DocAddressType = DocAddressType.LocalProcessorAddress;
			Assert(!jobDocAddress.IsDocumentaryOrPickupDeliveryAddress());
			jobDocAddress.DocAddressType = DocAddressType.LocalProcessorTranslatedDocAddress;
			Assert(!jobDocAddress.IsDocumentaryOrPickupDeliveryAddress());
			jobDocAddress.DocAddressType = DocAddressType.Manufacturer;
			Assert(!jobDocAddress.IsDocumentaryOrPickupDeliveryAddress());
		}

		public void TestIsLocalProcessorAddress()
		{
			var jobDocAddress = Factory.New<TWJobDocAddress>();
			jobDocAddress.DocAddressType = DocAddressType.SupplierDocumentaryAddress;
			Assert(!jobDocAddress.IsLocalProcessorAddress());
			jobDocAddress.DocAddressType = DocAddressType.SupplierPickupDeliveryAddress;
			Assert(!jobDocAddress.IsLocalProcessorAddress());
			jobDocAddress.DocAddressType = DocAddressType.SupplierTranslatedDocumentaryAddress;
			Assert(!jobDocAddress.IsLocalProcessorAddress());
			jobDocAddress.DocAddressType = DocAddressType.ImporterDocumentaryAddress;
			Assert(!jobDocAddress.IsLocalProcessorAddress());
			jobDocAddress.DocAddressType = DocAddressType.ImporterPickupDeliveryAddress;
			Assert(!jobDocAddress.IsLocalProcessorAddress());
			jobDocAddress.DocAddressType = DocAddressType.ImporterTranslatedDocumentaryAddress;
			Assert(!jobDocAddress.IsLocalProcessorAddress());
			jobDocAddress.DocAddressType = DocAddressType.LocalProcessorAddress;
			Assert(jobDocAddress.IsLocalProcessorAddress());
			jobDocAddress.DocAddressType = DocAddressType.LocalProcessorTranslatedDocAddress;
			Assert(!jobDocAddress.IsLocalProcessorAddress());
			jobDocAddress.DocAddressType = DocAddressType.Manufacturer;
			Assert(!jobDocAddress.IsLocalProcessorAddress());
		}

		public void TestIsManufacturerAddress()
		{
			var jobDocAddress = Factory.New<TWJobDocAddress>();
			jobDocAddress.DocAddressType = DocAddressType.SupplierDocumentaryAddress;
			Assert(!jobDocAddress.IsManufacturerAddress());
			jobDocAddress.DocAddressType = DocAddressType.SupplierPickupDeliveryAddress;
			Assert(!jobDocAddress.IsManufacturerAddress());
			jobDocAddress.DocAddressType = DocAddressType.SupplierTranslatedDocumentaryAddress;
			Assert(!jobDocAddress.IsManufacturerAddress());
			jobDocAddress.DocAddressType = DocAddressType.ImporterDocumentaryAddress;
			Assert(!jobDocAddress.IsManufacturerAddress());
			jobDocAddress.DocAddressType = DocAddressType.ImporterPickupDeliveryAddress;
			Assert(!jobDocAddress.IsManufacturerAddress());
			jobDocAddress.DocAddressType = DocAddressType.ImporterTranslatedDocumentaryAddress;
			Assert(!jobDocAddress.IsManufacturerAddress());
			jobDocAddress.DocAddressType = DocAddressType.LocalProcessorAddress;
			Assert(!jobDocAddress.IsManufacturerAddress());
			jobDocAddress.DocAddressType = DocAddressType.LocalProcessorTranslatedDocAddress;
			Assert(!jobDocAddress.IsManufacturerAddress());
			jobDocAddress.DocAddressType = DocAddressType.Manufacturer;
			Assert(jobDocAddress.IsManufacturerAddress());
		}

		public void TestIsLocalAddress()
		{
			var jobDocAddress = Factory.New<TWJobDocAddress>();
			jobDocAddress.DocAddressType = DocAddressType.SupplierDocumentaryAddress;
			Assert(!jobDocAddress.IsLocalAddress());
			jobDocAddress.DocAddressType = DocAddressType.SupplierPickupDeliveryAddress;
			Assert(!jobDocAddress.IsLocalAddress());
			jobDocAddress.DocAddressType = DocAddressType.SupplierTranslatedDocumentaryAddress;
			Assert(jobDocAddress.IsLocalAddress());
			jobDocAddress.DocAddressType = DocAddressType.ImporterDocumentaryAddress;
			Assert(!jobDocAddress.IsLocalAddress());
			jobDocAddress.DocAddressType = DocAddressType.ImporterPickupDeliveryAddress;
			Assert(!jobDocAddress.IsLocalAddress());
			jobDocAddress.DocAddressType = DocAddressType.ImporterTranslatedDocumentaryAddress;
			Assert(jobDocAddress.IsLocalAddress());
			jobDocAddress.DocAddressType = DocAddressType.LocalProcessorAddress;
			Assert(!jobDocAddress.IsLocalAddress());
			jobDocAddress.DocAddressType = DocAddressType.LocalProcessorTranslatedDocAddress;
			Assert(jobDocAddress.IsLocalAddress());
			jobDocAddress.DocAddressType = DocAddressType.Manufacturer;
			Assert(!jobDocAddress.IsLocalAddress());
			jobDocAddress.DocAddressType = DocAddressType.ManufacturerTranslatedDocumentaryAddress;
			Assert(jobDocAddress.IsLocalAddress());
		}

		public void TestIsControllingMessageDocAddress()
		{
			var jobDocAddress = Factory.NewWithValidTestData<TWJobDocAddress>();
			var controllingMessage = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			Assert(!jobDocAddress.IsControllingMessageDocAddress());

			controllingMessage.DocAddresses.Add(jobDocAddress);
			Assert(jobDocAddress.IsControllingMessageDocAddress());
		}

		public void TestIsAPPlicantAddress()
		{
			var jobDocAddress = Factory.New<TWJobDocAddress>();
			jobDocAddress.DocAddressType = DocAddressType.SupplierDocumentaryAddress;
			Assert(!jobDocAddress.IsApplicantAddress());
			jobDocAddress.DocAddressType = DocAddressType.SupplierPickupDeliveryAddress;
			Assert(!jobDocAddress.IsApplicantAddress());
			jobDocAddress.DocAddressType = DocAddressType.SupplierTranslatedDocumentaryAddress;
			Assert(!jobDocAddress.IsApplicantAddress());
			jobDocAddress.DocAddressType = DocAddressType.ImporterDocumentaryAddress;
			Assert(!jobDocAddress.IsApplicantAddress());
			jobDocAddress.DocAddressType = DocAddressType.ImporterPickupDeliveryAddress;
			Assert(!jobDocAddress.IsApplicantAddress());
			jobDocAddress.DocAddressType = DocAddressType.ImporterTranslatedDocumentaryAddress;
			Assert(!jobDocAddress.IsApplicantAddress());
			jobDocAddress.DocAddressType = DocAddressType.LocalProcessorAddress;
			Assert(!jobDocAddress.IsApplicantAddress());
			jobDocAddress.DocAddressType = DocAddressType.LocalProcessorTranslatedDocAddress;
			Assert(!jobDocAddress.IsApplicantAddress());
			jobDocAddress.DocAddressType = DocAddressType.Manufacturer;
			Assert(!jobDocAddress.IsApplicantAddress());
			jobDocAddress.DocAddressType = DocAddressType.Applicant;
			Assert(jobDocAddress.IsApplicantAddress());
		}

		public void TestIsApplicantTranslatedDocumentaryAddress()
		{
			var jobDocAddress = Factory.New<TWJobDocAddress>();
			jobDocAddress.DocAddressType = DocAddressType.SupplierDocumentaryAddress;
			Assert(!jobDocAddress.IsApplicantTranslatedDocumentaryAddress());
			jobDocAddress.DocAddressType = DocAddressType.ApplicantTranslatedDocumentaryAddress;
			Assert(jobDocAddress.IsApplicantTranslatedDocumentaryAddress());
		}
	}
}
