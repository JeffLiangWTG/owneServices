using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business
{
	public class OnBoardCourier : IPartyDetails
	{
		public OnBoardCourier(GlbPerson person)
		{
			this.person = Argument.NotNull(person, nameof(person));
			licenseNumber = person.PER_DriversLicenseNumber;
			TypeCode = licenseNumber.IsEmpty ? PartyIdentifierCodeList.Codes._53 : PartyIdentifierCodeList.Codes._174;
		}

		readonly GlbPerson person;
		readonly ZString licenseNumber;

		public ZString ID => TypeCode == PartyIdentifierCodeList.Codes._174 ? licenseNumber : person.PER_Passport;

		public ZString Name => person.PER_PreferredLanguage == Core.SharedConstants.Languages.English ? person.PER_FullName : ZString.Empty;

		public ZString ChineseName => person.PER_PreferredLanguage == Core.SharedConstants.Languages.ChineseTraditional ? person.PER_FullName : ZString.Empty;

		public ZString TypeCode { get; }

		public ZString CustomsControlID => null;

		public ZString PaymentOnAccountBusinessID => null;

		public ZString RoleCode => null;

		public ZString SubBoxID => null;

		public IAddress Address => null;

		public ILPCOAuthorizedParty LPCOAuthorizedParty => null;

		public IEnumerable<ICommunication> Communications => null;

		public ZString ContactName => null;

		public ZString OwnerName => null;

		public ZString MainManufacturer => null;

		public ZString UndertakeCode => null;

		public IEnumerable<IAdditionalInformation> AdditionalInformations => null;
	}
}
