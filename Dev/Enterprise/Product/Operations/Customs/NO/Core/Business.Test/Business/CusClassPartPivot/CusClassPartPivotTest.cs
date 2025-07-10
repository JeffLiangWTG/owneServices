using System.ComponentModel;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(CusClassPartPivot))]
sealed class CusClassPartPivotTest : EnterpriseBusinessObjectTestCase
{
	public void TestType()
	{
		AssertType<CusClassPartPivot>(pivot);
	}

	public void TestLookupsType()
	{
		AssertType<CusClassPartPivotLookups>(pivot.Lookups);
	}

	public void TestCI_Supplement1_Attributes() => CombineAssertions(() =>
		AssertEntity<CusClassPartPivot>()
			.HasProperty(l => l.CI_Supplement1)
			.WithAttribute<MaxLengthAttribute>(l => l.MaxLength == 35));

	public void TestCI_Supplement2_Attributes() => CombineAssertions(() =>
		AssertEntity<CusClassPartPivot>()
			.HasProperty(l => l.CI_Supplement2)
			.WithAttribute<MaxLengthAttribute>(l => l.MaxLength == 35));

	public void TestVatCode_Attributes() => CombineAssertions(() =>
		AssertEntity<CusClassPartPivot>()
			.HasProperty(l => l.CI_ZZF_NKTaxType)
			.WithCaption("VAT Code")
			.WithList("Lookups+VATCodeList"));

	public void TestReducedCustomsFlag_Attributes() => CombineAssertions(() =>
		AssertEntity<CusClassPartPivot>()
			.HasProperty(l => l.CI_ReducedCustomsFlag)
			.WithCaption("Reduced custom")
			.WithList("Lookups+ReducedCustomsFlagList"));

	public void TestStateOrRegionOfOrigin_Attributes() => CombineAssertions(() =>
		AssertEntity<CusClassPartPivot>()
			.HasProperty(l => l.CI_RW_NKOriginState)
			.WithCaption("County Of Origin")
			.WithList("Lookups+CountyOfOriginList"));

	public void TestCI_Supplement1()
	{
		pivot.CI_Supplement1 = "CD01";
		AssertCI_Supplement(1);
	}

	public void TestCI_Supplement2()
	{
		pivot.CI_Supplement2 = "CD01";
		AssertCI_Supplement(2);
	}

	void AssertCI_Supplement(ZShort index) => CombineAssertions($"Supplement: {index}", () =>
	{
		var code = new BaseSupplementaryCode.Loader(Factory).Load(pivot, index);
		AssertEquals("CY_Code", "CD01", code.CY_Code);
		AssertEquals("CY_Order", index, code.CY_Order);
		AssertEquals("CY_Type", BaseCusCodeDataTypeList.Codes.SupplementaryCode, code.CY_Type);
	});

	public void TestAdditionalSupplementaryCodes()
	{
		var additionalSupplementaryCode = pivot.AdditionalSupplementaryCodes;
		AssertType<SupplementaryCodeCollection>(additionalSupplementaryCode);

		var supplementaryCode = additionalSupplementaryCode.AddNew("CD1");
		AssertType<SupplementaryCode>(supplementaryCode);
	}

	public void TestCI_AdditionalSupplements_Attributes() => CombineAssertions(() =>
		AssertEntity<CusClassPartPivot>()
			.HasProperty(l => l.CI_AdditionalSupplements)
			.WithAttribute<ReadOnlyAttribute>(r => r.IsReadOnly));

	public void TestSupplementaryCodeSupporter_Tariff() => AssertNull(GetSupplementaryCodeSupporter().Tariff);

	public void TestSupplementaryCodeSupporter_SupplementaryCodesFieldType() =>
		AssertEquals("SupplementaryCodesFieldType", "Text", GetSupplementaryCodeSupporter().SupplementaryCodesFieldType);

	public void TestSupplementaryCodeSupporter_SupplementaryCodeCaption() =>
		AssertNull("SupplementaryCodeCaption", GetSupplementaryCodeSupporter().SupplementaryCodeCaption);

	public void TestSupplementaryCodeSupporter_GetCountryCodeForCodeProvider()
	{
		var supplementaryCodeSupporter = GetSupplementaryCodeSupporter();
		CombineAssertions(() =>
		{
			AssertEquals("When OrgSupplierPart is null", GlbCompany.CurrentCompany.Country.Code, supplementaryCodeSupporter.GetCountryCodeForCodeProvider());

			var part = Factory.New<OrgSupplierPart>();
			part.PivotsForBinding.Add(pivot);
			AssertEquals("When OrgSupplierPart Country Code is present", part.PivotsForBinding.countryCode, supplementaryCodeSupporter.GetCountryCodeForCodeProvider());
		});
	}

	public void TestSupplementaryCodeSupporter_RateSelectionCriteria() => AssertNull(GetSupplementaryCodeSupporter().RateSelectionCriteria);

	public void TestSupplementaryCodeSupporter_SupplementaryCodes()
	{
		pivot.CI_Supplement1 = "SUP1";
		pivot.CI_Supplement2 = "SUP2";
		pivot.AdditionalSupplementaryCodes.AddNew().CY_Code = "SUP3";

		AssertContainsExactElementsInAnyOrder(new ZString[] { "SUP3", "SUP1", "SUP2" }, GetSupplementaryCodeSupporter().SupplementaryCodes.Select(c => c.CY_Code).ToArray());
	}

	ISupplementaryCodeSupporter GetSupplementaryCodeSupporter()
	{
		ISupplementaryCodeSupporter supplementaryCodeSupporter = pivot;
		AssertNotNull("CusClassPartPivot as SupplementaryCodeSupporter", supplementaryCodeSupporter);
		return supplementaryCodeSupporter;
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		return pivot;
	}

	protected override void SetUp()
	{
		base.SetUp();
		pivot = Factory.New<CusClassPartPivot>();
	}
	CusClassPartPivot pivot;
}
