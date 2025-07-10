using System;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.Manifest.Business.Testing
{
	public partial class AsycudaManifestHeaderTest
	{
		public void TestGetMessageSendingNotification_NotExists()
		{
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Turkey, TRManifestTypes.Codes.ATAIHR);
			var message = header.MessageSendingNotificationHelper.GetNotifications();
			AssertNotContains(Business.ValidationConstants.MustBeLoggedInUnderTRToSendTRMessages, message);
		}

		public void TestGetMessageSendingNotification_Exists()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Eritrea))
			{
				var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Turkey, TRManifestTypes.Codes.ATAIHR);
				var message = header.MessageSendingNotificationHelper.GetNotifications();
				AssertContains(Business.ValidationConstants.PleaseSupplyAValueTRToSendTRMessages, "Please supply a value");
			}
		}

		protected override Type ExpectedTypeOfContainer => typeof(AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>);
	}
}
