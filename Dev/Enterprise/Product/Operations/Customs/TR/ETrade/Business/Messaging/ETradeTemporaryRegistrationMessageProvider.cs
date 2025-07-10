using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Customs.Universal.Helper;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.ETrade.Business
{
	public class ETradeTemporaryRegistrationMessageProvider : IETradeTemporaryRegistration
	{
		public ETradeTemporaryRegistrationMessageProvider(AsycudaManifestHeader header)
		{
			Header = Argument.NotNull(header, nameof(header));
		}
		protected readonly AsycudaManifestHeader Header;
		GlbExternalPassword_TR TRBPassword => TRGlbStaffWrapper.Get(GlbStaff.CurrentUser).TRBPassword;
		BusinessObjectFactory factory => Header.Factory;
		GlbCompany headerBranchCompany => Header.Branch?.Company;
		ZDateTime EffectiveDate => Header.ApplicationBusinessProvider.GetEffectiveDateForDutyRate(Header);

		public BusinessObject Parent => Header;
		public IBusinessObjectCollection Messages => Header.Messages;
		public ZString JobReference => Header.AMA_JobReference;
		public ZString ReferenceNoToUpdate => Header.TempRegNo;

		#region Declaration

		public ZString Declaration1 => Header.AMA_Nature.SubstringSafe(0, 2);
		public ZString Declaration2 => Header.ProcedureCode.SubstringSafe(0, 1);
		public ZString Declaration3 => SpecialCargoCodes.Codes.ET;

		#endregion

		public ZInt TotalLineCount => Header.Bills.Count;
		public ZInt TotalBoxQty => Header.TotalBoxQty;
		public ZString ReferenceNo => ZString.Empty;
		public ZString DeclaringRepresentativeNameAndTitle => headerBranchCompany?.GC_Name ?? ZString.Empty;
		public ZString DeclaringRepresentativeTCTaxNo => headerBranchCompany?.GC_BusinessRegNo ?? ZString.Empty;

		#region VehicleOnExit

		public ZString TypeOfVehicleOnExit => ETradeMessageHelper.VehicleTransportType(Header.AMA_TransportMode, true);
		public ZString VehiclePlateOfVehicleOnExit => Header.DepartureFlight;
		public ZString CountryCodeOfVehicleOnExit => ETradeMessageHelper.CountryCodeOfTRCustoms(factory, Header.AMA_RN_NKConveyanceNationality, EffectiveDate);

		#endregion

		public ZBool IsContainer => Header.AMA_ContainerMode == ETradeMessageHelper.TurkishConst.ContainerMode;

		#region VehicleOnBorder

		public ZString TypeOfVehicleOnBorder => ETradeMessageHelper.VehicleTransportType(Header.AMA_TransportMode, true);
		public ZString VehiclePlateOfVehicleOnBorder => Header.AMA_Voyage;
		public ZString CountryCodeOfVehicleOnBorder => ETradeMessageHelper.CountryCodeOfTRCustoms(factory, Header.AMA_RN_NKConveyanceNationality, EffectiveDate);

		#endregion

		#region Total

		public ZString TotalInvoiceCurrencyCode => Core.Constants.CurrencyCodes.EuropeanUnion;
		public ZDecimal TotalInvoiceCurrencyValue => Header.CustomsValue.RoundAmount();
		public ZDecimal TotalInvoiceExchangeRate => Header.ExchangeRate.RoundExchangeAmount();
		public ZDecimal InvoiceAmountInformationTurkishLira => TotalInvoiceExchangeRate != 0 ? ZArchitecture.Core.Utilities.Round(TotalInvoiceCurrencyValue * TotalInvoiceExchangeRate, 2).RoundAmount() : 0;
		public ZDecimal StatisticalValue => Header.ConvertUsingCustomsRate(Header.AMA_DateAtCustomsOffice, Header.CustomsValue, Core.Constants.CurrencyCodes.EuropeanUnion, Core.Constants.CurrencyCodes.UnitedStates, headerBranchCompany).RoundAmount();
		public ZString TotalExpensesFreightCurrencyCode => Header.FreightValueCurrency;
		public ZDecimal TotalExpensesFreightCurrencyValue => Header.FreightValue.RoundAmount();
		public ZString TotalExpensesInsuranceCurrencyCode => Header.InsuranceValueCurrency;
		public ZDecimal TotalExpensesInsuranceCurrencyValue => Header.InsuranceValue.RoundAmount();
		public ZString OtherOverseasExpenditureCurrencyCode => ZString.Empty;
		public ZDecimal OtherOverseasExpenditureCurrencyValue => ZDecimal.Zero;
		public ZDecimal DomesticExpenditures => Header.OtherValue.RoundAmount();

		#endregion Total

		public ZString TransportTypeCode => ETradeMessageHelper.VehicleTransportType(Header.AMA_TransportMode, false);
		public ZString LoadingUnloadingPlace => TRMessageHelper.RemoveCountryCodePrefix(Header.DischargeLoadingCustomsOffice);
		public ZString CustomsOfficeCodeOfEntryExit => TRMessageHelper.RemoveCountryCodePrefix(Header.DischargeLoadingCustomsOffice);
		public ZString GoodsLocationCode => Header.GoodsLocationCode;
		public ZString GoodsLocationName => Header.LocationInformation;
		public ZString Adjustment => ZString.Empty;
		public ZString WarehouseTypeCode => Header.GoodsLocationCode;
		public ZString PrincipleResponsibleNameAndTitle => TRBPassword != null ? GlbStaff.CurrentUser.GS_FullName : ZString.Empty;
		public ZString PrincipleResponsibleTCTaxNo => TRBPassword != null ? TRBPassword.GP_UserID : ZString.Empty;
		public ZString CustomsOfficeCodeOfPredicted => TRMessageHelper.RemoveCountryCodePrefix(Header.PresentationCustomsOffice);
		public ZString CountryCodeOfPredicted => Header.AMA_Nature == ShipmentTypeList.Codes.Export22 ? Header.PresentationCustomsOffice.SubstringSafe(1, 2) : ZString.Empty;
		public ZString TotalGuaranteesType => Header.GuaranteeType;
		public ZDecimal TotalGuaranteesValue => Header.GuaranteeAmount.RoundAmount();
		public ZString CustomsOfficeCodeOfDestination => TRMessageHelper.RemoveCountryCodePrefix(Header.AMA_CustomsOffice);

		#region Transfers

		public ZString TransfersCountryCode => ETradeMessageHelper.CountryCodeOfTRCustoms(factory, Header.TransshipmentCountry, EffectiveDate);
		public ZString TransfersPlace => Header.TransshipmentLocation;
		public ZString TransfersNewVehicleReferanceNumber => Header.TransshipmentReference;
		public ZString TransfersNewCountryCode => ETradeMessageHelper.CountryCodeOfTRCustoms(factory, Header.TransshipmentConveyanceCountry, EffectiveDate);
		public ZBool IsTransfersContainer => Header.IsContainerized;
		public ZString TransfersPreviousContainerNo => Header.PreviousContainerNo;
		public ZString TransfersNewContainerNo => Header.NewContainerNo;

		#endregion

		public ZString Explanations => ZString.Empty;
		public IEnumerable<IBill> Bills
		{
			get
			{
				var bills = Header.Bills.Cast<AsycudaBill>().OrderBy(x => x.ABL_SequenceNumber);
				foreach (var bill in bills)
				{
					yield return new BillProvider(bill, EffectiveDate);
				}
			}
		}
	}

	public class BillProvider : IBill
	{
		public BillProvider(AsycudaBill asycudaBill, ZDateTime effectiveDate)
		{
			this.bill = asycudaBill;
			this.effectiveDate = effectiveDate;
		}
		readonly AsycudaBill bill;
		readonly ZDateTime effectiveDate;

		#region Helper Properties

		BusinessObjectFactory factory => bill.Factory;
		OrgHeader MarketPlaceHeader => bill.NotifyParty?.Header;
		ZDecimal StampTaxValue => bill.ABL_SequenceNumber == 1 ? bill.Header.StampTaxValue.RoundAmount() : ZDecimal.Zero;
		IEnumerable<AsycudaTax> ListTaxes => bill.AsycudaTaxes.Cast<AsycudaTax>();

		#endregion

		public ZString BillOfLadingLineNo => bill.ABL_SequenceNumber.ToString();
		public ZString BillOfLadingNumber => bill.ABL_BillNumber;
		public ZString SummaryDeclarationNo => bill.CustomsEntryNumber;
		public ZString PackType => ETradeMessageHelper.TurkishConst.PackTypeBin;
		public ZInt PackQuantity => bill.ABL_ManifestQty;
		public ZString ContainerBrand => bill.ABL_MarksAndNumbers;
		public ZString ContainerNo => bill.ContainerNumber;
		public ZString ContainerPackType => ETradeMessageHelper.TurkishConst.PackTypeBin;
		public ZInt ContainerPackQuantity => bill.ABL_ManifestQty;
		public ZDecimal GrossWeight => bill.GrossWeightInKG.RoundAmount();
		public ZDecimal NetWeight => Core.Constants.Weight.ConvertSafe(bill.ABL_NetWeight, bill.ABL_NetWeightUQ, Core.Constants.Weight.Kilograms).RoundAmount();

		#region Forwarder

		public ZString ForwarderNameAndTitle => bill.ABL_ShipperName;
		public ZString ForwarderTCTaxNo => bill.ABL_ShipperRegNo;

		#endregion

		#region Consignee

		public ZString ConsigneeName => bill.ABL_ConsigneeName;
		public ZString ConsigneeTitle => ZString.Empty;
		public ZString ConsigneeTCTaxNo => bill.ABL_ConsigneeRegNo;
		public ZString ConsigneeStreetNumber => bill.ABL_ConsigneeStreet1 + bill.ABL_ConsigneeStreet2;
		public ZString ConsigneeCityCode => bill.ABL_ConsigneeCity;
		public ZString ConsigneeTown => bill.Lookups.ConsigneeState_List.GetDescriptionFromCode(bill.ABL_ConsigneeState);
		public ZString ConsigneePostalCode => bill.ABL_ConsigneePostcode;

		#endregion

		#region MarketPlace

		public ZString MarketPlaceNameAndTitle => MarketPlaceHeader?.OH_FullName.SubstringSafe(0, 35) ?? ZString.Empty;
		public ZString MarketPlaceTCTaxNo => MarketPlaceHeader?.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.Turkey) ?? ZString.Empty;

		#endregion

		#region Locations

		public ZString ReferralDestinationCountryCode => ETradeMessageHelper.CountryCodeOfTRCustoms(factory, bill.Header.IsImport ? bill.DepartureCountry : bill.ExportCountry, effectiveDate);
		public ZString TradeCountryCode => ETradeMessageHelper.CountryCodeOfTRCustoms(factory, bill.TradeCountry, effectiveDate);
		public ZString ExportCountryCode => ETradeMessageHelper.CountryCodeOfTRCustoms(factory, bill.Header.IsImport ? bill.ExportCountry : bill.DepartureCountry, effectiveDate);
		public ZString DestinationCountryCode => ETradeMessageHelper.CountryCodeOfTRCustoms(factory, bill.ArrivalCountry, effectiveDate);
		public ZString DeliveryMethod => bill.ABL_Incoterm;
		public ZString DeliverLocation => bill.ABL_GoodsLocation;
		public ZString TransactionNature => bill.NatureOfBusiness;

		#endregion

		#region Exception

		public ZString ExceptionCode1 => bill.ExemptionCode1;
		public ZString ExceptionCode2 => bill.ExemptionCode2;

		#endregion

		public ZString RegimeCode => bill.ABL_Procedure;
		public IEnumerable<IDocument> Documents
		{
			get
			{
				var docs = bill.SupportingDocumentsForBill.Cast<SupportingDocuments>();
				foreach (var doc in docs)
				{
					yield return new DocumentsProvider(doc);
				}
			}
		}
		public ZString FinancialBankingCode => ZString.Empty;
		public ZString FinancialBankingPaymentType => bill.PaymentMethod;
		public ZDecimal FinancialBankingAmount => ZDecimal.Zero;
		public ZString InvoiceCurrencyCode => bill.ABL_RX_NKCustomsValueCurrency;
		public ZDecimal InvoiceAmount => bill.ABL_CustomsValue.RoundAmount();
		public ZDecimal InvoiceExchangeRate => bill.Header.ExchangeRate.RoundExchangeAmount();

		public IEnumerable<ITax> Taxes
		{
			get
			{
				foreach (var tax in ListTaxes)
				{
					yield return new TaxesProvider(tax);
				}

				if (StampTaxValue > 0)
				{
					yield return new TaxesHeaderProvider(StampTaxValue);
				}
			}
		}

		#region Payment Total

		public ZDecimal TaxPaymentTotalTaxPaymentAmount
		{
			get
			{
				var cashPaymentTotal = GetTotalTaxByType(MethodOfPaymentList.Codes.Cash) + StampTaxValue;
				return cashPaymentTotal.RoundAmount();
			}
		}

		public ZDecimal TaxPaymentTaxAmountToBeBonded => GetTotalTaxByType(MethodOfPaymentList.Codes.Guarantee);
		public ZDecimal TaxPaymentTotalTaxAmountPayableLater => GetTotalTaxByType(MethodOfPaymentList.Codes.Defered);

		ZDecimal GetTotalTaxByType(ZString paymentType)
		{
			ZDecimal totalTax = ListTaxes.Where(x => x.AET_MethodOfPayment == paymentType).Sum(x => x.AET_ChargeAmount);
			return totalTax.RoundAmount();
		}

		public ZDecimal TaxPaymentTotal => (bill.TaxAmount + StampTaxValue).RoundAmount();

		#endregion

		#region Guarantee

		public ZString GuaranteeType => bill.GuaranteeType;
		public ZDecimal GuaranteeAmount => bill.GuaranteeAmount.RoundAmount();
		public ZString GuaranteeReferenceNo => bill.GuaranteeRefNo;

		#endregion

		public ZString FreightInformationCurrencyCode => bill.ABL_TransportValue > 0 ? bill.ABL_RX_NKTransportValueCurrency : bill.PrecedentFreightCostCurrency;
		public ZDecimal FreightInformationValue => (bill.ABL_TransportValue + bill.PrecedentFreightToDisplay).RoundAmount();
		public ZString InsuranceInformationCurrencyCode => bill.ABL_RX_NKInsuranceValueCurrency;
		public ZDecimal InsuranceInformationValue => bill.ABL_InsuranceValue.RoundAmount();
		public ZString OtherOverseasExpansesCurrencyCode => ZString.Empty;
		public ZDecimal OtherOverseasExpansesValue => ZDecimal.Zero;
		public ZDecimal DomesticExpanses => ZDecimal.Zero;
		public ZString Volume => bill.ABL_Volume.RoundAmount().ToString();
		public ZString TradeType => bill.ABL_SpecialCargoCode;

		public IEnumerable<IPack> Packs
		{
			get
			{
				var packs = bill.Packs.Cast<AsycudaPack>();
				foreach (var pack in packs)
				{
					yield return new PackProvider(pack, effectiveDate);
				}
			}
		}
	}

	public class DocumentsProvider : IDocument
	{
		public DocumentsProvider(SupportingDocuments document)
		{
			this.document = document;
		}

		readonly SupportingDocuments document;

		public ZString Code => document.CSI_Code;
		public ZDateTime DocumentDate => document.CSI_DateOfIssue;
		public ZString ReferenceNo => document.CSI_ReferenceNumber;
		public ZString Verfication => ETradeMessageHelper.VerficationCode(document.CSI_Status);
	}

	public class TaxesHeaderProvider : ITax
	{
		public TaxesHeaderProvider(ZDecimal chargeAmount)
		{
			this.chargeAmount = chargeAmount;
		}

		readonly ZDecimal chargeAmount;

		public ZString Code => TaxCodeList.Codes.StampTax;
		public ZString Description => new TaxCodeList().GetDescriptionFromCode(TaxCodeList.Codes.StampTax);
		public ZDecimal Base => ZDecimal.Zero;
		public ZDecimal Rate => ZDecimal.Zero;
		public ZDecimal Amount => chargeAmount.RoundAmount();
		public ZString PaymentType => ETradeMessageHelper.TurkishConst.TaxPaymentType;
	}

	public class TaxesProvider : ITax
	{
		public TaxesProvider(AsycudaTax tax)
		{
			this.tax = tax;
		}

		readonly AsycudaTax tax;

		public ZString Code => tax.AET_ChargeType;
		public ZString Description => new TaxCodeList().GetDescriptionFromCode(tax.AET_ChargeType);
		public ZDecimal Base => tax.AET_BaseValue.RoundAmount();
		public ZDecimal Rate => tax.AET_Rate.RoundAmount();
		public ZDecimal Amount => tax.AET_ChargeAmount.RoundAmount();
		public ZString PaymentType => ETradeMessageHelper.TurkishConst.TaxPaymentType;
	}

	public class PackProvider : IPack
	{
		public PackProvider(AsycudaPack asycudaPack, ZDateTime effectiveDate)
		{
			this.pack = asycudaPack;
			this.effectiveDate = effectiveDate;
		}

		readonly AsycudaPack pack;
		readonly ZDateTime effectiveDate;
		AsycudaPackedItem packedItem => pack.PackedItem;

		public ZString PackNo => pack.APA_LineNo.ToString();
		public ZString CommercialDescription => pack.APA_GoodsDescription;

		public ZString ItemsSerialNo => packedItem != null ? packedItem.SerialNo : ZString.Empty;
		public ZInt ItemsQuantity => packedItem != null ? packedItem.API_CustomsQty.ToZInt() : ZInt.Zero;
		public ZString ItemsBrand => packedItem != null ? packedItem.API_Brand : ZString.Empty;
		public ZString ItemsModel => packedItem != null ? packedItem.API_Model : ZString.Empty;
		public ZString UsedItemCode => packedItem != null ? packedItem.UsedGoodsCode : ZString.Empty;

		#region ItemCode

		public ZString ItemCode1 => packedItem != null ? packedItem.API_Tariff.SubstringSafe(0, 8) : ZString.Empty;
		public ZString ItemCode2 => packedItem != null ? packedItem.API_Tariff.SubstringSafe(8, 2) : ZString.Empty;
		public ZString ItemCode3 => packedItem != null ? packedItem.API_Tariff.SubstringSafe(10, 2) : ZString.Empty;

		#endregion

		#region PreferentialTariff

		public ZString PreferentialTariffCode1 => ZString.Empty;
		public ZString PreferentialTariffCode2 => ZString.Empty;

		#endregion

		public ZString ValueStatementForm => packedItem != null ? packedItem.ValueDeclarationForm : ZString.Empty;
		public ZString AgriculturePolicy => packedItem != null ? packedItem.AgriculturePolicy : ZString.Empty;
		public ZBool IsQuota => packedItem != null ? packedItem.QuotaCheck : ZBool.False;

		#region SupplementaryMeasures

		public ZString SupplementaryMeasuresType1 => packedItem != null ? packedItem.API_CustomsUQ2 : ZString.Empty;
		public ZDecimal SupplementaryMeasuresQuantity1 => packedItem != null ? packedItem.API_CustomsQty2.RoundAmount() : ZDecimal.Zero;
		public ZString SupplementaryMeasuresType2 => packedItem != null ? packedItem.API_CustomsUQ3 : ZString.Empty;
		public ZDecimal SupplementaryMeasuresQuantity2 => packedItem != null ? packedItem.API_CustomsQty3.RoundAmount() : ZDecimal.Zero;

		#endregion

		public ZString BillAmountCurrencyCode => packedItem != null ? packedItem.API_RX_NKGoodsValueCurrency : ZString.Empty;
		public ZDecimal BillAmountValue => packedItem != null ? packedItem.API_GoodsValue.RoundAmount() : ZDecimal.Zero;
		public ZString CalculationMethod => packedItem != null ? packedItem.CalculationMethod : ZString.Empty;
		public ZDecimal StatisticalValue => packedItem != null ? packedItem.StatisticalValue.RoundAmount() : ZDecimal.Zero;
		public ZString CountryCodeOfOrigin => ETradeMessageHelper.CountryCodeOfTRCustoms(packedItem.Factory, packedItem.API_RN_NKGoodsOrigin, effectiveDate);
	}
}
