namespace Enterprise.Customs.UY.Manifest.Business.Testing
{
	public static class DAETestingConstants
	{
		#region Directories

		public const string TestFilesDirectory = @"Enterprise\Product\Operations\Customs\UY\Manifest\Business.Test\Message\DAE\TestFiles";

		public const string TestFilesResponseDirectory = TestFilesDirectory + @"\DAEResponse";

		#endregion

		#region Files

		public const string BodyAmendPacks = TestFilesResponseDirectory + @"\BodyAmendPacks.xml";

		public const string BodyText = TestFilesResponseDirectory + @"\BodyText.xml";

		public const string BodyTextCredentialsError = TestFilesResponseDirectory + @"\BodyTextCredentialsError.xml";

		public const string BodyTextError = TestFilesResponseDirectory + @"\BodyTextError.xml";

		public const string BodyTextWithOutSign = TestFilesResponseDirectory + @"\BodyTextWithOutSign.xml";

		public const string HeaderText = TestFilesResponseDirectory + @"\HeaderText.xml";

		public const string HeaderTextErrorNotification = TestFilesResponseDirectory + @"\HeaderTextErrorNotification.txt";

		public const string HeaderTextErrorNotificationTwo = TestFilesResponseDirectory + @"\HeaderTextErrorNotification2.txt";

		public const string BodyTextErrorNotification = TestFilesResponseDirectory + @"\BodyTextErrorNotification.txt";

		public const string OneBillAcceptedOneBillRejected = TestFilesResponseDirectory + @"\OneBillAcceptedOneBillRejected.xml";

		public const string OneBillAcceptedOneBillRejectedResend = TestFilesResponseDirectory + @"\OneBillAcceptedOneBillRejectedResend.xml";

		public const string SuccessfulAmendment = TestFilesResponseDirectory + @"\SuccessfulAmendment.xml";

		public const string SuccessfulCancellation = TestFilesResponseDirectory + @"\SuccessfulCancellation.xml";

		public const string SampleWithEnvelope = TestFilesResponseDirectory + @"\SampleWithEnvelope.xml";

		public const string SampleWithOutEnvelope = TestFilesResponseDirectory + @"\SampleWithOutEnvelope.xml";

		public const string DAEManifestWithOutSign = TestFilesDirectory + @"\DAEManifestWithOutSign.txt";

		#endregion
	}
}
