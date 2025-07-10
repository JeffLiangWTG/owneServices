using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.MasterFiles.Testing
{
	[TestedType(typeof(OrgSupplierPart))]
	class OrgSupplierPartTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCustomsCountryCodeIsCorrect()
		{
			using (Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var part = Factory.New<OrgSupplierPart>();
				var pivot = part.PivotsForBinding.AddNew();
				AssertEquals(Core.Constants.CountryCodes.Turkey, pivot.CI_RN_NKCountry);
			}
		}

		protected override BusinessObject GetNewBusinessObject() => OrgSupplierPart.New(Factory);
	}
}
