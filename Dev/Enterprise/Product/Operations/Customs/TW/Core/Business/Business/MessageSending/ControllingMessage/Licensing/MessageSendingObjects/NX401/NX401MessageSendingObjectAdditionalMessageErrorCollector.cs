using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class NX401MessageSendingObjectAdditionalMessageErrorCollector : LicensingMessageSendingObjectAdditionalMessageErrorCollector
	{
		public NX401MessageSendingObjectAdditionalMessageErrorCollector(LicensingMessageSendingObjectParent parent)
			: base(parent)
		{
		}

		protected override List<INotification> GetDeclarationNotificationCore(JobDeclaration declaration)
		{
			var notifications = base.GetDeclarationNotificationCore(declaration);
			if (declaration.IsExport && declaration.JE_OH_Consignee.IsEmpty)
			{
				notifications.Add(GetNotification(ConsigneeHumanReadableName, MandatoryValidation.YouHaveNotEnteredMessage((NoResString)"Consignee")));
			}
			if (declaration.IsImport && declaration.IntermConsignee != null)
			{
				var consignee = declaration.IntermConsignee;
				var localAddress = consignee.MainAddress.TranslatedAddresses.FirstOrDefault((OrgTranslatedAddress a) => a.OTA_Language == Core.SharedConstants.Languages.ChineseTraditional);
				if (localAddress == null || localAddress.CompanyName.IsEmpty)
				{
					notifications.Add(GetNotification(ConsigneeHumanReadableName, MandatoryValidation.YouHaveNotEnteredMessage((NoResString)"Consignee Local Company Name")));
				}
				if (localAddress == null || localAddress.Address1.IsEmpty)
				{
					notifications.Add(GetNotification(ConsigneeHumanReadableName, MandatoryValidation.YouHaveNotEnteredMessage((NoResString)"Consignee Local Address")));
				}
			}
			var declarantAddress = declaration.DeclarantAddress;
			if (declarantAddress != null && !declarantAddress.HasCompanyNameOfLanguage(Enterprise.Core.SharedConstants.Languages.ChineseTraditional))
			{
				notifications.Add(GetNotification(DeclarantHumanReadableName, MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.Declaration.Declarant.LocalCompanyNameResString)));
			}
			return notifications;
		}
	}
}
