using CargoWise.Types;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class DocumentProvider_CC515_CC513Test : AESDocumentProviderTest
{
	public void TestDescriptionInTransitionPeriod()
	{
		CombineAssertions(() =>
		{
			cusSupportingInfo.CSI_ReferenceNumber = ZString.Empty;
			AssertNull("Reference Number is empty", GetProvider(referenceNumberAsDescription: true).Description);

			cusSupportingInfo.CSI_Description = ZString.Empty;
			AssertNull("Description is empty", GetProvider(referenceNumberAsDescription: false).Description);

			cusSupportingInfo.CSI_ReferenceNumber = new('Z', 70);
			cusSupportingInfo.CSI_Description = new('Y', 70);
			AssertEquals("Reference Number is not empty", new string('Z', 35), GetProvider(referenceNumberAsDescription: true).Description);
			AssertEquals("Description is not empty", new string('Y', 70), GetProvider(referenceNumberAsDescription: false).Description);
		});
	}

	protected override AESDocumentProvider GetProvider(bool referenceNumberAsDescription) =>
		new DocumentProvider_CC515_CC513(cusSupportingInfo, referenceNumberAsDescription, isAesTransitionPeriod: true);
}
