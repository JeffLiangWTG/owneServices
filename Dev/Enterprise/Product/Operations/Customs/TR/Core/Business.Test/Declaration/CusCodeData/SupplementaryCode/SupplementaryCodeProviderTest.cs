using System;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.TR.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing;
[TestedType(typeof(SupplementaryCodeProvider))]
sealed class SupplementaryCodeProviderTest : BaseSupplementaryCodeProviderTest<SupplementaryCodeProvider, SupplementaryCode>
{
	public void TestGetBySupplementaryCodeSupporterTR()
	{
		var provider = EU.Business.SupplementaryCodeProvider.GetBySupplementaryCodeSupporter(invoiceLine);
		AssertType<SupplementaryCodeProvider>(provider);
	}

	public void TestValidation()
	{
		var supplementaryCode = invoiceLine.AdditionalSupplementaryCodes.AddNew();
		var provider = EU.Business.SupplementaryCodeProvider.GetBySupplementaryCodeSupporter(invoiceLine);
		AssertType<SupplementaryCodeValidation>(provider.GetNewValidation(supplementaryCode));
	}

	protected override ZString CountryCodeForSupplementaryCodeProvider => "TR";

	protected override ZShort ExpectedAdditionalSupplementaryCodesStartingOrder => 3;

	protected override ZShort ExpectedNumberOfAdditionalSupplementaryCodes => 8;

	protected override Type ExpectedGetNewValidationReturnType => typeof(SupplementaryCodeValidation);

	protected override Type ExpectedGetNewLookupsReturnType => typeof(SupplementaryCodeLookups);

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
