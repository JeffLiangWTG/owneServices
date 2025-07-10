using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.TR.MessageContracts.Interfaces.Declaration;
using CargoWise.Customs.TR.MessageContracts.Interfaces.ExportUnion;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.Business
{
	public class EntryLinesProvider : IEntryLines
	{
		public EntryLinesProvider(CusEntryLine line, ZString messageType, ZDateTime effectiveDate)
		{
			CusEntryLine = Argument.NotNull(line, nameof(line));
			RandomInvoiceLine = (JobComInvoiceLine)CusEntryLine.RandomLine;
			MessageType = messageType;
			EffectiveDate = effectiveDate;
		}
		CusEntryLine CusEntryLine { get; }
		JobComInvoiceLine RandomInvoiceLine { get; }
		ZString MessageType { get; }
		ZDateTime EffectiveDate { get; }

		bool IsExport => RandomInvoiceLine.IsExport;

		#region Sub Tables

		public IReadOnlyCollection<IComplementaryInfo> ComplementaryInfo => Array.Empty<IComplementaryInfo>();

		public IReadOnlyCollection<IDeclarationOpeningAndClosingInfo> DeclarationOpeningAndClosingInfo
		{
			get
			{
				if (fDeclarationOpeningAndClosingInfo == null && !IsExport)
				{
					var list = new List<IDeclarationOpeningAndClosingInfo>();

					foreach (var lineWithSamePreviousEntryNumbers in CusEntryLine.InvoiceLines.Cast<JobComInvoiceLine>().Where(x => !x.JI_PreviousEntryNumber.IsEmpty && !x.JI_PreviousEntryLineNumber.IsEmpty).GroupBy(x => x.JI_PreviousEntryNumber))
					{
						foreach (var lineWithSamePreviousEntryLineNumbers in lineWithSamePreviousEntryNumbers.GroupBy(x => x.JI_PreviousEntryLineNumber))
						{
							var totalPackQty = ZDecimal.Zero;
							var allDescriptions = ZString.Empty;
							var invoiceLineWithSamePreviousEntryInfo = lineWithSamePreviousEntryLineNumbers.FirstOrDefault();

							foreach (var filteredInvoiceline in lineWithSamePreviousEntryLineNumbers)
							{
								totalPackQty += filteredInvoiceline.PackagesPivot.Cast<Customs.Business.InvoiceLinePackagePivot>().Sum(x => x.Package?.CW_PackQty ?? 0);

								if (!filteredInvoiceline.ZG_ProcessingDescription.IsEmpty && !allDescriptions.Contains(filteredInvoiceline.ZG_ProcessingDescription))
								{
									allDescriptions = allDescriptions + " " + filteredInvoiceline.ZG_ProcessingDescription;
								}
							}

							list.Add(new PreviousDocumentsProvider(invoiceLineWithSamePreviousEntryInfo, totalPackQty, allDescriptions));
						}
					}

					fDeclarationOpeningAndClosingInfo = list.ToArray();
				}

				return fDeclarationOpeningAndClosingInfo ?? Array.Empty<IDeclarationOpeningAndClosingInfo>();
			}
		}
		IReadOnlyCollection<IDeclarationOpeningAndClosingInfo> fDeclarationOpeningAndClosingInfo;

		public IReadOnlyCollection<ICusVehicle> CusVehicle
		{
			get
			{
				if (fVehicle == null)
				{
					var list = new List<ICusVehicle>();
					foreach (JobComInvoiceLine invoiceline in CusEntryLine.InvoiceLines)
					{
						foreach (CusVehicle vehicle in invoiceline.Vehicles)
						{
							list.Add(new CusVehicleProvider(vehicle));
						}
					}

					if (IsExport && list.Count == 0)
					{
						list.Add(new CusVehicleEmptyProvider());
					}

					fVehicle = list.ToArray();
				}
				return fVehicle ?? Array.Empty<ICusVehicle>();
			}
		}
		IReadOnlyCollection<ICusVehicle> fVehicle;

		public IReadOnlyCollection<IContainerInfo> ContainerInfo
		{
			get
			{
				if (fContainer == null)
				{
					var list = new List<IContainerInfo>();
					var groupContainerNumbers = CusEntryLine.InvoiceLines.Cast<JobComInvoiceLine>().SelectMany((JobComInvoiceLine x) => x.ContainersPivot)
																		 .Cast<Customs.Business.CusContainerInvoiceLinePivot>().GroupBy(x => x.ContainerNumber);
					foreach (var containers in groupContainerNumbers)
					{
						var container = containers.FirstOrDefault();
						list.Add(new ContainerProvider(container, RandomInvoiceLine.EffectiveAssessmentDate));
					}
					fContainer = list.ToArray();
				}

				return fContainer;
			}
		}
		IReadOnlyCollection<IContainerInfo> fContainer;

		public IReadOnlyCollection<ITaxImmunitys> TaxImmunitys
		{
			get
			{
				if (fTaxImmunitys == null && !IsExport)
				{
					var list = new List<ITaxImmunitys>();
					foreach (JobComInvoiceLine invoiceline in CusEntryLine.InvoiceLines)
					{
						foreach (var supplementaryCode in invoiceline.SupplementaryCodes)
						{
							list.Add(new AdditionalSupplementaryCodesProvider(supplementaryCode));
						}
					}
					fTaxImmunitys = list.ToArray();
				}

				return fTaxImmunitys ?? Array.Empty<ITaxImmunitys>();
			}
		}
		IReadOnlyCollection<ITaxImmunitys> fTaxImmunitys;

		public IReadOnlyCollection<IAviationFuelTypes> AviationFuelTypes
		{
			get
			{
				if (fAviationFuelTypes == null)
				{
					var list = new List<IAviationFuelTypes>();
					foreach (JobComInvoiceLine invoiceline in CusEntryLine.InvoiceLines)
					{
						foreach (var aviationFuelType in invoiceline.AviationFuelTypeCollection)
						{
							list.Add(new AviationFuelTypeProvider(aviationFuelType));
						}
					}
					fAviationFuelTypes = list.ToArray();
				}

				return fAviationFuelTypes;
			}
		}
		IReadOnlyCollection<IAviationFuelTypes> fAviationFuelTypes;

		public IReadOnlyCollection<IPaymentTypes> PaymentTypes
		{
			get
			{
				if (fPaymentTypes == null)
				{
					var list = new List<IPaymentTypes>();
					var group = CusEntryLine.InvoiceLines.Cast<JobComInvoiceLine>().GroupBy(x => x.ZG_CommercialPaymentCode);
					foreach (var payments in group)
					{
						var payment = payments.FirstOrDefault();
						list.Add(new PaymentProvider(payment, payments.Sum(s => s.ZG_CommercialPaymentAmount)));
					}
					fPaymentTypes = list.ToArray();
				}

				return fPaymentTypes;
			}
		}
		IReadOnlyCollection<IPaymentTypes> fPaymentTypes;

		#endregion

		public string Tariff => CusEntryLine.CL_AdValoremTariff;
		public string ManufacturerCompanyInfo => RandomInvoiceLine.JI_OA_ManufacturerAddress_ZAddress.OrgPK == ZGuid.Empty ? CusEntryMessageConstants.TurkishAnswers.No : CusEntryMessageConstants.TurkishAnswers.Yes;
		public int LineOrderNo => CusEntryLine.CL_LineNumber;
		public string CountryOfOrigin => UniversalReferenceDataHelper.MapCW1CountryCodeToCustomsCode(CusEntryLine.Factory, RandomInvoiceLine.JI_CountryOfOrigin, RandomInvoiceLine.EffectiveAssessmentDate);

		#region Weight and Quantity

		public decimal GrossWeight => CusEntryLine.InvoiceLines.Cast<JobComInvoiceLine>().Sum(i => i.JI_Weight).RoundAmount();
		public decimal NetWeight => CusEntryLine.InvoiceLines.Cast<JobComInvoiceLine>().Sum(i => i.JI_NetWeight).RoundAmount();
		public string ComplementaryDimensionsUnit => RandomInvoiceLine.JI_CustomsSecondUnitQty;
		public decimal StatisticalQuantity => CusEntryLine.InvoiceLines.Cast<JobComInvoiceLine>().Sum(i => i.JI_CustomsSecondQuantity).RoundAmount();
		public string InternationalAgreement => !IsExport ? RandomInvoiceLine.JI_PrimaryPreference : ZString.Empty;
		public string PerceptionUnit1 => !IsExport ? RandomInvoiceLine.JI_CustomsThirdUnitQty : ZString.Empty;
		public decimal PerceptionQuantity1 => !IsExport ? CusEntryLine.InvoiceLines.Cast<JobComInvoiceLine>().Sum(i => i.JI_CustomsThirdQuantity).RoundPerceptionQuantity() : decimal.Zero;
		public string PerceptionUnit2 => !IsExport ? RandomInvoiceLine.JI_CustomsFourthUnitQty : ZString.Empty;
		public decimal PerceptionQuantity2 => !IsExport ? CusEntryLine.InvoiceLines.Cast<JobComInvoiceLine>().Sum(i => i.JI_CustomsFourthQuantity).RoundPerceptionQuantity() : decimal.Zero;
		public string PerceptionUnit3 => !IsExport ? RandomInvoiceLine.JI_CustomsFifthUnitQty : ZString.Empty;
		public decimal PerceptionQuantity3 => !IsExport ? CusEntryLine.InvoiceLines.Cast<JobComInvoiceLine>().Sum(i => i.JI_CustomsFifthQuantity).RoundPerceptionQuantity() : decimal.Zero;

		#endregion

		#region Exemption Codes

		ZString[] Exemptions => RandomInvoiceLine.JI_AdditionalSupplements.Split(TRMessageConstants.Comma);
		public string Exemptions1 => Exemptions.Length >= 1 ? (string)Exemptions[0] : string.Empty;
		public string Exemptions2 => Exemptions.Length >= 2 ? (string)Exemptions[1] : string.Empty;
		public string Exemptions3 => Exemptions.Length >= 3 ? (string)Exemptions[2] : string.Empty;
		public string Exemptions4 => Exemptions.Length >= 4 ? (string)Exemptions[3] : string.Empty;
		public string Exemptions5 => Exemptions.Length >= 5 ? (string)Exemptions[4] : string.Empty;

		#endregion

		public string TypeOfDelivery => IsExport ? RandomInvoiceLine.InvoiceHeader?.JZ_IncoTerm.ToString() ?? ZString.Empty : ZString.Empty;
		public string AdditionalCode => !IsExport ? RandomInvoiceLine.JI_SupplementaryCode1 : ZString.Empty;
		public string Feature => RandomInvoiceLine.ZG_PriceType.ToString() ?? ZString.Empty;

		#region Values

		public decimal InvoiceAmount
		{
			get
			{
				var invoiceAmount = ZDecimal.Zero;
				var invoiceLines = CusEntryLine.InvoiceLines.Cast<JobComInvoiceLine>().ToArray();
				if (invoiceLines.Length > 0)
				{
					var defaultInvoice = CusEntryLine.Declaration.Invoices.Cast<JobComInvoiceHeader>().Single();
					var converter = defaultInvoice.CurrencyConverter;
					var currency = RefCurrency.LoadFromCurrencyCode(defaultInvoice.Factory, invoiceLines[0].JI_RX_NKLinePriceCurr);
					var sumInLocal = Money.Empty;

					foreach (JobComInvoiceLine line in invoiceLines)
					{
						sumInLocal = converter.Add(sumInLocal, line.JI_LinePriceMoney);
					}
					if (!sumInLocal.IsEmpty)
					{
						invoiceAmount = converter.ConvertExact(sumInLocal, currency).Amount;
					}
				}

				return invoiceAmount.RoundAmount();
			}
		}

		public string InvoiceAmountCurrency => RandomInvoiceLine.JI_RX_NKLinePriceCurr;
		public decimal BorderPassFee => ZDecimal.Zero;
		public decimal FreightAmount => DeclarationProviderHelper.GetChargesTotal(CusEntryLine, ZBool.False, TRIncotermChargeCodeList.Codes.OFT);
		public string FreightAmountCurrency => DeclarationProviderHelper.GetDefaultCurrencyCode(CusEntryLine, TRIncotermChargeCodeList.Codes.OFT);
		public decimal StatisticalValue => IsExport ? CusEntryLine.InvoiceLines.Cast<JobComInvoiceLine>().Sum(i => i.JI_StatisticalValueUSD) : ZDecimal.Zero;
		public decimal InsuranceValue => DeclarationProviderHelper.GetChargesTotal(CusEntryLine, ZBool.False, TRIncotermChargeCodeList.Codes.ONS);
		public string InsuranceValueCurrency => DeclarationProviderHelper.GetDefaultCurrencyCode(CusEntryLine, TRIncotermChargeCodeList.Codes.ONS);

		#endregion

		public string TariffDescription => ZString.Empty;
		public string CommercialDescription => RandomInvoiceLine.JI_NDescription;
		public string Brand => RandomInvoiceLine.JI_BrandName;

		public string Number
		{
			get
			{
				var marksAndNoList = new List<ZString>();
				var marksAndNoGroups = CusEntryLine.InvoiceLines.Cast<JobComInvoiceLine>()
					.SelectMany((JobComInvoiceLine line) => line.PackagesPivot)
					.Cast<Customs.Business.InvoiceLinePackagePivot>()
					.Select((Customs.Business.InvoiceLinePackagePivot pivot) => pivot.Package)
					.Cast<Customs.Business.BasePackage>()
					.GroupBy(g => g.CW_MarksAndNos);

				foreach (var mark in marksAndNoGroups)
				{
					marksAndNoList.Add(mark.Key);
				}

				return marksAndNoList.Count == 0 ? TRMessageConstants.Dot : string.Join(" ", marksAndNoList);
			}
		}

		public string Type
		{
			get
			{
				var containerCount = DeclarationProviderHelper.GetContainerCount(CusEntryLine);
				return containerCount > 0 ? CusEntryMessageConstants.CountryConstants.PackTypeContainer : CusEntryMessageConstants.CountryConstants.PackTypeQuantity;
			}
		}

		public decimal Pieces
		{
			get
			{
				var totalPieces = ZDecimal.Zero;
				var containerCount = DeclarationProviderHelper.GetContainerCount(CusEntryLine);
				if (containerCount > 0)
				{
					totalPieces = containerCount;
				}
				else
				{
					totalPieces = CusEntryLine.InvoiceLines.Cast<JobComInvoiceLine>().SelectMany((JobComInvoiceLine line) => line.PackagesPivot).Cast<Customs.Business.InvoiceLinePackagePivot>().Sum(x => x.CHC_NumberOfPacks);
				}

				return totalPieces.RoundAmount();
			}
		}

		public string QuantityUnit => RandomInvoiceLine.JI_CustomsSecondUnitQty;
		public string ReturnToCountryOfOrigin => CusEntryLine.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.ZG_ReturnToOrigin) ? CusEntryMessageConstants.TurkishAnswers.Yes : string.Empty;
		public string SecondaryProcess => CusEntryLine.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.ZG_SecondaryTreatedProduct) ? CusEntryMessageConstants.TurkishAnswers.Yes : string.Empty;
		public string LineNo => RandomInvoiceLine.ZG_InwardProcessingLicenseLineNumber;
		public decimal Quantity => CusEntryLine.InvoiceLines.Cast<JobComInvoiceLine>().Sum(x => x.JI_CustomsSecondQuantity).RoundAmount();
		public string VatRatio => DeclarationProviderHelper.GetCustomsRatioCode(IsExport, RandomInvoiceLine.Lookups.TaxOrFeeCodeList, RandomInvoiceLine.JI_ZZF_NKTaxType);
		public string UsedGoods => !IsExport ? RandomInvoiceLine.ZG_UsedGoodsCode : ZString.Empty;
		public string Description44
		{
			get
			{
				var allDescriptions = ZString.Empty;
				var invoiceLineDescriptions = ZString.Empty;

				if (!IsExport)
				{
					foreach (JobComInvoiceLine invoiceLine in CusEntryLine.InvoiceLines)
					{
						invoiceLineDescriptions = ZString.Empty;
						foreach (var additionalInfo in invoiceLine.AdditionalInfos.Where(x => !x.CSI_Description.IsEmpty))
						{
							if (!invoiceLineDescriptions.Contains(additionalInfo.CSI_Description))
							{
								invoiceLineDescriptions += additionalInfo.CSI_Description + " ";
							}
						}
						invoiceLineDescriptions = invoiceLineDescriptions.TrimEnd();

						if (!invoiceLineDescriptions.IsEmpty && !allDescriptions.Contains(invoiceLineDescriptions))
						{
							allDescriptions += invoiceLineDescriptions + " ";
						}
					}
				}
				return allDescriptions.TrimEnd();
			}
		}

		#region Charges

		public string ProducerRegNo => RandomInvoiceLine.ManufacturerAddress?.Header.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.VATCode) ?? ZString.Empty;
		public decimal DomesticOther => DeclarationProviderHelper.GetChargesTotal(CusEntryLine, ZBool.False, TRIncotermChargeCodeList.Codes.LOT);
		public decimal DomesticBank => DeclarationProviderHelper.GetChargesTotal(CusEntryLine, ZBool.False, TRIncotermChargeCodeList.Codes.LBC);
		public decimal DomesticStoring => DeclarationProviderHelper.GetChargesTotal(CusEntryLine, ZBool.False, TRIncotermChargeCodeList.Codes.LSC);
		public decimal DomesticDischarge => DeclarationProviderHelper.GetChargesTotal(CusEntryLine, ZBool.False, TRIncotermChargeCodeList.Codes.LDC);
		public decimal DomesticPort => DeclarationProviderHelper.GetChargesTotal(CusEntryLine, ZBool.False, TRIncotermChargeCodeList.Codes.LPC);
		public decimal DomesticCulture => !IsExport ? DeclarationProviderHelper.GetChargesTotal(CusEntryLine, ZBool.False, TRIncotermChargeCodeList.Codes.LocalCultureCharge) : ZDecimal.Zero;
		public decimal DomesticKkdf => !IsExport ? DeclarationProviderHelper.GetChargesTotal(CusEntryLine, ZBool.False, TRIncotermChargeCodeList.Codes.LocalResourceUtilizationSupportFundCharge) : ZDecimal.Zero;
		public decimal TotalDomesticExpensesIncludingVAT => !IsExport ? DeclarationProviderHelper.GetChargesTotal(CusEntryLine, ZBool.False, TRIncotermChargeCodeList.Codes.LocalTotalCharges) : ZDecimal.Zero;
		public decimal DomesticEnvironment => !IsExport ? DeclarationProviderHelper.GetChargesTotal(CusEntryLine, ZBool.False, TRIncotermChargeCodeList.Codes.LocalEnvironmentCharge) : ZDecimal.Zero;
		public string DomesticOtherDescription => DeclarationProviderHelper.GetChargesExplanation(CusEntryLine, TRIncotermChargeCodeList.Codes.LOT);
		public string ExemptionDescription => ZString.Empty;
		public string ReferenceDate => ZString.Empty;
		public decimal OverseasCommission => !IsExport ? DeclarationProviderHelper.GetChargesTotal(CusEntryLine, ZBool.False, TRIncotermChargeCodeList.Codes.COM) : ZDecimal.Zero;
		public decimal OverseasDemurrage => !IsExport ? DeclarationProviderHelper.GetChargesTotal(CusEntryLine, ZBool.False, TRIncotermChargeCodeList.Codes.DEM) : ZDecimal.Zero;
		public decimal OverseasRoyalty => !IsExport ? DeclarationProviderHelper.GetChargesTotal(CusEntryLine, ZBool.False, TRIncotermChargeCodeList.Codes.ROY) : ZDecimal.Zero;
		public decimal OverseasInterest => !IsExport ? DeclarationProviderHelper.GetChargesTotal(CusEntryLine, ZBool.False, TRIncotermChargeCodeList.Codes.INT) : ZDecimal.Zero;
		public decimal OverseasOther => DeclarationProviderHelper.GetChargesTotal(CusEntryLine, ZBool.False, TRIncotermChargeCodeList.Codes.OTH);
		public string OverseasCommissionCurrency => DeclarationProviderHelper.GetDefaultCurrencyCode(CusEntryLine, TRIncotermChargeCodeList.Codes.COM);
		public string OverseasDemurrageCurrency => DeclarationProviderHelper.GetDefaultCurrencyCode(CusEntryLine, TRIncotermChargeCodeList.Codes.DEM);
		public string OverseasRoyaltyCurrency => !IsExport ? DeclarationProviderHelper.GetDefaultCurrencyCode(CusEntryLine, TRIncotermChargeCodeList.Codes.ROY) : ZString.Empty;
		public string OverseasInterestCurrency => !IsExport ? DeclarationProviderHelper.GetDefaultCurrencyCode(CusEntryLine, TRIncotermChargeCodeList.Codes.INT) : ZString.Empty;
		public string OverseasOtherCurrency => !IsExport ? DeclarationProviderHelper.GetDefaultCurrencyCode(CusEntryLine, TRIncotermChargeCodeList.Codes.OTH) : ZString.Empty;
		public string OverseasOtherDescription => !IsExport ? DeclarationProviderHelper.GetChargesExplanation(CusEntryLine, TRIncotermChargeCodeList.Codes.OTH) : ZString.Empty;

		#endregion

		public string QualificationOfLine => RandomInvoiceLine.JI_ValuationCode;
		public string EntranceAndExitPurpose => RandomInvoiceLine.ZG_EntryExitPurposeCode;
		public string EntranceAndExitPurposeDescription => RandomInvoiceLine.ZG_EntryExitPurposeDetail;
		public string STMCityCode => RandomInvoiceLine.ZG_RW_NKBorderTradeStateCode;
		public string GoodsComeBackReason => RandomInvoiceLine.ZG_ReturningGoodsReasonCode;
		public string GoodsComeBackReasonDescription => RandomInvoiceLine.ZG_ReturningGoodsReasonDetail;
		public string OrderType => RandomInvoiceLine.InvoiceHeader.JobDeclaration.JE_ExportGoodsType;

		#region Export Union Fields

		public IExportUnionItems ExportUnionItems => MessageType == TRMessageTypes.Codes.EUT ? new ExportUnionItemsProvider(CusEntryLine, EffectiveDate) : null;

		#endregion
	}
}
