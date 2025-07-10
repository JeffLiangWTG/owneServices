using System;
using CargoWise.Types;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class GoodsItemSupportingDocumentProvider_CC515_CC513Test : AESGoodsItemSupportingDocumentProviderTest
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

	protected override AESGoodsItemSupportingDocumentProvider GetProvider(Func<ZDecimal> getAmount, Func<ZDecimal> getQuantity = null) =>
		new GoodsItemSupportingDocumentProvider_CC515_CC513(cusSupportingInfo, getAmount, getQuantity, isAesTransitionPeriod: true);
}
