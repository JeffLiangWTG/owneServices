using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CommonCusPermitHeaderTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var header = (CommonCusPermitHeader)Factory.New<Integration.Customs.IBaseCusPermitHeader>();
			header.CPH_ApplicationCode = Common.Shared.CusPermitHeaderApplicationCodeList.Codes.Permit;
			var row = ((INeedRow)header).Row;
			var typeDecider = new CommonCusPermitHeaderTypeDecider();
			var typeForLoad = typeDecider.GetTypeForLoad(row, Factory);
			AssertEquals("Enterprise.Customs.Business.BaseCusPermitHeader", typeForLoad.FullName);

			header.CPH_ApplicationCode = Common.Shared.CusPermitHeaderApplicationCodeList.Codes.Authorisation;
			typeForLoad = typeDecider.GetTypeForLoad(row, Factory);
			AssertEquals("Enterprise.Customs.Business.CusAuthorisationHeader", typeForLoad.FullName);

			header.CPH_ApplicationCode = Common.Shared.CusPermitHeaderApplicationCodeList.Codes.Guarantee;
			typeForLoad = typeDecider.GetTypeForLoad(row, Factory);
			AssertEquals("Enterprise.Customs.Business.BaseCusGuaranteeHeader", typeForLoad.FullName);

			header.CPH_ApplicationCode = Common.Shared.CusPermitHeaderApplicationCodeList.Codes.Rule;
			typeForLoad = typeDecider.GetTypeForLoad(row, Factory);
			AssertEquals("Enterprise.Customs.Business.CustomsRule", typeForLoad.FullName);

			header.CPH_ApplicationCode = Common.Shared.CusPermitHeaderApplicationCodeList.Codes.Operational;
			typeForLoad = typeDecider.GetTypeForLoad(row, Factory);
			AssertEquals("Enterprise.Customs.BR.Business.CusLPCOHeader", typeForLoad.FullName);

			header.CPH_ApplicationCode = "Z!";
			typeForLoad = typeDecider.GetTypeForLoad(row, Factory);
			AssertNull(typeForLoad);
			AssertEquals("CommonCusPermitHeader for Application Code 'Z!' is unknown", ErrorReporter.LastKeyReported);
			AssertEquals("Cannot determine the CommonCusPermitHeader object for Application Code 'Z!'", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}
	}
}
