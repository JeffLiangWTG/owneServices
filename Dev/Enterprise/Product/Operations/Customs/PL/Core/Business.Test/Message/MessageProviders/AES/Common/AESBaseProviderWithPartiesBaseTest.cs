using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using CargoWise.Types;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business.Testing;

abstract class AESBaseProviderWithPartiesBaseTest<TDataProvider> : AESBaseProviderTest<TDataProvider>
	where TDataProvider : class, IAESWithPartiesBase
{
	public void TestExporter()
	{
		CombineAssertions(() =>
		{
			AssertNull("No valid exporter", GetProvider().Exporter);

			var supplierHeader = Factory.New<OrgHeader>();
			var supplierAddress = supplierHeader.Addresses.AddNew();
			supplierAddress.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "supplier");
			declaration.JE_OA_SupplierAddress = supplierAddress.PK;
			AssertEquals("Mapped from supplier", "PLsupplier", GetProvider().Exporter.IdentificationNumber);

			var suppDocumentaryJobDocAddress = declaration.SupplierDocumentaryAddress;
			var suppDocumentaryHeader = Factory.New<OrgHeader>();
			var suppDocumentaryAddress = suppDocumentaryHeader.Addresses.AddNew();
			suppDocumentaryAddress.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "suppdocumentary");
			suppDocumentaryJobDocAddress.E2_OA_Address = suppDocumentaryAddress.PK;
			AssertEquals("Mapped from supplier documentary", "PLsuppdocumentary", GetProvider().Exporter.IdentificationNumber);

			var exporterJobDocAddress = declaration.ExporterDocAddress;
			var exporterHeader = Factory.New<OrgHeader>();
			var exporterAddress = exporterHeader.Addresses.AddNew();
			exporterAddress.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "exporter");
			exporterJobDocAddress.E2_OA_Address = exporterAddress.PK;
			AssertEquals("Mapped from exporter", "PLexporter", GetProvider().Exporter.IdentificationNumber);
		});
	}

	public virtual void TestExporterType()
	{
		var supplierHeader = Factory.New<OrgHeader>();
		var supplierAddress = supplierHeader.Addresses.AddNew();
		declaration.JE_OA_SupplierAddress = supplierAddress.PK;

		AssertType<AESExporterProvider>(GetProvider().Exporter);
	}

	public void TestDeclarant()
	{
		CombineAssertions(() =>
		{
			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			AssertNull("No valid declarant", GetProvider().Declarant);

			var declarantHeader = Factory.New<OrgHeader>();
			var declarantAddress = declarantHeader.Addresses.AddNew();
			declaration.JE_OA_DeclarantAddress = declarantAddress.PK;
			AssertNotNull("Valid declarant address exists", GetProvider().Declarant);
		});
	}

	public virtual void TestDeclarantType()
	{
		var declarantHeader = Factory.New<OrgHeader>();
		var declarantAddress = declarantHeader.Addresses.AddNew();
		declaration.JE_OA_DeclarantAddress = declarantAddress.PK;

		AssertType<AESDeclarantWithIdentificationNumbersProvider>(GetProvider().Declarant);
	}

	public void TestRepresentative()
	{
		CombineAssertions(() =>
		{
			declaration.JE_DeclarantType = ZString.Empty;
			AssertNull("Declarant Type is empty", GetProvider().Representative);

			declaration.JE_DeclarantType = PLRepresentationTypeList.Codes._3Consignee;
			AssertNull("Declarant Type is not '4'", GetProvider().Representative);

			declaration.JE_DeclarantType = PLRepresentationTypeList.Codes._4Direct;
			AssertNull("Declarant Type is '4' but no valid representative exists", GetProvider().Representative);

			var representativeHeader = Factory.New<OrgHeader>();
			var representativeAddress = representativeHeader.Addresses.AddNew();
			declaration.JE_OA_Representative = representativeAddress.PK;
			AssertNotNull("Declarant type is '4' and valid representative exists", GetProvider().Representative);
		});
	}
}
