
//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.UY.Manifest.Business
{
	public static class ValidationsConstants
	{
		public static string MustBeLoggedInUnderUYToSendUYMessages
		{
			get { return ResString.GetMultilingualString("2547C549-8F82-4DD7-85B2-01350CD0ED67", "To create a message for Uruguay you must be logged-in under a Uruguayan company."); }
		}

		public static string CompanyCredentialMustBeUpdatedToSendUYMessages
		{
			get { return ResString.GetMultilingualString("05EE868C-794F-46C5-B45D-1CA6072FE9D5", "It is necessary to review and update the certificate information in the Brokerage tab on the Company form before to send a new Manifest."); }
		}
	}
}
