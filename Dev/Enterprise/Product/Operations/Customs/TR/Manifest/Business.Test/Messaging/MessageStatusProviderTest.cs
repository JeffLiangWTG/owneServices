namespace Enterprise.Customs.TR.Manifest.Business.Testing
{
	class MessageStatusProviderTest : ASYCUDA.Business.Testing.MessageStatusProviderTest
	{
		public override void TestAllowCancellationMessage()
		{
			AssertEquals(false, statusProvider.AllowCancellationMessage(header));
		}

		public override void TestAllowManifestCancellationMessage()
		{
			AssertEquals(false, statusProvider.AllowManifestCancellationMessage(header));
		}

		public override void TestAllowModificationMessage()
		{
			AssertEquals(false, statusProvider.AllowModificationMessage(header));
		}

		public override void TestAllowOriginalMessage()
		{
			AssertEquals(true, statusProvider.AllowOriginalMessage(header));
		}

		public override void TestHasManifestBeenAcceptedByCustoms()
		{
			AssertEquals(false, statusProvider.HasManifestBeenAcceptedByCustoms(header));
		}

		public override void TestHasManifestBeenSubmittedToCustoms()
		{
			AssertEquals(false, statusProvider.HasManifestBeenSubmittedToCustoms(header));
		}

		public override void TestMessageStatusCanBeReset()
		{
			AssertEquals(false, statusProvider.MessageStatusCanBeReset(header));
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Turkey;
			statusProvider = header.MessageStatusProvider;
		}

		protected override ASYCUDA.Business.MessageStatusProvider GetMessageStatusProvider()
		{
			return statusProvider;
		}

		AsycudaManifestHeader header;
		ASYCUDA.Business.MessageStatusProvider statusProvider;
	}
}
