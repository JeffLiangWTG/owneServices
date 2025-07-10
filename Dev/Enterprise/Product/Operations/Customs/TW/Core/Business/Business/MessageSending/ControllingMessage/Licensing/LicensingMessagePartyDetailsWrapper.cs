using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class LicensingMessagePartyDetailsWrapper : PartyDetailsWrapper
	{
		protected TWJobDocAddress JobDocumentaryAddress { get; }

		protected readonly bool isCertificate15;

		public LicensingMessagePartyDetailsWrapper(TWJobDocAddress jobDocumentaryAddress, bool isCertificate15 = false)
			: base(jobDocumentaryAddress.Address)
		{
			JobDocumentaryAddress = Argument.NotNull(jobDocumentaryAddress, nameof(jobDocumentaryAddress));
			this.isCertificate15 = isCertificate15;
		}

		protected override ZString IDCore => JobDocumentaryAddress.IDCode;

		protected override ZString NameCore => JobDocumentaryAddress.CompanyEnglishName;

		protected override ZString ChineseNameCore => JobDocumentaryAddress.CompanyChineseName;

		protected override ZString TypeCodeCore => JobDocumentaryAddress.TypeCode;

		protected override AddressData GetEnglishAddress() => new LicensingMessageAddressData(JobDocumentaryAddress, isCertificate15, SharedHelper.GetEnglishLanguageCodes());

		protected override AddressData GetChineseTraditionalAddress() => new LicensingMessageAddressData(JobDocumentaryAddress.E2_AddressOverride ? JobDocumentaryAddress.LocalAddress : JobDocumentaryAddress, isCertificate15, Core.SharedConstants.Languages.ChineseTraditional);

		protected override IEnumerable<ICommunication> CommunicationsCore
		{
			get
			{
				var phone = JobDocumentaryAddress.E2_Phone;
				if (!phone.IsEmpty)
				{
					yield return new CommunicationWrapper(phone, MessageConstants.CommunicationTypeIDs.TE);
				}
				var email = JobDocumentaryAddress.E2_Email;
				if (!email.IsEmpty)
				{
					yield return new CommunicationWrapper(email, MessageConstants.CommunicationTypeIDs.MA);
				}
				var fax = JobDocumentaryAddress.E2_Fax;
				if (!fax.IsEmpty)
				{
					yield return new CommunicationWrapper(fax, MessageConstants.CommunicationTypeIDs.FX);
				}
			}
		}

		protected override ZString ContactNameCore => JobDocumentaryAddress.E2_Contact;
	}
}
