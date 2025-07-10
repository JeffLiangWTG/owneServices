using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

[TestedType(typeof(ImportSingleLineEntry))]
sealed class ImportSingleLineEntryTest : NonPersistentBusinessObjectTestCase
{
	public void TestGoodsOrigin_MaxLength()
	{
		var importSingleLineEntry = (ImportSingleLineEntry)GetNewBusinessObject();

		AssertEquals(2, importSingleLineEntry.GoodsOriginInfo.MaxLength);
	}

	public void TestGoodsOrigin_ResourceStringData()
	{
		AssertEquals("[34] Goods Origin",
			DataBoundResourceStrings.GetDataForProperty(typeof(ImportSingleLineEntry), nameof(ImportSingleLineEntry.GoodsOrigin)).Caption);
	}

	public void TestPreviousDocument_MaxLength()
	{
		var importSingleLineEntry = (ImportSingleLineEntry)GetNewBusinessObject();

		AssertEquals(6, importSingleLineEntry.PreviousDocumentInfo.MaxLength);
	}

	public void TestPreviousDocument_ResourceStringData()
	{
		AssertEquals("Previous Document",
			DataBoundResourceStrings.GetDataForProperty(typeof(ImportSingleLineEntry), nameof(ImportSingleLineEntry.PreviousDocument)).Caption);
	}

	public void TestPreviousDocumentNumber_MaxLength()
	{
		var importSingleLineEntry = (ImportSingleLineEntry)GetNewBusinessObject();

		AssertEquals(35, importSingleLineEntry.PreviousDocumentNumberInfo.MaxLength);
	}

	public void TestPreviousDocumentNumber_ResourceStringData()
	{
		var resourceString = DataBoundResourceStrings.GetDataForProperty(typeof(ImportSingleLineEntry), nameof(ImportSingleLineEntry.PreviousDocumentNumber));
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Previous Document Reference Number", resourceString.Caption);
			AssertEquals("Medium Caption", "Reference Number", resourceString.MediumCaption);
			AssertEquals("Short Caption", "Ref. Num.", resourceString.ShortCaption);
		});
	}

	protected override BusinessObject GetNewBusinessObject() => new ImportSingleLineEntry(Factory.New<JobDeclaration>());

	public void TestTariffNumberMaxLen()
	{
		var tariffNumberInfo = ((ImportSingleLineEntry)GetNewBusinessObject()).TariffNumberInfo;

		AssertEquals("formated tariff number should have 13 MaxLength", 13, tariffNumberInfo.MaxLength);
	}
}
