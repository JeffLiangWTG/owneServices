using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class LicensingMessageApplicationApplicant : IPartyDetails
	{
		protected TWJobDocAddress Applicant { get; }

		protected IReservedFieldSupporter ReservedFieldSupporter { get; }

		public LicensingMessageApplicationApplicant(TWJobDocAddress applicant, IReservedFieldSupporter reservedFieldSupporter)
		{
			Applicant = Argument.NotNull(applicant, nameof(applicant));
			ReservedFieldSupporter = reservedFieldSupporter;
		}

		public ZString ID => Applicant.IDCode;

		public ZString Name => Applicant.CompanyEnglishName;

		public ZString ChineseName => Applicant.CompanyChineseName;

		public ZString MainManufacturer => ZString.Empty;

		public ZString TypeCode => Applicant.TypeCode;

		public IAddress Address => new AddressWrapper(chineseLine: new AddressData(Applicant.E2_AddressOverride ? Applicant.LocalAddress : Applicant, Core.SharedConstants.Languages.ChineseTraditional).ChineseTraditionalAddressFormat, line: new AddressData(Applicant, Core.SharedConstants.Languages.English).EnglishAddressFormat);

		public IEnumerable<ICommunication> Communications
		{
			get
			{
				var phone = Applicant.E2_Phone;
				if (!phone.IsEmpty)
				{
					yield return new CommunicationWrapper(phone, MessageConstants.CommunicationTypeIDs.TE);
				}

				var email = Applicant.E2_Email;
				if (!email.IsEmpty)
				{
					yield return new CommunicationWrapper(email, MessageConstants.CommunicationTypeIDs.MA);
				}
			}
		}

		public ZString CustomsControlID => ZString.Empty;

		public ZString PaymentOnAccountBusinessID => ZString.Empty;

		public ZString RoleCode => ZString.Empty;

		public ZString SubBoxID => ZString.Empty;

		public ILPCOAuthorizedParty LPCOAuthorizedParty => null;

		public ZString ContactName => Applicant.E2_Contact;

		public ZString OwnerName => ZString.Empty;

		public ZString UndertakeCode => Customs.Business.YesNoList.Codes.Yes;

		public IEnumerable<IAdditionalInformation> AdditionalInformations => ReservedFieldSupporter?.GetAdditionalInformations();
	}
}
