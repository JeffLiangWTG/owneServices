//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoItemPackagingAddInfoValidation
//
//    This class should be used for overriding validation in AutoItemPackagingAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	using CargoWise.EntityFramework.Testing;

	internal class ItemPackagingAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestNumberOfPackages()
		{
			AssertNoMessageErrors("Legacy Export entry should not validate Packaging count", packaging.NZ_NumberOfPackagesInfo);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertNoMessageErrors("Legacy Import entry should not validate Packing count either", packaging.NZ_NumberOfPackagesInfo);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			packaging.NZ_NumberOfPackages = 0;
			AssertHasWarnings("Number of Packages", packaging.NZ_NumberOfPackagesInfo);

			packaging.NZ_NumberOfPackages = 5;
			AssertNoWarnings("Number of Packages", packaging.NZ_NumberOfPackagesInfo);
		}

		public void TestPackageVolume()
		{
			AssertNoMessageErrors("Legacy Export entry should not validate Packaging volume", packaging.NZ_PackageVolumeInfo);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertNoMessageErrors("Legacy Import entry should not validate Packing volume either", packaging.NZ_PackageVolumeInfo);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			packaging.NZ_PackageVolume = 0m;
			AssertNoMessageErrors("Packages Volume - Customs accepts zero - do not validate this field for entry.", packaging.NZ_PackageVolumeInfo);

			packaging.NZ_PackageVolume = 1.5m;
			AssertNoMessageErrors("Packages Volume", packaging.NZ_PackageVolumeInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0203.11.00.02C";
			packaging = invoiceLine.ItemPackages.AddNew();
		}

		JobDeclaration declaration;
		ItemPackaging packaging;
	}
}
