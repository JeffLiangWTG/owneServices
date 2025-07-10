using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TR.Messaging;

namespace Enterprise.Customs.TR.Manifest.Business.Testing
{
	public class LadingExportProviderTest : TestCaseWithFactory
	{
		public void TestLadingExportsMembers()
		{
			using (var helper = new ProviderTestHelper(Factory))
			{
				var header = helper.GetProviderHeader();
				var sumDec = new ManifestMessageProvider(header);
				IBillofLading billofLading = sumDec.BillofLadings.FirstOrDefault();
				ILadingExports ladingExports = billofLading.LadingExports.FirstOrDefault();
				CombineAssertions("Lading Exports Members", () =>
				{
					AssertEquals(Convert.ToDecimal(123.89), ladingExports.GrossWeight);
					AssertEquals(Convert.ToInt32(20), ladingExports.BoxQuantity);
					AssertEquals("340300IM123456", ladingExports.ReferenceNumber);
					AssertEquals(TurkishConstants.AnswerNo, ladingExports.IsSubType);
					AssertEquals("TCGB", ladingExports.IsProcedure);
				});
			}
		}
	}
}
