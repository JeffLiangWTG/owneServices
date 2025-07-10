using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.TW.Business;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business
{
	public class ManifestRegistrationNumberValidation : CustomsRegistrationNumberValidation
	{
		public ManifestRegistrationNumberValidation(BusinessObject master)
			: base(master)
		{
		}

		protected override INotificationType ErrorType => CargoWise.EntityFramework.NotificationType.MessageError;

		protected override INotificationType WarningOrMessageError => CargoWise.EntityFramework.NotificationType.MessageError;
	}
}
