using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ACEManifest.Business
{
	[CodeProperty(AsycudaBill.Schema.ABL_BillNumber), DescriptionProperty(AsycudaBill.Schema.ABL_BillNumber)]
	public partial class AsycudaBill : ASYCUDA.Business.AsycudaBill, Integration.Customs.ASYCUDA.ACEManifest.IAsycudaBill
	{
		public AsycudaBill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			ABL_BolType = Core.Constants.ShipmentTypes.StandardHouse;
		}

		protected override ZString HumanReadableShortcutNameCore
		{
			get
			{
				var result = $"{AsycudaManifestHeader.US_Air_AMS} {Header.AMA_JobReference}";
				if (!ABL_BillNumber.IsEmpty)
				{
					result += $" - {ABL_BillNumber}";
				}
				return result;
			}
		}

		public override ZString ABL_BillNumber
		{
			get => base.ABL_BillNumber;
			set
			{
				var oldValue = ABL_BillNumber;
				base.ABL_BillNumber = value;
				if (ABL_BillNumber != oldValue && !IsCopying)
				{
					var arrivalHeaders = Header?.ArrivalHeaders;
					if (arrivalHeaders != null)
					{
						foreach (AsycudaArrivalHeader arrivalHeader in arrivalHeaders)
						{
							arrivalHeader.MarkTransferBillsAsNeedingValidation();
						}
					}
				}
			}
		}

		protected override bool ABL_BillStatus_ReadOnly => true;

		public bool IsSent => ABL_MessageStatus == EDIMessage.Status.Sent;

		public bool IsError => ABL_MessageStatus == EDIMessage.Status.Error;

		public void ResetMessageStatus()
		{
			if (IsSent || IsError)
			{
				ABL_MessageStatus = ZString.Empty;
			}
		}

		public ZDecimal GoodsValueInUSD => ConvertToUSD(ABL_GoodsValue, ABL_RX_NKGoodsValueCurrency);

		public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;
		protected override ZString GetCountryCode() => Core.Constants.CountryCodes.UnitedStates;
		protected override ManifestBase.AsycudaBillValidation GetNewValidationForRegularBill() => new AsycudaBillValidationForRegularBill(this);
		public new AsycudaBillLookups Lookups => (AsycudaBillLookups)base.Lookups;
		protected override ManifestBase.AsycudaBillLookups GetNewLookups() => new AsycudaBillLookups(this);
		protected override Type GetPackageContainerLinkTypeCore() => typeof(AsycudaContainerBillOrPackageLink);

		#region CurrencyConversion

		internal ZDecimal ConvertToUSD(ZDecimal amount, ZString currencyCode)
		{
			ZDecimal result = 0m;

			if (currencyCode == Core.Constants.CurrencyCodes.UnitedStates)
			{
				result = amount;
			}
			else if (!amount.IsEmpty && !currencyCode.IsEmpty && LoadFromCurrencyCode(currencyCode) is RefCurrency currency)
			{
				var usd = LoadFromCurrencyCode(Core.Constants.CurrencyCodes.UnitedStates);
				CurrencyConverter.DateForRate = ValuationDate;
				result = CurrencyConverter.ConvertExact(new Money(amount, currency), usd).Amount.Round(0);
			}

			return result;
		}

		public ZDateTime ValuationDate
		{
			get
			{
				var departureDate = Header.AMA_E_DEP;
				return departureDate.IsEmpty ? ZDateTime.Today : departureDate;
			}
		}

		RefCurrency LoadFromCurrencyCode(ZString currencyCode)
		{
			return Factory.GetCachedValue("RefCurrency|" + currencyCode, () => RefCurrency.LoadFromCurrencyCode(Factory, currencyCode));
		}

		CurrencyConverter CurrencyConverter => fCurrencyConverter ?? (fCurrencyConverter = CurrencyConverter.New(Factory, ValuationDate, ExchangeRateType.Customs, 0));
		CurrencyConverter fCurrencyConverter;

		#endregion

		#region IOriginatorCodeProvider Members

		ZString Integration.Customs.ASYCUDA.ACEManifest.IOriginatorCodeProvider.AirAMSOriginatorCode => Header is Integration.Customs.ASYCUDA.ACEManifest.IOriginatorCodeProvider header ? header.AirAMSOriginatorCode : ZString.Empty;

		#endregion
	}
}
