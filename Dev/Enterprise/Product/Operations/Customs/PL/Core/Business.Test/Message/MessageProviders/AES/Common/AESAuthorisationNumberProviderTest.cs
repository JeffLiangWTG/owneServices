using System;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class AESAuthorisationNumberProviderTest : DataProviderTestCase<AESAuthorisationNumberProvider>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null CusAuthorizationUsage", "Value cannot be null.\r\nParameter name: authorisationUsage",
				() => new AESAuthorisationNumberProvider(null, 1));
		});
	}

	public void TestSequenceNumber() => AssertEquals(999, GetProvider().SequenceNumber);

	[TestDate(2023, 8, 12)]
	public void TestType_ValidDate()
	{
		CombineAssertions(() =>
		{
			AssertNull(GetProvider().Type);

			cusAuthorizationUsage.AGC_Code = "C521";
			AssertEquals("AGC_Code can't find in mapping", "C521", GetProvider().Type);

			cusAuthorizationUsage.AGC_Code = "ACR";
			AssertEquals("AGC_Code can find in mapping", "C521", GetProvider().Type);

			cusAuthorizationUsage.AGC_Code = "";
			AssertNull(GetProvider().Type);
		});
	}

	[TestDate(2023, 10, 12)]
	public void TestType_InvalidDate()
	{
		CombineAssertions(() =>
		{
			AssertNull(GetProvider().Type);

			cusAuthorizationUsage.AGC_Code = "C521";
			AssertEquals("AGC_Code can't find in mapping", "C521", GetProvider().Type);

			cusAuthorizationUsage.AGC_Code = "ACR";
			AssertEquals("AGC_Code can't find in mapping because of invalid date", "ACR", GetProvider().Type);

			cusAuthorizationUsage.AGC_Code = "";
			AssertNull(GetProvider().Type);
		});
	}

	public void TestReferenceNumber()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Empty Declaration", string.Empty, GetProvider().ReferenceNumber);

			cusAuthorizationUsage.EffectiveReferenceNumber = "123";
			AssertEquals("ReferenceNumber is 123", "123", GetProvider().ReferenceNumber);
		});
	}

	public void TestHolderOfAuthorisation()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Empty Declaration", string.Empty, GetProvider().HolderOfAuthorisation);

			var orgHeader = Factory.New<OrgHeader>();
			cusAuthorizationUsage.AGC_OH_Owner = orgHeader.PK;
			var eoriCode = orgHeader.CustomsCodes.AddNew();
			eoriCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			eoriCode.OK_CustomsRegNo = "ASD123";

			AssertEquals("OK_CustomsRegNo is ASD123 for EORI", "ASD123", GetProvider().HolderOfAuthorisation);
		});
	}

	protected override AESAuthorisationNumberProvider GetProvider() => new AESAuthorisationNumberProvider(cusAuthorizationUsage, 999);

	protected override void SetUp()
	{
		base.SetUp();

		var helper = new UniversalReferenceTestDataHelper(Factory);
		var eunau = EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU;
		helper.CreateCusMapType(eunau, MapDirectionList.Codes.OUT, "EUNAU", true);
		helper.CreateCusMap(eunau, "ACR", "C521", new ZDateTime(2023, 8, 10), new ZDateTime(2023, 8, 30), Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		Factory.Save();

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;

		instruction = declaration.CustomsEntryInstructions.AddNew();
		invoice = declaration.Invoices.AddNew();
		invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = instruction.PK;
		invoiceLine.JI_Description = "invoice line 1";
		cusAuthorizationUsage = invoiceLine.CusAuthorizationUsages.AddNew();
	}

	JobDeclaration declaration;
	JobComInvoiceLine invoiceLine;
	JobComInvoiceHeader invoice;
	CusEntryInstruction instruction;
	CusAuthorizationUsage cusAuthorizationUsage;
}
