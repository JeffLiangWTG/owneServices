using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	internal class DocAddressCustomsRegistrationNumberValidation : CustomsRegistrationNumberValidation
	{
		internal DocAddressCustomsRegistrationNumberValidation(BusinessObject master)
			: base(master)
		{ }

		new TWJobDocAddress Master => (TWJobDocAddress)base.Master;

		ZBool IsExport => Master.IsExport;

		ZBool IsImport => Master.IsImport;

		protected override INotificationType ErrorType => CargoWise.EntityFramework.NotificationType.MessageError;

		protected override INotificationType WarningOrMessageError => CargoWise.EntityFramework.NotificationType.MessageError;

		internal override void CheckAEOCustomsRegistrationNumber(ZPropertyInfo customsRegNoInfo, ZString number, ZString registrationNumberCountryCode)
		{
			base.CheckAEOCustomsRegistrationNumber(customsRegNoInfo, number, registrationNumberCountryCode);

			if (!number.IsEmpty)
			{
				if (registrationNumberCountryCode == Core.Constants.CountryCodes.Taiwan
					|| (IsExport && Master.IsImporterDocumentaryAddress()) || (IsImport && Master.IsSupplierDocumentaryAddress()))
				{
					var maxNumberLength = GetMaxAEONumberCharacters(registrationNumberCountryCode);
					if (maxNumberLength > 0 && maxNumberLength < number.Length)
					{
						customsRegNoInfo.AddWarning(ValidationConstants.TWJobDocAddressValidationMessages.MaxCharactersLength(maxNumberLength));
					}
				}
			}
		}

		ZInt GetMaxAEONumberCharacters(ZString countryCode)
		{
			switch (countryCode)
			{
				case Core.Constants.CountryCodes.Taiwan:
					return 14;
				case Core.Constants.CountryCodes.Singapore:
				case Core.Constants.CountryCodes.Israel:
				case Core.Constants.CountryCodes.China:
				case Core.Constants.CountryCodes.KoreaSouth:
					return 15;
				case Core.Constants.CountryCodes.Australia:
				case Core.Constants.CountryCodes.India:
					return 18;
				case Core.Constants.CountryCodes.Japan:
					return 20;
				default:
					return 0;
			}
		}
	}
}
