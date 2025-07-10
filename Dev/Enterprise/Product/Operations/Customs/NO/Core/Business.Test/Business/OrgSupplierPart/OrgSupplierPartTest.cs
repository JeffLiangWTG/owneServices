using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(OrgSupplierPart))]
sealed class OrgSupplierPartTest : Customs.Business.Testing.OrgSupplierPartTest
{
	public void TestCustomsCountryCodeIsCorrect()
	{
		using (MasterFiles.Business.GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
		{
			var part = Factory.New<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			AssertEquals(Core.Constants.CountryCodes.Norway, pivot.CI_RN_NKCountry);
		}
	}

	public void TestPivotsForBinding_Type()
	{
		var part = Factory.New<OrgSupplierPart>();
		AssertType<CusClassPartPivotCollection<CusClassPartPivot>>("CusClassPartPivots", part.PivotsForBinding);
	}

	protected override Type ExpectedClassificationCollectionType => typeof(ClassificationCollection<CusClassification>);

	protected override BusinessObject GetNewBusinessObject() => OrgSupplierPart.New(Factory);
}
