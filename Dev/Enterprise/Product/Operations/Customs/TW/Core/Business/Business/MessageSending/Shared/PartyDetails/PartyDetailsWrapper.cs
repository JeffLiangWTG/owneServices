using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	public class PartyDetailsWrapper : DocumentWrapper, IPartyDetails
	{
		protected readonly OrgAddress orgAddress;

		protected readonly OrgHeader orgHeader;

		public PartyDetailsWrapper(OrgAddress orgAddress) : this(orgAddress, orgAddress?.Header)
		{
		}

		public PartyDetailsWrapper(OrgAddress orgAddress, OrgHeader orgHeader)
		{
			this.orgHeader = orgHeader;
			this.orgAddress = orgAddress;
		}

		public PartyDetailsWrapper(ZString id, ZString roleCode, ZString subBoxId, OrgAddress orgAddress)
			: this(id, roleCode, subBoxId, ZString.Empty, orgAddress)
		{
		}

		public PartyDetailsWrapper(ZString id, ZString roleCode, ZString subBoxId, ZString typeCode, OrgAddress orgAddress)
			: this(orgAddress)
		{
			IDCore = id;
			RoleCodeCore = roleCode;
			SubBoxIDCore = subBoxId;
			TypeCodeCore = typeCode;
			LPCOAuthorizedPartyIDCore = GetAEONumber();
		}

		internal virtual ZBool ShouldAllowShowNoEnglishName => ZBool.False;

		protected AddressData EnglishAddress => fEnglishAddress ?? (fEnglishAddress = GetEnglishAddress());
		AddressData fEnglishAddress;

		protected virtual AddressData GetEnglishAddress() => orgAddress != null ? new AddressData(orgAddress, false, false, SharedHelper.GetEnglishLanguageCodes()) : null;

		protected AddressData ChineseTraditionalAddress => fChineseTraditionalAddress ?? (fChineseTraditionalAddress = GetChineseTraditionalAddress());
		AddressData fChineseTraditionalAddress;

		protected virtual AddressData GetChineseTraditionalAddress() => orgAddress != null ? new AddressData(orgAddress, false, false, Core.SharedConstants.Languages.ChineseTraditional) : null;

		protected virtual ZString IDCore { get; }
		ZString IPartyDetails.ID => IDCore;

		protected virtual ZString NameCore => EnglishAddress?.CompanyName ?? ZString.Empty;
		ZString IPartyDetails.Name => NameCore.IsEmpty && ShouldAllowShowNoEnglishName ? (ZString)MessageConstants.NoEnglishName : NameCore;

		protected virtual ZString ChineseNameCore => ChineseTraditionalAddress?.CompanyName ?? ZString.Empty;
		ZString IPartyDetails.ChineseName => ChineseNameCore;

		protected virtual ZString TypeCodeCore { get; }
		ZString IPartyDetails.TypeCode => TypeCodeCore;

		protected virtual ZString CustomsControlIDCore { get; }
		ZString IPartyDetails.CustomsControlID => CustomsControlIDCore;

		protected virtual ZString PaymentOnAccountBusinessIDCore { get; }
		ZString IPartyDetails.PaymentOnAccountBusinessID => PaymentOnAccountBusinessIDCore;

		protected virtual ZString RoleCodeCore { get; }
		ZString IPartyDetails.RoleCode => RoleCodeCore;

		protected virtual ZString SubBoxIDCore { get; }
		ZString IPartyDetails.SubBoxID => SubBoxIDCore;

		protected virtual IAddress AddressCore => new AddressWrapper(EnglishAddress?.EnglishAddressFormat ?? ZString.Empty, ChineseTraditionalAddress?.ChineseTraditionalAddressFormat ?? ZString.Empty, EnglishAddress?.CountryCode ?? ZString.Empty, GetAddressCountrySubDivisionIDCore(), GetAddressCountrySubDivisionNameCore());
		IAddress IPartyDetails.Address => AddressCore;

		protected virtual ZString GetAddressCountrySubDivisionIDCore() => EnglishAddress?.StateCode ?? ZString.Empty;

		protected virtual ZString GetAddressCountrySubDivisionNameCore() => EnglishAddress?.StateDescription ?? ZString.Empty;

		protected virtual ZString LPCOAuthorizedPartyIDCore { get; }
		ILPCOAuthorizedParty IPartyDetails.LPCOAuthorizedParty
		{
			get
			{
				if (!LPCOAuthorizedPartyIDCore.IsEmpty)
				{
					return new LPCOAuthorizedPartyWrapper(LPCOAuthorizedPartyIDCore);
				}
				return null;
			}
		}

		protected virtual IEnumerable<ICommunication> CommunicationsCore { get; }
		IEnumerable<ICommunication> IPartyDetails.Communications => CommunicationsCore;

		protected virtual ZString ContactNameCore { get; }
		ZString IPartyDetails.ContactName => ContactNameCore;

		protected virtual ZString MainManufacturerCore { get; }
		ZString IPartyDetails.MainManufacturer => MainManufacturerCore;

		protected virtual ZString UndertakeCodeCore { get; }
		ZString IPartyDetails.UndertakeCode => UndertakeCodeCore;

		public ZString OwnerName => null;

		public IEnumerable<IAdditionalInformation> AdditionalInformations => null;

		protected ZString GetAEONumber() => FormatAEONumber(orgHeader.GetCustomsRegNo(OrgCusCode.TaiwanCodeTypes.AEO));

		protected virtual ZString FormatAEONumber(ZString number)
		{
			if (!number.IsEmpty)
			{
				number = FormattableString.Invariant($"{MessageConstants.LPCOAuthorizedParty.TWAEO}-{number}");
			}
			return number;
		}

		protected ZString GetTypeCodeBasedOnOrgCusCode(ZString type)
		{
			var result = ZString.Empty;
			switch (type)
			{
				case OrgCusCode.CodeTypes.VATCode:
				case Constants.CCPPrefix:
					result = PartyIdentifierCodeList.Codes._58;
					break;
				case OrgCusCode.CodeTypes.PassportID:
					result = PartyIdentifierCodeList.Codes._53;
					break;
				case OrgCusCode.TaiwanCodeTypes.PID:
					result = PartyIdentifierCodeList.Codes._174;
					break;
			}
			return result;
		}

		protected static ZString GetCustomsRegNo(OrgCusCodeCollection codes, string codeType)
		{
			return codes?.GetCustomsRegNo(codeType, Core.Constants.CountryCodes.Taiwan) ?? ZString.Empty;
		}
	}
}
