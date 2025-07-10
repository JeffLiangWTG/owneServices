using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class LicensingMessageManufacturer : PartyDetailsWrapper
	{
		protected TWJobDocAddress ManufacturerAddress { get; }

		public LicensingMessageManufacturer(TWJobDocAddress localManufacturer)
			: base(localManufacturer.Address)
		{
			ManufacturerAddress = Argument.NotNull(localManufacturer, nameof(localManufacturer));
		}

		protected override ZString IDCore => ManufacturerAddress.IDCode;

		protected override ZString NameCore => ManufacturerAddress.CompanyEnglishName;

		protected override ZString ChineseNameCore => ManufacturerAddress.CompanyChineseName;

		protected override AddressData GetEnglishAddress() => new AddressData(ManufacturerAddress, SharedHelper.GetEnglishLanguageCodes());

		protected override AddressData GetChineseTraditionalAddress() => ManufacturerAddress.E2_AddressOverride
			? new AddressData(ManufacturerAddress.LocalAddress, Core.SharedConstants.Languages.ChineseTraditional)
			: base.GetChineseTraditionalAddress();

		protected override IEnumerable<ICommunication> CommunicationsCore
		{
			get
			{
				var phone = ManufacturerAddress.E2_Phone;
				if (!phone.IsEmpty)
				{
					yield return new CommunicationWrapper(phone, MessageConstants.CommunicationTypeIDs.TE);
				}
				var email = ManufacturerAddress.E2_Email;
				if (!email.IsEmpty)
				{
					yield return new CommunicationWrapper(email, MessageConstants.CommunicationTypeIDs.MA);
				}
				var fax = ManufacturerAddress.E2_Fax;
				if (!fax.IsEmpty)
				{
					yield return new CommunicationWrapper(fax, MessageConstants.CommunicationTypeIDs.FX);
				}
			}
		}

		protected override ZString ContactNameCore => ManufacturerAddress.E2_Contact;
	}
}
