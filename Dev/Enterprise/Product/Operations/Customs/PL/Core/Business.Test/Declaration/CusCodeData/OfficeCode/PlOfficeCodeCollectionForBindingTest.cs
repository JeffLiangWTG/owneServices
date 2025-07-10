using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

[TestedType(typeof(PlOfficeCodeCollectionForBinding))]
sealed class PlOfficeCodeCollectionForBindingTest : BusinessObjectCollectionViewTestCase<PlOfficeCodeCollectionForBinding>
{
	public void TestCollectionIsFiltered()
		=> AssertContainsExactElementsInAnyOrder(
			expected: new [] {
				EuOfficeCodesTypes.Codes.AuthorityControlCode,
				EuOfficeCodesTypes.Codes.OfficeOfPresentation,
				EuOfficeCodesTypes.Codes.CustomsOfficeForControllingEconomicalProcedure },
			actual: officeCodeCollectionForBinding.Select(x => x.CY_Code));

	public void TestAddingNew() => CombineAssertions(() =>
	{
		var officeCode = officeCodeCollectionForBinding.AddNew();
		officeCode.CY_Code = EuOfficeCodesTypes.Codes.CompetentAuthorityOfRecovery;
		AssertContainsExactElementsInAnyOrder("Test adding new to the PlOfficeCodeCollectionForBinging affects the declaration.CustomsOffices",
			expected: new [] {
				EuOfficeCodesTypes.Codes.AuthorityControlCode,
				EuOfficeCodesTypes.Codes.OfficeOfPresentation,
				EuOfficeCodesTypes.Codes.CustomsOfficeForControllingEconomicalProcedure,
				EuOfficeCodesTypes.Codes.OfficeOfExit,
				EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent,
				EuOfficeCodesTypes.Codes.CompetentAuthorityOfRecovery },
			actual: declaration.CustomsOffices.Select(x => x.CY_Code));

		declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.CompetentAuthorityOfDispatch);
		AssertContainsExactElementsInAnyOrder("Test adding new to the declaration.CustomsOffices affects the PlOfficeCodeCollectionForBinging",
			expected: new [] {
				EuOfficeCodesTypes.Codes.AuthorityControlCode,
				EuOfficeCodesTypes.Codes.OfficeOfPresentation,
				EuOfficeCodesTypes.Codes.CustomsOfficeForControllingEconomicalProcedure,
				EuOfficeCodesTypes.Codes.CompetentAuthorityOfRecovery,
				EuOfficeCodesTypes.Codes.CompetentAuthorityOfDispatch },
			actual: officeCodeCollectionForBinding.Select(x => x.CY_Code));
	});

	public void TestDeletion() => CombineAssertions(() =>
	{
		officeCodeCollectionForBinding.RemoveAndDelete(officeCodeCollectionForBinding.Cast<EuOfficeCode>().First(x => x.CY_Code == EuOfficeCodesTypes.Codes.AuthorityControlCode));
		AssertContainsExactElementsInAnyOrder("Test deletion from the PlOfficeCodeCollectionForBinging affects the declaration.CustomsOffices",
			expected: new [] {
				EuOfficeCodesTypes.Codes.OfficeOfPresentation,
				EuOfficeCodesTypes.Codes.CustomsOfficeForControllingEconomicalProcedure,
				EuOfficeCodesTypes.Codes.OfficeOfExit,
				EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent },
			actual: declaration.CustomsOffices.Select(x => x.CY_Code));

		declaration.CustomsOffices.RemoveAndDelete(declaration.CustomsOffices.Cast<EuOfficeCode>().First(x => x.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfPresentation));
		AssertContainsExactElementsInAnyOrder("Test deletion from then declaration.CustomsOffices affects the PlOfficeCodeCollectionForBinging",
			expected: new [] { EuOfficeCodesTypes.Codes.CustomsOfficeForControllingEconomicalProcedure },
			actual: officeCodeCollectionForBinding.Select(x => x.CY_Code));
	});

	public void TestLoadAfterSave() => CombineAssertions(() =>
	{
		declaration.CustomsOffices.RemoveAndDeleteAll();
		var officeCode = officeCodeCollectionForBinding.AddNew();
		officeCode.CY_Code = EuOfficeCodesTypes.Codes.AuthorityControlCode;
		declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExit);
		Factory.Save();

		declaration = Factory.Load<JobDeclaration>(declaration.PK);
		AssertContainsExactElementsInAnyOrder(
			"CustomsOffices",
			expected: new [] {
				EuOfficeCodesTypes.Codes.AuthorityControlCode,
				EuOfficeCodesTypes.Codes.OfficeOfExit },
			actual: declaration.CustomsOffices.Select(x => x.CY_Code));
		AssertContainsExactElementsInAnyOrder(
			"PlOfficeCodeCollectionForBinging",
			expected: new [] { EuOfficeCodesTypes.Codes.AuthorityControlCode },
			actual: officeCodeCollectionForBinding.Select(x => x.CY_Code));
	});

	public void TestUpdateElement() => CombineAssertions(() =>
	{
		var officeOfPresentation = declaration.CustomsOffices.Cast<EuOfficeCode>().First(x => x.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfPresentation);
		officeOfPresentation.CY_Data = "ABC";

		officeOfPresentation = officeCodeCollectionForBinding.Cast<EuOfficeCode>().First(x => x.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfPresentation);
		AssertEquals("Update in the CustomsOffices affects CustomsOfficesForBinding", "ABC", officeOfPresentation.CY_Data);
		officeOfPresentation.CY_Data = "DCU";

		officeOfPresentation = declaration.CustomsOffices.Cast<EuOfficeCode>().First(x => x.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfPresentation);
		AssertEquals("Update in the CustomsOfficesForBinding affects CustomsOffices", "DCU", officeOfPresentation.CY_Data);
	});

	protected override PlOfficeCodeCollectionForBinding GetCollectionToTest()
	{
		declaration = Factory.New<JobDeclaration>();
		return new PlOfficeCodeCollectionForBinding(declaration);
	}

	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		var result = Factory.New<OfficeCode>();
		result.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfPresentation;
		return result;
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();

		declaration.CustomsOffices.RemoveAndDeleteAll();
		declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.AuthorityControlCode);
		declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfPresentation);
		declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.CustomsOfficeForControllingEconomicalProcedure);
		declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExit);
		declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent);
		officeCodeCollectionForBinding = new (declaration);
	}

	JobDeclaration declaration;
	PlOfficeCodeCollectionForBinding officeCodeCollectionForBinding;
}
