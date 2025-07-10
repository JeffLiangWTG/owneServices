using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Manifest.Business.Testing;

[TestedType(typeof(PreviousDocumentLookups))]
sealed class PreviousDocumentLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestCodeList()
	{
		var today = ZDateTime.Now;
		var yesterday = today.AddDays(-1);
		var tomorrow = today.AddDays(1);

		var helper = new UniversalReferenceTestDataHelper(Factory);
		_ = helper.CreateCusCodeType(NO.Business.UniversalReferenceConstants.RefCusCodeListType.PreviousDocumentsForManifestBill, "Previous Documents");
		_ = helper.CreateCusCodeList(Core.Constants.CountryCodes.Norway, NO.Business.UniversalReferenceConstants.RefCusCodeListType.PreviousDocumentsForManifestBill, "N820", "Transit declaration", yesterday, tomorrow);
		_ = helper.CreateCusCodeList(Core.Constants.CountryCodes.Norway, NO.Business.UniversalReferenceConstants.RefCusCodeListType.PreviousDocumentsForManifestBill, "Y001", "Transit declaration", yesterday, tomorrow);
		Factory.Save();

		var doc = Factory.New<AsycudaBill>().PreviousDocuments.AddNew();
		var list = doc.Lookups.CodeList;

		((BusinessObjectCollection)list).Load();

		AssertType<PreviousDocumentRefCusCodeListCombinedCollection>("[PRE-CONDITION] CodeList", doc.Lookups.CodeList);
		CombineAssertions(() =>
		{
			AssertContainsExactElementsInAnyOrder("Codes", new[] { "Y001", "N820" }, list.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));
		});
	}
}
