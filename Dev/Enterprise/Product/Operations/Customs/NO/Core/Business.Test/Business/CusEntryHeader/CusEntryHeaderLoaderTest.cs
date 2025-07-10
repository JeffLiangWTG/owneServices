using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(CusEntryHeader.Loader))]
sealed class CusEntryHeaderLoaderTest : LoaderTestCase
{
	public void TestGetByBGMReferenceNumber() => CombineAssertions(() =>
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_BGMReference = "1234567892024010111223304";
		Factory.Save();

		var newFactoryInstance = new BusinessObjectFactory();
		var cusEntryHeaderLoader = new CusEntryHeader.Loader(newFactoryInstance);
		var header = cusEntryHeaderLoader.GetByBGMReferenceNumber("12345678920240101112233");
		AssertNotNull("When ReferenceNumber: 12345678920240101112233", header);
		AssertEquals("When ReferenceNumber: 12345678920240101112233 and CusEntryHeader exists with same reference number, PK", entryHeader.PK, header.PK);

		header = cusEntryHeaderLoader.GetByBGMReferenceNumber(string.Empty);
		AssertNull("When ReferenceNumber is empty", header);

		header = cusEntryHeaderLoader.GetByBGMReferenceNumber("1234567892024010111");
		AssertNull("When ReferenceNumber length is less than 23. Value: 1234567892024010111", header);

		header = cusEntryHeaderLoader.GetByBGMReferenceNumber("6767");
		AssertNull("When ReferenceNumber: 6767 and no CusEntryHeader exists", header);
	});

	protected override BusinessObject.Loader GetNewLoaderToTest() => new CusEntryHeader.Loader(Factory);
}
