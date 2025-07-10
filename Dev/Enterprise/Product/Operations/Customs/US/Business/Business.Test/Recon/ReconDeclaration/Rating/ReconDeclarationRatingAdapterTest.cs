using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ReconDeclaration))]
	sealed class ReconDeclarationRatingAdapterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestJobDatesProvider()
		{
			var reconDeclaration = new ReconDeclaration(Declaration);
			AssertType<JobDatesProvider<ReconDeclaration>>(reconDeclaration.GetFirstAdapter().JobDatesProvider);
		}

		public void TestImportBroker()
		{
			var reconDeclaration = new ReconDeclaration(Declaration);
			AssertNull(reconDeclaration.GetFirstAdapter().ImportBroker);
		}

		public void TestExportBroker()
		{
			var dec = Factory.New<JobDeclaration>();
			var reconDec = new ReconDeclaration(dec);
			AssertNull(reconDec.GetFirstAdapter().ExportBroker);
		}

		public void TestIAutoRatingCustomsInfoMessageSubType()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			var customsInfo = reconDeclaration.GetFirstAdapter() as IAutoRatingCustomsInfo;
			AssertEquals("09", customsInfo.MessageSubType);
			AssertEquals(Core.Constants.CountryCodes.UnitedStates, reconDeclaration.GetFirstAdapter().Destination.Code);
		}

		public void TestMonetaryValues()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var reconDeclaration = new ReconDeclaration(jobDeclaration);
			AssertNotNull(reconDeclaration.GetFirstAdapter().MonetaryValues);
		}

		public void TestInvoices()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var reconDeclaration = new ReconDeclaration(jobDeclaration);
			AssertNotNull(((IAutoRatingCustomsInfo)reconDeclaration.GetFirstAdapter()).Invoices);
		}

		public void TestAdapterTypeAndID()
		{
			var reconDeclaration = new ReconDeclaration(Declaration);
			AssertEquals(AdapterType.ReconDeclaration, reconDeclaration.GetFirstAdapter().AdapterType);
			AssertEquals(reconDeclaration.JE_DeclarationReference, reconDeclaration.GetFirstAdapter().OperationalJobCode);
		}

		public void TestDeliveryAddressShouldReturnNullIfImporterIsNull()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var reconDeclaration = new ReconDeclaration(jobDeclaration);
			var reconDeclarationRatingAdapter = new ReconDeclarationRatingAdapter(reconDeclaration);
			AssertNull("Precondition: Importer is null.", reconDeclaration.Importer);
			AssertNull("DeliveryAddress is null.", reconDeclarationRatingAdapter.DeliveryAddress);
		}

		protected override BusinessObject GetNewBusinessObject() => new ReconDeclaration(Declaration);

		JobDeclaration declaration;
		JobDeclaration Declaration => declaration ?? (declaration = Factory.New<JobDeclaration>());
	}
}
