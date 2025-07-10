using CargoWise.Common;
using CargoWise.Customs.TR.MessageContracts.Interfaces.Declaration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.TR.Business.CusEntryMessageConstants;

namespace Enterprise.Customs.TR.Business
{
	public class TraderProvider : ITrader
	{
		public TraderProvider(OrgAddress orgAddress, ZString addressType, ZBool isExport, ZString yfkNo, ZString procedureCode, ZDateTime effectiveDate)
		{
			TraderOrgAddress = Argument.NotNull(orgAddress, nameof(orgAddress));
			TraderOrgHeader = TraderOrgAddress.Header;
			AddressType = Argument.NotNull(addressType, nameof(addressType));
			IsExport = isExport;
			YfkNo = yfkNo;
			ProcedureCode = procedureCode;
			EffectiveDate = effectiveDate;
		}
		OrgAddress TraderOrgAddress { get; }
		OrgHeader TraderOrgHeader { get; }
		ZString AddressType { get; }
		ZBool IsExport { get; }
		ZString YfkNo { get; }
		ZString ProcedureCode { get; }
		ZDateTime EffectiveDate { get; }

		static class Constants
		{
			public const string CompanyIdentityType = "1";
			public const string SuffixOfYkfs = "/YKFS";

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Localised strings")]
			public static class TraderTypes
			{
				public const string Buyer = "Alici";
				public const string Sender = "Gonderici";
				public const string OtherBuyer = "DigerAlici";
				public const string OtherSender = "DigerGonderici";
			}
		}

		public string Type
		{
			get
			{
				switch (AddressType)
				{
					case DocAddressTypes.Codes.SupplierDocumentaryAddress:
						return Constants.TraderTypes.Sender;
					case DocAddressTypes.Codes.ImporterDocumentaryAddress:
						return Constants.TraderTypes.Buyer;
					case DocAddressTypes.Codes.SellerDocumentaryAddress:
						return Constants.TraderTypes.OtherSender;
					case DocAddressTypes.Codes.BuyerDocumentaryAddress:
						return Constants.TraderTypes.OtherBuyer;
					default:
						return string.Empty;
				}
			}
		}

		public string CountryCode => YfkNo.IsEmpty ? UniversalReferenceDataHelper.MapCW1CountryCodeToCustomsCode(TraderOrgHeader.Factory, TraderOrgAddress.Country.Code, EffectiveDate) : ZString.Empty;
		public string PostalCode => TraderOrgAddress.Postcode;
		public string IdentityType => Constants.CompanyIdentityType;
		public string Fax => TraderOrgAddress.FaxNumber.ToString();

		public string Number
		{
			get
			{
				var idNumber = ZString.Empty;
				if (!IsExport)
				{
					if (AddressType == DocAddressTypes.Codes.BuyerDocumentaryAddress)
					{
						idNumber = TraderOrgHeader.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.Turkey);
					}
					else if (!YfkNo.IsEmpty && (AddressType == DocAddressTypes.Codes.SupplierDocumentaryAddress || AddressType == DocAddressTypes.Codes.SellerDocumentaryAddress))
					{
						idNumber = YfkNo + Constants.SuffixOfYkfs;
					}
				}
				else
				{
					if (AddressType == DocAddressTypes.Codes.SellerDocumentaryAddress)
					{
						idNumber = TraderOrgHeader.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.Turkey);
					}
					else if (ProcedureCode == ProcedureCodes._7200 && AddressType == DocAddressTypes.Codes.ImporterDocumentaryAddress && !YfkNo.IsEmpty)
					{
						idNumber = YfkNo + Constants.SuffixOfYkfs;
					}
				}

				return idNumber;
			}
		}

		public string NameAndTitle => YfkNo.IsEmpty ? TraderOrgHeader.OH_FullName : ZString.Empty;
		public string StreetNo => TraderOrgAddress != null ? (TraderOrgAddress.Address1.TrimEnd() + " " + TraderOrgAddress.Address2.TrimStart()).Trim() : string.Empty;
		public string Telephone => TraderOrgAddress.PhoneNumber.ToString();
		public string ProvinceAndDistrict => TraderOrgAddress.City;
	}
}
