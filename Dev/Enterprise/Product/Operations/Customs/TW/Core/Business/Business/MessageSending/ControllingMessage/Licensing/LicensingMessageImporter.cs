using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class LicensingMessageImporter : PartyDetailsWrapper
	{
		TWJobDocAddress ImporterDocumentaryAddress { get; }

		public LicensingMessageImporter(TWJobDocAddress importerDocumentaryAddress)
			: base(importerDocumentaryAddress.Address)
		{
			ImporterDocumentaryAddress = Argument.NotNull(importerDocumentaryAddress, nameof(importerDocumentaryAddress));
		}

		protected override ZString IDCore => ImporterDocumentaryAddress.IDCode;

		protected override ZString NameCore => ImporterDocumentaryAddress.CompanyEnglishName;

		protected override ZString ChineseNameCore => ImporterDocumentaryAddress.CompanyChineseName;

		protected override ZString TypeCodeCore => ImporterDocumentaryAddress.TypeCode;

		protected override AddressData GetEnglishAddress() => new AddressData(ImporterDocumentaryAddress, SharedHelper.GetEnglishLanguageCodes());

		protected override AddressData GetChineseTraditionalAddress() => ImporterDocumentaryAddress.E2_AddressOverride
			? new AddressData(ImporterDocumentaryAddress.LocalAddress, Core.SharedConstants.Languages.ChineseTraditional)
			: base.GetChineseTraditionalAddress();

		protected override IEnumerable<ICommunication> CommunicationsCore
		{
			get
			{
				var phone = ImporterDocumentaryAddress.E2_Phone;
				if (!phone.IsEmpty)
				{
					yield return new CommunicationWrapper(phone, MessageConstants.CommunicationTypeIDs.TE);
				}
				var email = ImporterDocumentaryAddress.E2_Email;
				if (!email.IsEmpty)
				{
					yield return new CommunicationWrapper(email, MessageConstants.CommunicationTypeIDs.MA);
				}
			}
		}
	}
}
