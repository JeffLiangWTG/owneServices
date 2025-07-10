using CargoWise.Types;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class GoodsItemPreviousDocumentProvider_CC515_CC513Test : AESGoodsItemPreviousDocumentProviderTest
{
	public void TestDescriptionInTransitionPeriod()
	{
		CombineAssertions(() =>
		{
			cusSupportingInfo.CSI_ReferenceNumber = new('Z', 70);
			AssertEquals("Description is not empty", new string('Z', 35), GetProvider().Description);

			cusSupportingInfo.CSI_ReferenceNumber = ZString.Empty;
			AssertNull("Description is empty", GetProvider().Description);
		});
	}

	protected override AESGoodsItemPreviousDocumentProvider GetProvider(ZString procedureCode) =>
		new GoodsItemPreviousDocumentProvider_CC515_CC513(cusSupportingInfo, procedureCode, isAesTransitionPeriod: true);
}
