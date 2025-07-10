using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.SADH
{
	public class SADHFormData : AutoSADHFormData
	{
		public SADHFormData(BusinessObjectFactory factory, BaseJobDeclaration declaration)
			: base(factory)
		{
			if (declaration == null)
			{
				throw new ArgumentNullException(nameof(declaration));
			}

			Declaration = declaration;
			CurrConverter = ((ICurrencyConverterProvider)declaration).CurrencyConverter;
		}
		internal readonly BaseJobDeclaration Declaration;

		internal CurrencyConverter CurrConverter
		{
			get;
			set;
		}

		#region public ZString ConsignorFormattedAddress
		public ZString ConsignorFormattedAddress
		{
			get { return FormatAddress(Consignor); }
		}

		public ZPropertyInfo ConsignorFormattedAddressInfo
		{
			get { return GetZPropertyInfo(nameof(ConsignorFormattedAddress)); }
		}
		#endregion

		#region public ZString ConsigneeFormattedAddress
		public ZString ConsigneeFormattedAddress
		{
			get { return FormatAddress(Consignee); }
		}

		public ZPropertyInfo ConsigneeFormattedAddressInfo
		{
			get { return GetZPropertyInfo(nameof(ConsigneeFormattedAddress)); }
		}
		#endregion

		#region public ZString CountryOfOriginFormatted
		public ZString CountryOfOriginFormatted
		{
			get
			{
				if (!D1_RL_NKCountryOfOrigin.IsEmpty)
				{
					RefCountry country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, D1_RL_NKCountryOfOrigin.Substring(0, 2));
					if (country != null)
					{
						return FormatCountryCodeWithDescription(country);
					}
				}
				return ZString.Empty;
			}
		}

		public ZPropertyInfo CountryOfOriginFormattedInfo
		{
			get { return GetZPropertyInfo(nameof(CountryOfOriginFormatted)); }
		}
		#endregion

		#region public ZString CountryOfDestinationFormatted
		public ZString CountryOfDestinationFormatted
		{
			get
			{
				if (!D1_RL_NKCountryOfDestination.IsEmpty)
				{
					RefCountry country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, D1_RL_NKCountryOfDestination.Substring(0, 2));
					if (country != null)
					{
						return FormatCountryCodeWithDescription(country);
					}
				}
				return ZString.Empty;
			}
		}

		public ZPropertyInfo CountryOfDestinationFormattedInfo
		{
			get { return GetZPropertyInfo(nameof(CountryOfDestinationFormatted)); }
		}
		#endregion

		public OrgHeader Consignor
		{
			get { return D1_OH_Consignor.IsValid ? Factory.Load<OrgHeader>(D1_OH_Consignor) : null; }
		}

		public OrgHeader Consignee
		{
			get { return D1_OH_Consignee.IsValid ? Factory.Load<OrgHeader>(D1_OH_Consignee) : null; }
		}

		#region public ZString DeclarantRepresentativeFormattedAddress
		public ZString DeclarantRepresentativeFormattedAddress
		{
			get { return FormatAddress(GlbCompany.CurrentCompany.OrgProxy); }
		}

		public ZPropertyInfo DeclarantRepresentativeFormattedAddressInfo
		{
			get { return GetZPropertyInfo(nameof(DeclarantRepresentativeFormattedAddress)); }
		}
		#endregion

		public override ZGuid D1_RX_InvoiceCurrency
		{
			get { return base.D1_RX_InvoiceCurrency; }
			set
			{
				base.D1_RX_InvoiceCurrency = value;
				ExchangeRateInfo.RefreshBinding();
			}
		}

		#region public ZDecimal ExchangeRate

		[DecimalPlaces(6)]
		public ZDecimal ExchangeRate
		{
			get
			{
				if (!D1_RX_InvoiceCurrency.IsEmpty)
				{
					RefCurrency currency = Factory.Load<RefCurrency>(D1_RX_InvoiceCurrency);
					return CurrConverter.GetExchangeRate(currency);
				}
				return ZDecimal.Zero;
			}
		}

		public ZPropertyInfo ExchangeRateInfo
		{
			get { return GetZPropertyInfo(nameof(ExchangeRate)); }
		}

		#endregion

		#region public ZString CountryOfDispatchExportCode
		public ZString CountryOfDispatchExportCode
		{
			get
			{
				if (!D1_RL_NKCountryOfDispatchOrExport.IsEmpty)
				{
					RefCountry country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, D1_RL_NKCountryOfDispatchOrExport.Substring(0, 2));
					if (country != null)
					{
						return FormatCountryCodeWithDescription(country);
					}
				}
				return ZString.Empty;
			}
		}

		public ZPropertyInfo CountryOfDispatchExportCodeInfo
		{
			get { return GetZPropertyInfo(nameof(CountryOfDispatchExportCode)); }
		}
		#endregion

		public ZBool IsImport
		{
			get { return (ZBool)(D1_MessageType == JobMessageTypeList.Codes.Import); }
		}

		public ZBool IsExport
		{
			get { return (ZBool)(D1_MessageType == JobMessageTypeList.Codes.Export); }
		}

		#region Currency
		public ZString Currency
		{
			get
			{
				if (!D1_RX_InvoiceCurrency.IsEmpty)
				{
					RefCurrency currency = Factory.Load<RefCurrency>(D1_RX_InvoiceCurrency);
					if (currency == null)
					{
						return ZString.Empty;
					}

					return currency.RX_Code;
				}
				return ZString.Empty;
			}
		}

		public ZPropertyInfo CurrencyInfo
		{
			get { return GetZPropertyInfo(nameof(Currency)); }
		}
		#endregion

		#region Overrides

		#region D1_GrossMass

		[DecimalPlaces(3)]
		public override ZDecimal D1_GrossMass
		{
			get { return base.D1_GrossMass; }
			set { base.D1_GrossMass = value; }
		}

		#endregion

		#region D1_ItemQty

		[DecimalPlaces(4)]
		public override ZDecimal D1_ItemQty
		{
			get { return base.D1_ItemQty; }
			set { base.D1_ItemQty = value; }
		}

		#endregion

		#region D1_InvoiceTotalAmount

		[DecimalPlaces(4)]
		public override ZDecimal D1_InvoiceTotalAmount
		{
			get { return base.D1_InvoiceTotalAmount; }
			set { base.D1_InvoiceTotalAmount = value; }
		}

		#endregion

		#endregion

		#region Implementation
		ZString FormatCountryCodeWithDescription(RefCountry country)
		{
			return country.Code + " - " + country.Description;
		}

		ZString FormatAddress(OrgHeader organisation)
		{
			if (organisation == null)
			{
				return ZString.Empty;
			}

			ZStringBuilder formattedAddress = new ZStringBuilder();
			formattedAddress.AppendIfNotEmpty(organisation.OH_FullNameTruncated);
			formattedAddress.AppendIfNotEmpty(organisation.MainAddress.OA_Address1);
			formattedAddress.AppendIfNotEmpty(organisation.MainAddress.OA_Address2);
			ZStringBuilder cityAndPostCode = new ZStringBuilder();
			cityAndPostCode.AppendIfNotEmpty(organisation.MainAddress.OA_City);
			cityAndPostCode.AppendIfNotEmpty(organisation.MainAddress.OA_PostCode);
			formattedAddress.AppendIfNotEmpty(cityAndPostCode.ToStringWithDelimiterBetweenAppends(" "));

			RefCountry refCountry = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, organisation.OH_RL_NKClosestPort.Left(2));
			ZStringBuilder stateAndCountry = new ZStringBuilder();
			stateAndCountry.AppendIfNotEmpty(organisation.MainAddress.OA_State);
			if (refCountry != null)
			{
				stateAndCountry.AppendIfNotEmpty(refCountry.Description.ToUpper());
			}
			formattedAddress.AppendIfNotEmpty(stateAndCountry.ToStringWithDelimiterBetweenAppends(" "));

			return formattedAddress.ToStringWithNewLineBetweenAppends();
		}
		#endregion
	}
}
