using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(RestrictedCodeCollection))]
	sealed class RestrictedCodeCollectionTest : Customs.Business.Testing.CusCodeDataCollectionTest<RestrictedCode>
	{
		protected override Customs.Business.CusCodeDataCollection<RestrictedCode> GetCusCodeDataCollection() => new RestrictedCodeCollection(Factory.New<OrgHeader>().CountryData, RestrictedCodeTypeList.Codes.RestrictedSPI);

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var code = Factory.New<RestrictedCode>();
			var wrapper = OrgHeaderWrapper.New(Factory.New<OrgHeader>());
			code.CY_Type = CusCodeDataTypeList.Codes.IORBusinessRules;
			code.CY_Code = RestrictedCodeTypeList.Codes.RestrictedSPI;
			code.CY_ParentID = wrapper.CountryData.PK;
			code.CY_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			return code;
		}
	}
}
