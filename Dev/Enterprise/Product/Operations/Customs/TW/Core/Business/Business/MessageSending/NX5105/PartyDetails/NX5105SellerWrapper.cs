using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	class NX5105SellerWrapper : NX5105PartyDetailsWrapper
	{
		public NX5105SellerWrapper(OrgAddress orgAddress, ZString customsControlID, TWJobDocAddress jobDocAddress, params string[] codesToLookFor)
			: base(orgAddress, orgAddress, codesToLookFor)
		{
			this.customsControlID = customsControlID;
			supplierDocumentaryAddress = Argument.NotNull(jobDocAddress, nameof(jobDocAddress));
		}

		readonly TWJobDocAddress supplierDocumentaryAddress;
		readonly ZString customsControlID;

		protected override ZString IDCore
		{
			get
			{
				ZString result = ZString.Empty;
				var countryCode = supplierDocumentaryAddress.E2_RN_NKCountryCode;
				if (countryCode == Core.Constants.CountryCodes.Taiwan)
				{
					result = SharedHelper.GetIDStartWithNO(supplierDocumentaryAddress.IDCode, TypeCodeCore);
				}
				else
				{
					var name = NameCore;
					if (name != MessageConstants.NoEnglishName)
					{
						result = SharedHelper.GetLetterFromEnglishName(name);
						if (countryCode == Core.Constants.CountryCodes.UnitedStates && result.Length > 0)
						{
							result = FormattableString.Invariant($"{result,-6}{supplierDocumentaryAddress.E2_State.SubstringSafe(0, 2)}");
						}
					}
				}
				return result;
			}
		}

		internal override ZBool ShouldAllowShowNoEnglishName => true;

		protected override ZString CustomsControlIDCore => customsControlID;

		protected override ZString TypeCodeCore => supplierDocumentaryAddress.TypeCode;

		protected override ZString Communications1IdCore => supplierDocumentaryAddress.E2_Phone;

		protected override ZString ContactNameCore => supplierDocumentaryAddress.E2_Contact;

		protected override ZString LPCOAuthorizedPartyIDCore => FormatAEONumber(supplierDocumentaryAddress.AEOCode);

		protected override AddressData GetEnglishAddress()
		{
			return new AddressData(supplierDocumentaryAddress, SharedHelper.GetEnglishLanguageCodes());
		}

		protected override AddressData GetChineseTraditionalAddress()
		{
			if (supplierDocumentaryAddress.E2_AddressOverride)
			{
				return new AddressData(supplierDocumentaryAddress.LocalAddress, Core.SharedConstants.Languages.ChineseTraditional);
			}
			else
			{
				return base.GetChineseTraditionalAddress();
			}
		}

		protected override ZString FormatAEONumber(ZString number)
		{
			var result = ZString.Empty;
			if (!number.IsEmpty)
			{
				var countryCode = ((IPartyDetails)this).Address.CountryCode;
				var suffixLetters = SharedHelper.GetSuffixLetters(countryCode);
				result = FormattableString.Invariant($"{suffixLetters}{number}");
			}
			return result;
		}
	}
}
