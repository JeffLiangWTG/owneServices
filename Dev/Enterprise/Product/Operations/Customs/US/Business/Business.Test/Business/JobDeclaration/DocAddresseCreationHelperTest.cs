using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.US.Business.Testing
{
	class DocAddresseCreationHelperTest : TestCaseWithFactory
	{
		public void TestAllSupportedDocTypeIsDefinedAccessibility()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_JS = Factory.New<ForwardingShipment>().PK;
			AssertNoExceptionThrown(() =>
			{
				foreach (var docType in ((IDocAddresses)dec).SupportedAddressTypes)
				{
					DocAddressesCreationHelper.ShouldBeCreated(dec, docType);
				}
			});
		}

		public void TestAllSupportedDocTypeInStandAloneDecIsDefinedAccessibility()
		{
			var dec = Factory.New<JobDeclaration>();
			AssertNoExceptionThrown(() =>
			{
				foreach (var docType in ((IDocAddresses)dec).SupportedAddressTypes)
				{
					DocAddressesCreationHelper.ShouldBeCreated(dec, docType);
				}
			});
		}

		public void TestIsAccessibleOnGUIForStandAloneJob()
		{
			var dec = Factory.New<JobDeclaration>();
			foreach (var addressType in ((IDocAddresses)dec).SupportedAddressTypes)
			{
				Assert(DocAddressesCreationHelper.ShouldBeCreated(dec, addressType));
			}
			Assert(!DocAddressesCreationHelper.ShouldBeCreated(dec, DocAddressType.None));
		}

		public void TestIsAccessibleOnGUIForNonStandAloneJob()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_JS = Factory.New<ForwardingShipment>().PK;
			Assert(DocAddressesCreationHelper.ShouldBeCreated(dec, DocAddressType.SupplierDocumentaryAddress));
			Assert(!DocAddressesCreationHelper.ShouldBeCreated(dec, DocAddressType.NotifyParty));
		}
	}
}
