namespace Enterprise.Customs.UY.Manifest.Business.Testing
{
	class MessageStatusProviderTest : ASYCUDA.Business.Testing.MessageStatusProviderTest
	{
		public override void TestAllowCancellationMessage()
		{
			header.AMA_MessageStatus = "ACP";
			AssertEquals(true, statusProvider.AllowCancellationMessage(header));
		}

		public override void TestAllowManifestCancellationMessage()
		{
			AssertEquals(false, statusProvider.AllowManifestCancellationMessage(header));
		}

		public override void TestAllowModificationMessage()
		{
			header.AMA_MessageStatus = "ACP";
			AssertEquals(true, statusProvider.AllowModificationMessage(header));
		}

		public override void TestAllowOriginalMessage()
		{
			var bill = header.Bills.AddNew();
			header.AMA_MessageStatus = "ACP";
			AssertEquals(true, statusProvider.AllowOriginalMessage(header));

			bill.ABL_BillStatus = "ACP";
			AssertEquals(false, statusProvider.AllowOriginalMessage(header));

			bill.ABL_BillStatus = "ERR";
			AssertEquals(true, statusProvider.AllowOriginalMessage(header));
		}

		public override void TestHasManifestBeenAcceptedByCustoms()
		{
			AssertEquals(false, statusProvider.HasManifestBeenAcceptedByCustoms(header));
		}

		public override void TestHasManifestBeenSubmittedToCustoms()
		{
			header.AMA_MessageStatus = "";
			AssertEquals(false, statusProvider.HasManifestBeenSubmittedToCustoms(header));
			header.AMA_MessageStatus = "ACP";
			AssertEquals(true, statusProvider.HasManifestBeenSubmittedToCustoms(header));
			header.AMA_MessageStatus = "ERR";
			AssertEquals(true, statusProvider.HasManifestBeenSubmittedToCustoms(header));
		}

		public override void TestMessageStatusCanBeReset()
		{
			AssertEquals(false, statusProvider.MessageStatusCanBeReset(header));
		}
		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Uruguay;
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
