using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business.Testing;

abstract class CC515513RootProviderTest<TDataProvider, TExportOperation>
	: AESBaseProviderWithPartiesBaseTest<TDataProvider>
	where TExportOperation : class, IExportOperation
	where TDataProvider : RootProviderBase_CC515_CC513<TExportOperation>, IAESBase
{
	public void TestAuthorisationNumbers()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Empty declaration", 0, GetProvider().AuthorisationNumbers.Count);

			var authUsage = instruction.CusAuthorizationUsages.AddNew();
			authUsage.AGC_Code = "A";
			authUsage.AGC_Number = "123";
			var authUsage2 = instruction.CusAuthorizationUsages.AddNew();
			authUsage2.AGC_Code = "A";
			authUsage2.AGC_Number = "123";
			var authUsage3 = instruction.CusAuthorizationUsages.AddNew();
			authUsage3.AGC_Code = "B";
			authUsage3.AGC_Number = "123";
			var authUsage4 = instruction.CusAuthorizationUsages.AddNew();
			authUsage4.AGC_Code = "B";
			authUsage4.AGC_Number = "456";

			AssertEquals("4 CusAuthorizationUsages where 3 have unique AGC_Code and AGC_Number", 3, GetProvider().AuthorisationNumbers.Count);
		});
	}

	public void TestCustomsOfficeOfPresentationReferenceNumber()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Non Existing CustomsOfficeOfPresentation", string.Empty, GetProvider().CustomsOfficeOfPresentationReferenceNumber);
			var office = declaration.CustomsOfficesForBinding.AddNew();
			office.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfPresentation;
			office.CY_Data = ZString.Empty;
			AssertEquals("Empty CustomsOfficeOfPresentation", string.Empty, GetProvider().CustomsOfficeOfPresentationReferenceNumber);
			office.CY_Data = "asd";
			AssertEquals("Not Empty CustomsOfficeOfPresentation", "asd", GetProvider().CustomsOfficeOfPresentationReferenceNumber);
		});
	}

	public void TestCustomsOfficeOfExportReferenceNumber()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Empty CustomsOfficeOfExport", string.Empty, GetProvider().CustomsOfficeOfExportReferenceNumber);

			declaration.JE_CustomsOffice = "asd";
			AssertEquals("Not Empty CustomsOfficeOfExport", "asd", GetProvider().CustomsOfficeOfExportReferenceNumber);
		});
	}

	public void TestCustomsOfficeOfExitDeclaredReferenceNumber()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Empty CustomsOfficeOfExit", string.Empty, GetProvider().CustomsOfficeOfExitDeclaredReferenceNumber);

			declaration.JE_OfficeOfEntryExit = "asd";
			AssertEquals("Not empty CustomsOfficeOfExit", "asd", GetProvider().CustomsOfficeOfExitDeclaredReferenceNumber);
		});
	}

	public void TestCustomsOfficeOfSupervisingReferenceNumber()
	{
		AssertEquals("Non Existing CustomsOfficeOfSupervising", string.Empty, GetProvider().CustomsOfficeOfSupervisingReferenceNumber);
		var office = declaration.CustomsOfficesForBinding.AddNew();
		office.CY_Code = EuOfficeCodesTypes.Codes.SupervisingCustomsOffice;
		office.CY_Data = ZString.Empty;
		AssertEquals("Empty CustomsOfficeOfSupervising", string.Empty, GetProvider().CustomsOfficeOfSupervisingReferenceNumber);
		office.CY_Data = "asd";
		AssertEquals("Not Empty CustomsOfficeOfSupervising", "asd", GetProvider().CustomsOfficeOfSupervisingReferenceNumber);
	}

	public virtual void TestCurrencyExchange() => AssertNotNull(GetProvider().CurrencyExchange);

	public void TestDeferredPayment() => AssertNull(GetProvider().DeferredPayment);

	public void TestGoodsShipment() => AssertNotNull(GetProvider().GoodsShipment);

	public void TestExportOperation() => AssertNotNull(GetProvider().ExportOperation);

	public override void TestExporterType()
	{
		var supplierHeader = Factory.New<OrgHeader>();
		var supplierAddress = supplierHeader.Addresses.AddNew();
		declaration.JE_OA_SupplierAddress = supplierAddress.PK;

		AssertType<ExporterProvider_CC515_CC513>(GetProvider().Exporter);
	}

	public override void TestDeclarantType()
	{
		var declarantHeader = Factory.New<OrgHeader>();
		var declarantAddress = declarantHeader.Addresses.AddNew();
		declaration.JE_OA_DeclarantAddress = declarantAddress.PK;

		AssertType<DeclarantWithIdentificationNumbersProvider_CC515_CC513>(GetProvider().Declarant);
	}
}
