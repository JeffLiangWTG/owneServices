using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing
{
	[TestedType(typeof(ExportJobComInvoiceLineLookups))]
	sealed class ExportJobComInvoiceLineLookupsTest : JobComInvoiceLineLookupsAbstractTest<ExportJobComInvoiceLineLookups>
	{
		protected override string MessageType => JobMessageTypeList.Codes.Export;

		public void Test_JI_PrimaryPreference()
		{
			var codeList = (CodeDescriptionPairList)lookups.PrimaryPreferenceList;
			CombineAssertions(() =>
			{
				AssertSame("Cached", lookups.PrimaryPreferenceList, codeList);
				AssertType<PrimaryPreferenceCodeList>("Type", codeList);
				AssertEquals("Codes from list1", "A, B, C, G, P, N", codeList.CodesAsString);
			});
		}
	}
}
