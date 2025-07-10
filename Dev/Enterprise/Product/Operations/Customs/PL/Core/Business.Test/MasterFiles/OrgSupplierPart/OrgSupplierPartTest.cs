using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.MasterFiles;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(OrgSupplierPart))]
class OrgSupplierPartTest : Customs.Business.Testing.OrgSupplierPartTest
{
	public void TestCustomsCountryCodeIsCorrect()
	{
		using (Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
		{
			var part = Factory.New<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			AssertEquals(Core.Constants.CountryCodes.Poland, pivot.CI_RN_NKCountry);
		}
	}

	protected override Type ExpectedClassificationCollectionType => typeof(ClassificationCollection<CusClassification>);

	#region GetNewBusinessObject
	protected override BusinessObject GetNewBusinessObject()
	{
		return OrgSupplierPart.New(Factory);
	}
	#endregion
}
