using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(OrgSupplierPart))]
	sealed class OrgSupplierPartTest : Customs.Business.Testing.OrgSupplierPartTest
	{
		public void TestCustomsCountryCodeIsCorrect()
		{
			using (MasterFiles.Business.GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var part = Factory.New<OrgSupplierPart>();
				var pivot = part.PivotsForBinding.AddNew();
				AssertEquals(Core.Constants.CountryCodes.SouthAfrica, pivot.CI_RN_NKCountry);
			}
		}

		protected override BusinessObject GetNewBusinessObject() => OrgSupplierPart.New(Factory);

		protected override Type ExpectedClassificationCollectionType => typeof(ClassificationCollection<CusClassification>);
	}
}
