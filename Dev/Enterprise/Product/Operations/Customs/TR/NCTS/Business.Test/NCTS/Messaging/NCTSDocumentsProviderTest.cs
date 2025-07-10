using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TR.Business;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	public class NCTSDocumentsProviderTest : TestCaseWithFactory
	{
		public void TestDocumentsMembers()
		{
			using (var helper = new NCTSMessageProviderTestHelper(Factory))
			{
				var header = helper.GetProviderNCTSHeader();
				var nctsHeaderProvider = new NCTSHeaderProvider(header);

				var goodsItems = nctsHeaderProvider.GoodsItems.ToArray();

				var supportingDocuments1 = goodsItems[0].SupportingDocuments.ToArray();
				var supportingDocuments2 = goodsItems[1].SupportingDocuments.ToArray();

				CombineAssertions("Supporting documents members with items", () =>
				{
					AssertEquals("Type", "705", supportingDocuments1[0].Type);
					AssertEquals("ReferenceNumber", "MSCIST67676", supportingDocuments1[0].ReferenceNumber);
					AssertEquals("ReferenceNumberLNG", TRMessageConstants.LanguageCode, supportingDocuments1[0].ReferenceNumberLNG);
					AssertEquals("ComplementofInformation", "19/02/2012", supportingDocuments1[0].ComplementofInformation);

					AssertEquals("Type", "706", supportingDocuments1[1].Type);
					AssertEquals("ReferenceNumber", "MSCIST67676", supportingDocuments1[1].ReferenceNumber);
					AssertEquals("ReferenceNumberLNG", TRMessageConstants.LanguageCode, supportingDocuments1[1].ReferenceNumberLNG);
					AssertEquals("ComplementofInformation", "19/02/2012", supportingDocuments1[1].ComplementofInformation);

					AssertEquals("Type", "707", supportingDocuments2[0].Type);
					AssertEquals("ReferenceNumber", "MSCIST67676", supportingDocuments2[0].ReferenceNumber);
					AssertEquals("ReferenceNumberLNG", TRMessageConstants.LanguageCode, supportingDocuments2[0].ReferenceNumberLNG);
					AssertEquals("ComplementofInformation", "19/02/2012", supportingDocuments2[0].ComplementofInformation);
				});

				var previousDocuments1 = goodsItems[0].PreviousDocuments.ToArray();
				var previousDocuments2 = goodsItems[1].PreviousDocuments.ToArray();

				CombineAssertions("Previous documents members with items", () =>
				{
					AssertNotEquals("CSI_Code not equals", "ANT", previousDocuments1[0].Type);
					AssertEquals("Type", "708", previousDocuments1[0].Type);
					AssertEquals("ReferenceNumber", "MSCIST67677", previousDocuments1[0].ReferenceNumber);
					AssertEquals("ReferenceNumberLNG", TRMessageConstants.LanguageCode, previousDocuments1[0].ReferenceNumberLNG);
					AssertEquals("ComplementofInformation", "19/02/2013", previousDocuments1[0].ComplementofInformation);

					AssertNotEquals("CSI_Code not equals", "ANT", previousDocuments1[1].Type);
					AssertEquals("Type", "709", previousDocuments1[1].Type);
					AssertEquals("ReferenceNumber", "MSCIST67677", previousDocuments1[1].ReferenceNumber);
					AssertEquals("ReferenceNumberLNG", TRMessageConstants.LanguageCode, previousDocuments1[1].ReferenceNumberLNG);
					AssertEquals("ComplementofInformation", "19/02/2013", previousDocuments1[1].ComplementofInformation);

					AssertNotEquals("CSI_Code not equals", "ANT", previousDocuments2[0].Type);
					AssertEquals("Type", "710", previousDocuments2[0].Type);
					AssertEquals("ReferenceNumber", "MSCIST67677", previousDocuments2[0].ReferenceNumber);
					AssertEquals("ReferenceNumberLNG", TRMessageConstants.LanguageCode, previousDocuments2[0].ReferenceNumberLNG);
					AssertEquals("ComplementofInformation", "19/02/2013", previousDocuments2[0].ComplementofInformation);
				});

				var additionalDocuments1 = goodsItems[0].SpecialMentionsDocuments.ToArray();
				var additionalDocuments2 = goodsItems[1].SpecialMentionsDocuments.ToArray();

				CombineAssertions("Additional documents members with items", () =>
				{
					AssertEquals("Type", "711", additionalDocuments1[0].Type);
					AssertEquals("ReferenceNumber", "MSCIST67678", additionalDocuments1[0].ReferenceNumber);
					AssertEquals("ReferenceNumberLNG", TRMessageConstants.LanguageCode, additionalDocuments1[0].ReferenceNumberLNG);
					AssertEquals("ComplementofInformation", "19/02/2014", additionalDocuments1[0].ComplementofInformation);
					AssertEquals("ComplementofInformation", Core.Constants.CountryCodes.Turkey, additionalDocuments1[0].CountryCode);

					AssertEquals("Type", "712", additionalDocuments1[1].Type);
					AssertEquals("ReferenceNumber", "MSCIST67678", additionalDocuments1[1].ReferenceNumber);
					AssertEquals("ReferenceNumberLNG", TRMessageConstants.LanguageCode, additionalDocuments1[1].ReferenceNumberLNG);
					AssertEquals("ComplementofInformation", "19/02/2014", additionalDocuments1[1].ComplementofInformation);
					AssertEquals("ComplementofInformation", Core.Constants.CountryCodes.Turkey, additionalDocuments1[1].CountryCode);

					AssertEquals("Type", "713", additionalDocuments2[0].Type);
					AssertEquals("ReferenceNumber", "MSCIST67678", additionalDocuments2[0].ReferenceNumber);
					AssertEquals("ReferenceNumberLNG", TRMessageConstants.LanguageCode, additionalDocuments2[0].ReferenceNumberLNG);
					AssertEquals("ComplementofInformation", "19/02/2014", additionalDocuments2[0].ComplementofInformation);
					AssertEquals("ComplementofInformation", Core.Constants.CountryCodes.Turkey, additionalDocuments2[0].CountryCode);
				});
			}
		}
	}
}
