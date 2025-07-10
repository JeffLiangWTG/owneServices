using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(CusEntryHeaderLookups))]
sealed class CusEntryHeaderLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestCH_ReCalcDeclType_List() => CombineAssertions(() =>
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var lookups = entryHeader.Lookups;

		AssertEquals("without CopyStatus", expected: 0, lookups.CH_ReCalcDeclType_List.Count);

		declaration.JE_CopyStatus = NODeclarationCopyStatus.Codes.Recalculation;
		AssertCodeDescriptionPairList("CopyStatus = 'REC' Recalculation", lookups.CH_ReCalcDeclType_List,
			("EB/RE", "EB/RE"),
			("SO", "SO")
		);

		declaration.JE_CopyStatus = NODeclarationCopyStatus.Codes.ReExport;
		AssertCodeDescriptionPairList("CopyStatus = 'REX' ReExport", lookups.CH_ReCalcDeclType_List,
			("MA", "MA")
		);

		declaration.JE_CopyStatus = NODeclarationCopyStatus.Codes.FinalImport;
		AssertCodeDescriptionPairList("CopyStatus = 'FIN' FinalImport", lookups.CH_ReCalcDeclType_List,
			("EN", "EN")
		);
	});

	public void TestCH_ReCalcDeclType_List_Cached()
	{
		var entryHeader = Factory.NewMoq<CusEntryHeader>().Object;
		var codeList = entryHeader.Lookups.CH_ReCalcDeclType_List;
		AssertSame("Cached", entryHeader.Lookups.CH_ReCalcDeclType_List, codeList);
	}

	public void TestPaymentMethodCodeList() => CombineAssertions(() =>
	{
		var entryHeaderMock = Factory.NewMoq<CusEntryHeader>();
		_ = entryHeaderMock.SetupGet(x => x.HasPayments).Returns(false);
		var lookups = entryHeaderMock.Object.Lookups;
		AssertType<NOPaymentMethodCodeList>(lookups.PaymentMethodCodeList);

		AssertCodeDescriptionPairList("When HasPayments is false", lookups.PaymentMethodCodeList,
			("N", "No Duties/VAT payable")
		);

		_ = entryHeaderMock.SetupGet(x => x.HasPayments).Returns(true);
		AssertCodeDescriptionPairList("When HasPayments is true", lookups.PaymentMethodCodeList,
			("D", "Forwarders Day Credit"),
			("K", "Cash"),
			("M", "Importers Deferred")
		);
	});

	public void TestPaymentMethodCodeList_Cached()
	{
		var entryHeader = Factory.NewMoq<CusEntryHeader>().Object;
		var codeList = entryHeader.Lookups.PaymentMethodCodeList;
		AssertSame("Cached", entryHeader.Lookups.PaymentMethodCodeList, codeList);
	}

	public void TestGetCustomsEntryPhaseStatusList() => CombineAssertions(() =>
	{
		var entryHeader = Factory.NewMoq<CusEntryHeader>().Object;
		var codeList = entryHeader.Lookups.PhaseStatusList;
		AssertType<CustomsEntryPhaseStatusList>(codeList);
		AssertSame("Cached", entryHeader.Lookups.PhaseStatusList, codeList);
		AssertEquals("Codes from list", "REM, FIN", entryHeader.Lookups.PhaseStatusList.CodesAsString);
	});
}
