using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(ImportMessageSendingObjectLookups))]
sealed class ImportMessageSendingObjectLookupsTest : MessageSendingObjectLookupsAbstractTest<ImportMessageSendingObjectLookups>
{
	protected override string MessageType => SharedJobMessageTypeList.Codes.Import;

	public void TestCustomsOfficeList()
	{
		var declarant = Factory.New<OrgHeader>();
		declarant.OH_Code = "Dec1";
		declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
		SetupCustomsOfficesForTest(CusAuthorizationHeaderTypeList.Codes.ImportCustomsDeclaration, "ABC", new[] { "DEF" }, declarant);
		CombineAssertions(() =>
		{
			AssertSame("Cached", lookups.CustomsOfficeList, lookups.CustomsOfficeList);
			AssertEquals("Codes from list", "ABC, DEF", lookups.CustomsOfficeList.CodesAsString);
			AssertEquals("Default code should be MCO", "ABC", lookups.CustomsOfficeList.DefaultCode);
		});
	}
}
