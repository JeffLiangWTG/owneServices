using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(ExportMessageSendingObjectLookups))]
sealed class ExportMessageSendingObjectLookupsTest : MessageSendingObjectLookupsAbstractTest<ExportMessageSendingObjectLookups>
{
	protected override string MessageType => SharedJobMessageTypeList.Codes.Export;

	public void TestCustomsOfficeList()
	{
		var declarant = Factory.New<OrgHeader>();
		declarant.OH_Code = "Dec1";
		declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
		SetupCustomsOfficesForTest(CusAuthorizationHeaderTypeList.Codes.ExportCustomsDeclaration, "GHI", new[] { "JKL" }, declarant);
		CombineAssertions(() =>
		{
			AssertSame("Cached", lookups.CustomsOfficeList, lookups.CustomsOfficeList);
			AssertEquals("Codes from list", "GHI, JKL", lookups.CustomsOfficeList.CodesAsString);
			AssertEquals("Default code should be MCO", "GHI", lookups.CustomsOfficeList.DefaultCode);
		});
	}
}
