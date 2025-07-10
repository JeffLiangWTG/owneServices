using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class AddressHelperTest : TestCaseWithFactory
{
	public void TestGetExporterOrSupplierNodeAddress()
	{
		var declaration = Factory.New<JobDeclaration>();
		var exporterAddress = createAddress(declaration.ExporterDocAddress);
		var supplierDocumentaryAddress = createAddress(declaration.SupplierDocumentaryAddress);
		var orgHeader = Factory.New<OrgHeader>();
		var supplierAddress = orgHeader.Addresses.AddNew();
		declaration.JE_OA_SupplierAddress = supplierAddress.PK;
		CombineAssertions(() =>
		{
			AssertEquals("Return exporterAddress", exporterAddress, AddressHelper.GetExporterAddressWithSupplierFallback(declaration));

			declaration.ExporterDocAddress.E2_OA_Address = Guid.Empty;
			AssertEquals("Return supplierDocumentaryAddress", supplierDocumentaryAddress, AddressHelper.GetExporterAddressWithSupplierFallback(declaration));

			declaration.SupplierDocumentaryAddress.E2_OA_Address = Guid.Empty;
			AssertEquals("Return supplierAddress", supplierAddress, AddressHelper.GetExporterAddressWithSupplierFallback(declaration));
		});
	}

	OrgAddress createAddress(JobDocAddress jobDocAddress)
	{
		var exporterHeader = Factory.New<OrgHeader>();
		var orgAddress = exporterHeader.Addresses.AddNew();
		jobDocAddress.OrganisationPK = exporterHeader.PK;
		jobDocAddress.E2_OA_Address = orgAddress.PK;
		return orgAddress;
	}
}
