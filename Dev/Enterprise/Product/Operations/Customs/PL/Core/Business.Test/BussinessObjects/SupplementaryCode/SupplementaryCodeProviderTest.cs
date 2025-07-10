using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.PL;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(SupplementaryCodeProvider))]
sealed class SupplementaryCodeProviderTest : BaseSupplementaryCodeProviderTest<SupplementaryCodeProvider, SupplementaryCode>
{
	public void TestGetBySupplementaryCodeSupporterDE()
	{
		var provider = EU.Business.SupplementaryCodeProvider.GetBySupplementaryCodeSupporter(invoiceLine);
		AssertType<SupplementaryCodeProvider>(provider);
	}

	public void TestValidation()
	{
		var supplementaryCode = invoiceLine.AdditionalSupplementaryCodes.AddNew();
		var provider = EU.Business.SupplementaryCodeProvider.GetBySupplementaryCodeSupporter(invoiceLine);

		CombineAssertions(() =>
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			AssertType<SupplementaryCodeValidation>("GetNewValidation Export", provider.GetNewValidation(supplementaryCode));
			AssertType<SupplementaryCodeValidation>("SupplementaryCode Export", supplementaryCode.Validation);

			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			AssertType<SupplementaryCodeValidation>("GetNewValidation Import", provider.GetNewValidation(supplementaryCode));
			AssertType<SupplementaryCodeValidation>("SupplementaryCode Import", supplementaryCode.Validation);

			declaration.JE_MessageType = PLJobMessageTypeList.Codes.ExitSummary;
			AssertType<EU.Business.SupplementaryCodeValidation>("GetNewValidation Exit Summary", provider.GetNewValidation(supplementaryCode));
			AssertType<EU.Business.SupplementaryCodeValidation>("SupplementaryCode Exit Summary", supplementaryCode.Validation);
		});
	}

	protected override ZString CountryCodeForSupplementaryCodeProvider => Core.Constants.CountryCodes.Poland;

	protected override ZShort ExpectedAdditionalSupplementaryCodesStartingOrder => 3;

	protected override ZShort ExpectedNumberOfAdditionalSupplementaryCodes => 8;

	protected override Type ExpectedGetNewValidationReturnType => typeof(SupplementaryCodeValidation);

	protected override Type ExpectedGetNewLookupsReturnType => typeof(BaseSupplementaryCodeLookups);

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		var invoiceHeader = declaration.Invoices.AddNew();
		invoiceLine = invoiceHeader.InvoiceLines.AddNew();
	}
	JobDeclaration declaration;
	JobComInvoiceLine invoiceLine;
}
