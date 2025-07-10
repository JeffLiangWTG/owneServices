using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Business
{
	public class NX201_07MessageSendingObjectAdditionalMessageErrorCollector : LicensingMessageSendingObjectAdditionalMessageErrorCollector
	{
		public NX201_07MessageSendingObjectAdditionalMessageErrorCollector(LicensingMessageSendingObjectParent parent)
			: base(parent)
		{
		}

		protected override List<INotification> GetDeclarationNotificationCore(JobDeclaration declaration)
		{
			var notifications = base.GetDeclarationNotificationCore(declaration);
			var declarantAddress = declaration.DeclarantAddress;
			if (declarantAddress != null && declarantAddress.Language != Enterprise.Core.SharedConstants.Languages.ChineseTraditional)
			{
				var chineseTranslatedAddress = declarantAddress.GetTranslatedAddressInSpecificLanguage(Enterprise.Core.SharedConstants.Languages.ChineseTraditional);
				if (chineseTranslatedAddress == null || chineseTranslatedAddress.CompanyName.IsEmpty)
				{
					notifications.Add(GetNotification(DeclarantHumanReadableName, MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.Declaration.Declarant.LocalCompanyNameResString)));
				}
				if (chineseTranslatedAddress == null || chineseTranslatedAddress.Address1.IsEmpty)
				{
					notifications.Add(GetNotification(DeclarantHumanReadableName, MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.Declaration.Declarant.LocalAddressResString)));
				}
			}
			return notifications;
		}
	}
}
