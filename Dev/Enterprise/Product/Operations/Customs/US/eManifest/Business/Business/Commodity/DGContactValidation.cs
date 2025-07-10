using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.eManifest.Business
{
	class DGContactValidation : RelatedObjectValidation
	{
		internal DGContactValidation(Commodity parent, UNDGDataItem undg)
			: base(undg.DI_OC_DGContactInfo)
		{
			this.parent = parent;
			this.undg = undg;
		}

		internal void HookValidation()
		{
			notificationInfo.AdditionalValidation += Validate;
			notificationInfo.AdditionalValidation += ValidateBY_HazardousGoodsContact;
			undg.DI_DGInfo.AdditionalValidation += ValidateBY_HazardousGoodsIdentifier;
		}

		internal void UnHookValidation()
		{
			notificationInfo.AdditionalValidation -= Validate;
			notificationInfo.AdditionalValidation -= ValidateBY_HazardousGoodsContact;
			undg.DI_DGInfo.AdditionalValidation -= ValidateBY_HazardousGoodsIdentifier;
		}

		void Validate()
		{
			MandatoryValidation.MessageErrorIfNotEntered(notificationInfo);
			if (!notificationInfo.HasNotifications())
			{
				var contact = undg.DGContact;
				var errorBuilder = new ErrorStringBuilder(notificationInfo, "UNDG Contact");
				MessageErrorIfNotEntered(contact.OC_ContactNameInfo, errorBuilder);
				MessageErrorIfNotEntered(contact.OC_PhoneInfo, errorBuilder, "Work Phone");
				errorBuilder.FillErrorMessages();
			}
		}

		void ValidateBY_HazardousGoodsIdentifier()
		{
			parent.Validation.ValidateBY_HazardousGoodsIdentifier();
		}

		void ValidateBY_HazardousGoodsContact()
		{
			parent.Validation.ValidateBY_HazardousGoodsContact();
		}

		readonly Commodity parent;
		readonly UNDGDataItem undg;
	}
}
