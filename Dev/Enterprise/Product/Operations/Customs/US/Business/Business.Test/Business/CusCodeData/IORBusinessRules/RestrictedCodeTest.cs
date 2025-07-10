using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(RestrictedCode))]
	sealed class RestrictedCodeTest : Customs.Business.Testing.CusCodeDataTest<RestrictedCode>
	{
		public void TestIRestrictedCodeIsCorrectlySetup()
		{
			AssertEquals(typeof(RestrictedCode), ObjectFactory.GetType<Integration.Customs.US.IRestrictedCode>());
		}

		public void TestDeleteWhenEmptyAndWhenParentIsDeleted()
		{
			GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates);
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var wrapper = OrgHeaderWrapper.New(org);
			var codes = new RestrictedCodeCollection(wrapper.CountryData, RestrictedCodeTypeList.Codes.RestrictedSPI);
			var code = codes.AddNew();
			Factory.Save();
			Assert(code.IsDeleted);
			code = codes.AddNew();
			code.CY_Data = "something";
			Factory.Save();
			Assert(!code.IsDeleted);
			wrapper.CountryData.Delete();
			Factory.Save();
			Assert(code.IsDeleted);
		}

		protected override BusinessObject GetNewBusinessObject() => Factory.New<RestrictedCode>();

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
			=> OrgHeaderWrapper.New(factory.NewWithValidTestData<OrgHeader>()).RestrictedSPIs.AddNew(RestrictedCodeTypeList.Codes.RestrictedSPI, "TTT");
	}
}
