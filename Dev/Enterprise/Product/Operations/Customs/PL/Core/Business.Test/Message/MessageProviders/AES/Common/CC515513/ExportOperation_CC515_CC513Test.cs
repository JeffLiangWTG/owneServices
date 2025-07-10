using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business.Testing;

abstract class ExportOperation_CC515_CC513Test<TProvider, TInterface> : AESExportOperationProviderTestBase<TProvider, TInterface>
		where TInterface : class, IExportOperation
		where TProvider : ExportOperationProvider_CC515_CC513, TInterface
{
	public override void TestEadPrint()
	{
		base.TestEadPrint();
		AesRuleTestHelper.TestR0089E(EntryInstruction, () => GetProvider().EadPrint);
	}

	public override void TestPresentationOfTheGoodsDateAndTime()
	{
		base.TestPresentationOfTheGoodsDateAndTime();

		var testDate = DateTime.UtcNow.Date;
		Declaration.ZG_PresentationStartDate = testDate;
		AesRuleTestHelper.TestR0089E(EntryInstruction, () => GetProvider().PresentationOfTheGoodsDateAndTime);
	}

	public override void TestSpecificCircumstanceIndicator()
	{
		base.TestSpecificCircumstanceIndicator();
		AesRuleTestHelper.TestR0089E(EntryInstruction, () => GetProvider().SpecificCircumstanceIndicator);
	}

	public override void TestStorage()
	{
		base.TestStorage();

		Declaration.JE_LocationQualifier = QualifierOfTheIdentificationList.Codes.V;
		Declaration.JE_CustomsOffice = "asd";
		Declaration.JE_OfficeOfEntryExit = "asd";
		EntryInstruction.ZG_ExportManifest = false;
		AesRuleTestHelper.TestR0089E(EntryInstruction, () => GetProvider().Storage);
	}
}
