using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	class Buyer : PartyDetails
	{
		public Buyer(JobDeclaration declaration, OrgHeader orgHeader, TWJobDocAddress jobDocAddress)
			: base(declaration, orgHeader, jobDocAddress?.Address ?? orgHeader.MainAddress)
		{
			Argument.NotNull(jobDocAddress, nameof(jobDocAddress));
			importerDocumentaryAddress = Argument.NotNull(jobDocAddress, nameof(jobDocAddress));
		}

		readonly TWJobDocAddress importerDocumentaryAddress;

		protected override ZString CustomsControlIDCore => importerDocumentaryAddress.CBPCode;

		internal override ZBool ShouldAllowShowNoEnglishName => true;

		protected override AddressData GetEnglishAddress()
		{
			return new BuyerAddressData(orgHeader, importerDocumentaryAddress, SharedHelper.GetEnglishLanguageCodes());
		}

		protected override AddressData GetChineseTraditionalAddress()
		{
			var jobDocAddress = importerDocumentaryAddress.E2_AddressOverride ? importerDocumentaryAddress.LocalAddress : importerDocumentaryAddress;
			return new BuyerAddressData(orgHeader, jobDocAddress, Core.SharedConstants.Languages.ChineseTraditional);
		}

		protected override ZString LPCOAuthorizedPartyIDCore => FormatAEONumber(importerDocumentaryAddress.AEOCode);

		protected override ZString TypeCodeCore => importerDocumentaryAddress.TypeCode;

		protected override ZString IDCore
		{
			get
			{
				ZString result = ZString.Empty;
				var countryCode = importerDocumentaryAddress.E2_RN_NKCountryCode;
				if (countryCode == Core.Constants.CountryCodes.Taiwan)
				{
					result = SharedHelper.GetIDStartWithNO(importerDocumentaryAddress.IDCode, TypeCodeCore);
				}
				else
				{
					var name = NameCore;
					if (name != MessageConstants.NoEnglishName)
					{
						result = SharedHelper.GetLetterFromEnglishName(name);
						if (countryCode == Core.Constants.CountryCodes.UnitedStates && result.Length > 0)
						{
							result = FormattableString.Invariant($"{result,-6}{importerDocumentaryAddress.E2_State.SubstringSafe(0, 2)}");
						}
					}
				}
				return result;
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
