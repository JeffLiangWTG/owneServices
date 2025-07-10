using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

[TestedType(typeof(PlOfficeCodeCollection))]
sealed class PLOfficeCodeCollectionTest : EuOfficeCodeCollectionTest
{
	public void TestNotFilteredPlOfficeCodeCollection()
	{
		AssertContainsExactElementsInAnyOrder(
			expected: new [] {
				EuOfficeCodesTypes.Codes.AuthorityControlCode,
				EuOfficeCodesTypes.Codes.OfficeOfExit,
				EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent },
			actual: officeCodeCollection.Select(x => x.CY_Code));
	}

	protected override BusinessObjectCollection GetCollectionToTest()
	{
		var parent = Factory.New<JobDeclaration>();
		return new PlOfficeCodeCollection(parent);
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();

		declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.AuthorityControlCode);
		declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExit);
		declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent);
		Factory.Save();

		officeCodeCollection = new (declaration);
		officeCodeCollection.Load();
	}

	JobDeclaration declaration;
	PlOfficeCodeCollection officeCodeCollection;
}
