using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.TW.Business;
using NotificationType = CargoWise.EntityFramework.NotificationType;

namespace Enterprise.Customs.TW.Manifest.Business
{
	internal class TWManifestRegistrationNumberValidation : CustomsRegistrationNumberValidation
	{
		public TWManifestRegistrationNumberValidation(BusinessObject master)
			: base(master)
		{
		}

		protected override INotificationType ErrorType => NotificationType.MessageError;

		protected override INotificationType WarningOrMessageError => NotificationType.MessageError;
	}
}
