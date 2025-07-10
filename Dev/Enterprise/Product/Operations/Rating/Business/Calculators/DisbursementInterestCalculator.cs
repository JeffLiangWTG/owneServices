using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.DataTransfer.Ratings;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.DocumentPrinting.DocRollUpSort;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	[CalculatorProperty("CreditTerms", IsCalculatedField = true, MapTo = "String1", RelatedTo = "CreditTerms")]
	[CalculatorProperty("CurrentPrimeRate", IsCalculatedField = true, MapTo = "Decimal1", RelatedTo = "CurrentPrimeRate")]
	[CalculatorProperty(DisbursementInterestCalculator.Items.Uplift, RateLineItem.Schema.TM_RelevantValue, IsMandatory = true)]
	[CalculatorProperty("Uplift", IsCalculatedField = true, MapTo = "Decimal2", RelatedTo = "Uplift")]
	[CalculatorProperty("EffectiveRate", IsCalculatedField = true, MapTo = "Decimal3", RelatedTo = "EffectiveRate")]
	[CalculatorProperty(DisbursementInterestCalculator.Items.AdjustmentDays, RateLineItem.Schema.TM_RelevantValue, IsMandatory = true, MapTo = "Decimal4", RelatedTo = "AdjustmentDays")]
	[CalculatorProperty(DisbursementInterestCalculator.Items.OutstandingDays, RateLineItem.Schema.TM_Text, IsMandatory = true, MapTo = "Bool1", RelatedTo = "OutstandingDays")]
	[CalculatorProperty(CalculatorConstants.Text.IncludeGST, RateLineItem.Schema.TM_Text, IsMandatory = true, MapTo = "Bool2", RelatedTo = "IncludeGST")]
	public class DisbursementInterestCalculator : BasePercentageCalculator
	{
		public DisbursementInterestCalculator(IRateLine master)
			: base(master)
		{
		}

		public const string Code = RatingCalculatorCodes.DisbursementInterest;

		#region Properties

		public ZBool IncludeGST
		{
			get { return (ZBool)this[CalculatorConstants.Text.IncludeGST]; }
			set { this[CalculatorConstants.Text.IncludeGST] = value; }
		}

		public ZBool OutstandingDays
		{
			get { return (ZBool)this[Items.OutstandingDays]; }
			set { this[Items.OutstandingDays] = value; }
		}

		public ZDecimal AdjustmentDays
		{
			get { return (ZDecimal)this[Items.AdjustmentDays]; }
			set { this[Items.AdjustmentDays] = value; }
		}

		#region CreditTerms

		public ZString CreditTerms
		{
			get
			{
				ZString result;

				if (ParentRatingHeader.Header == null || ParentRateEntry.IsCompanyTariff())
				{
					result = Res.GetString("ddb142ee-1d64-406c-9975-4673d03abbe6", "Taken From Invoiced Party.");
				}
				else
				{
					var invoiceTerm = GetInvoiceTerm(ParentRatingHeader.Header);
					var days = invoiceTerm.Days;

					if (OutstandingDays)
					{
						result = Res.GetString("8fb87b81-9c93-40d8-b609-4f3df60e2d05", "{0} Outstanding Days", days + AdjustmentDays);
					}
					else
					{
						var creditTermsTypeDescription = invoiceTerm.TermDescription;
						result = days == 0
							? creditTermsTypeDescription.ToString()
							: (invoiceTerm.IsTermWithMonths
								? Res.GetString("FE292207-3907-40F5-93A4-EAECA336C937", "{0} Months {1}", days, creditTermsTypeDescription)
								: Res.GetString("d2f9524c-b0b1-4fa1-bf17-8f3bc89e83be", "{0} Days {1}", days, creditTermsTypeDescription));
					}
				}

				return result;
			}
		}

		public ZPropertyInfo CreditTermsInfo
		{
			get { return ((IGetZPropertyInfo)RateLineBizO).GetZPropertyInfo("Calculator+String1"); }
		}

		InvoiceTerm GetInvoiceTerm(OrgHeader header)
		{
			if (ParentRatingHeader is Costing)
			{
				return header.CompanyData.GetAPTerm();
			}
			else
			{
				var rateType = ParentRateEntry.RateType();

				JobInvoicingConsumerType jobType = null;
				switch (rateType)
				{
					case RateType.Forwarding:
						jobType = JobInvoicingConsumerTypes.Shipment;
						break;
					case RateType.CFS:
						jobType = JobInvoicingConsumerTypes.CFSShipment;
						break;
					case RateType.Warehouse:
						jobType = JobInvoicingConsumerTypes.WarehouseStorage;
						break;
					case RateType.TransportBookings:
						jobType = JobInvoicingConsumerTypes.TransportBooking;
						break;
					case RateType.Shipping:
						jobType = JobInvoicingConsumerTypes.AgencyBillOfLading;
						break;
					case RateType.ShippingImportDetention:
						jobType = JobInvoicingConsumerTypes.AgencyDetentionInvoice;
						break;
					case RateType.ShippingExportDetention:
						jobType = JobInvoicingConsumerTypes.AgencyDetentionInvoice;
						break;
					case RateType.LocalTransport:
						jobType = JobInvoicingConsumerTypes.LocalCartage;
						break;
					default:
						break;
				}

				var serviceDirection = OrgConstants.ServiceDirection.Code.Unknown;
				switch (ParentRateEntry.JobDirection)
				{
					case Directions.CrossTrade:
						serviceDirection = OrgConstants.ServiceDirection.Code.CrossTrade;
						break;
					case Directions.Domestic:
						serviceDirection = OrgConstants.ServiceDirection.Code.Domestic;
						break;
					case Directions.Export:
						serviceDirection = OrgConstants.ServiceDirection.Code.Export;
						break;
					case Directions.Import:
						serviceDirection = OrgConstants.ServiceDirection.Code.Import;
						break;
					default:
						break;
				}

				return header.CompanyData.GetDisbursementARTerm(jobType,
																serviceDirection,
																RatingConstants.GetTransportModeFromMode(ParentRateEntry.TI_Mode),
																ZGuid.Empty,
																ZGuid.Empty);
			}
		}

		#endregion

		#region CurrentPrimeRate

		public ZDecimal CurrentPrimeRate
		{
			get { return ObjectFactory.Get<IAccounting>().CurrentPrimeRate; }
		}

		public ZPropertyInfo CurrentPrimeRateInfo
		{
			get { return RateLineBizO.CurrentPrimeRateInfo; }
		}

		#endregion

		#region Uplift

		public ZDecimal Uplift
		{
			get { return (ZDecimal)this[DisbursementInterestCalculator.Items.Uplift]; }
			set
			{
				this[DisbursementInterestCalculator.Items.Uplift] = value;
				EffectiveRateInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo UpliftInfo
		{
			get
			{
				var upliftItem = FindRateLineItem(DisbursementInterestCalculator.Items.Uplift);
				return upliftItem == null ? DummyRateLineItem.TM_RelevantValueInfo : upliftItem.TM_RelevantValueInfo;
			}
		}

		#endregion

		#region EffectiveRate

		public ZDecimal EffectiveRate
		{
			get { return CurrentPrimeRate + Uplift; }
		}

		public ZPropertyInfo EffectiveRateInfo
		{
			get { return RateLineBizO.EffectiveRateInfo; }
		}

		#endregion

		#endregion

		#region Quotation Lines

		protected override QuotationLineList GetQuotationLinesInternal(GetQuotationLinesParam flags)
		{
			var result = new QuotationLineList();

			var additionalDescription = CreditTermText(CreditTerms);

			if (!Uplift.IsEmpty)
			{
				result.Add(QuotationLine.NewWithValue(Line, RateDescriptionFlags(flags) | QuotationLineType.NoCurrency | QuotationLineType.MergeWithRateLineDescription, Uplift, additionalDescription, PercentagePrimeRateText));
			}
			else
			{
				result.Add(QuotationLine.New(Line, RateDescriptionFlags(flags) | QuotationLineType.NoCurrency | QuotationLineType.MergeWithRateLineDescription, additionalDescription, (NoResString)ZString.Empty, AtPrimeRateText));
			}

			return result;
		}

		static string CreditTermText(string creditTerms) => Res.GetString("856a1fe5-9257-4bdf-93ae-8e40cc8a9088", "- Credit Terms: {0}", creditTerms);

		static ResourceString PercentagePrimeRateText => ResString.GetMultilingualString("7ac21c5f-9b2d-4e02-b370-37736906c51c", "% + Prime Rate");

		static ResourceString PercentagePrimeRateDocLineAmountText => ResString.GetMultilingualString("7FE66579-63DF-4B4B-A819-AB5AD76A58B1", "% Plus Prime Rate");

		static ResourceString AtPrimeRateText => ResString.GetMultilingualString("47e55859-ff1a-414d-8464-1f1cc02678f8", "At Prime Rate");

		public override DocLineAmount GetDocLineAmount()
		{
			var result = new DocLineAmount();

			var additionalDescription = CreditTermText(CreditTerms);
			result.SetPercentage(Line.TL_RX_NKCurrency, CreditTermText(CreditTerms), PercentagePrimeRateDocLineAmountText, Uplift);

			return result;
		}

		#endregion

		#region Calculation

		protected override void CalculateInternal(CalculatorOutput calcOutput)
		{
			var parameters = calcOutput.Parameters;
			var criteria = parameters.Criteria;

			OrgHeader orgForTerms = null;
			if (ParentRatingHeader.IsCosting())
			{
				orgForTerms = ParentRatingHeader.Header ?? criteria.Creditors[Line.ChargeCode.AC_ChargeGroup].FirstOrDefault()?.Org;

				if (orgForTerms == null)
				{
					calcOutput.FailureMessage = ZString.Format("{0}: {1}", Line.ChargeCode.AC_Code, Res.GetString("52F0A271-FF95-499F-A197-994D4C0B8B66", "Cannot calculate disbursement interest because Service Provider could not be established"));
					return;
				}
			}
			else
			{
				if (criteria.Job != null)
				{
					orgForTerms = parameters.Factory.Load<OrgHeader>(criteria.GetDebtorPK(Line.ChargeCode));
				}

				if (orgForTerms == null)
				{
					calcOutput.FailureMessage = ZString.Format("{0}: {1}", Line.ChargeCode.AC_Code, Res.GetString("d2488a31-7059-4528-b886-ca78bdb7089b", "Cannot calculate disbursement interest because Debtor could not be established"));
					return;
				}
			}

			var monetaryValue = this.GetValueCalculatorAppliesTo(parameters, out _, IncludeGST, false, false);
			var invoiceTerms = GetInvoiceTerm(orgForTerms);
			var termType = invoiceTerms.Term;
			ZInt termDays = (int)invoiceTerms.Days;
			ZInt effectiveDays;

			if (OutstandingDays)
			{
				effectiveDays = (int)(termDays + AdjustmentDays);
			}
			else
			{
				var effectiveInterestDaysResult = GetEffectiveInterestDays(parameters, termType, termDays);

				if (!effectiveInterestDaysResult.Success)
				{
					calcOutput.FailureMessage = effectiveInterestDaysResult.ErrorMessage;
					return;
				}

				effectiveDays = (int)(effectiveInterestDaysResult.EffectiveDays + AdjustmentDays);
			}

			var rate = (Uplift + CurrentPrimeRate) / 100 * ((decimal)effectiveDays / 365);

			var currency = RefCurrency.LoadFromCurrencyCode(Line.Factory, monetaryValue.Unit);
			var currencyCode = currency?.Code;

			var calcDescription = DescriptionForDisbursementInterestCalc(orgForTerms, invoiceTerms, termDays, effectiveDays);

			var rateInfo = RateInfo.CreateUNT(rate, currencyCode, currencyCode, null, calcDescription, unitMultiplier: UnitMultiplier);
			var chargeableDescription = this.GetChargeableDescription(null, monetaryValue);
			var basis = criteria.CreatePaymentBasis(rateInfo, monetaryValue, null, chargeableDescription);
			calcOutput.Add(basis);
		}

		#region Description

		ZString DescriptionForDisbursementInterestCalc(OrgHeader orgForTerms, InvoiceTerm invoiceTerms, ZInt termDays, ZInt effectiveDays)
		{
			ZString desc = (Uplift + CurrentPrimeRate).ToString("G29", Culture.CurrentCompanyCountryCulture) + " " + Res.GetString("86becd30-ab23-4ffd-a4b5-0f21c8f99b0f", "% pa - {0} Days", termDays) + " ";

			if (AdjustmentDays > 0)
			{
				desc += Res.GetString("8bef4ef4-c134-4e72-a13a-42443f17a201", "+ {0} Adjustment Days", AdjustmentDays.ToString("G29", Culture.CurrentCompanyCountryCulture)) + " ";
			}

			if (OutstandingDays)
			{
				desc += Res.GetString("e93d8de1-9d3b-47b1-bae3-9d54ba2c7f90", "({0} effective outstanding days)", effectiveDays);
			}
			else
			{
				var creditTermsTypeDescription = invoiceTerms.TermDescription;
				desc += Res.GetString("3a10134e-51ca-4c6a-9b37-a398e03cee57", "{0} ({1} effective days)", creditTermsTypeDescription, effectiveDays);
			}

			desc += " " + Res.GetString("f7ee6217-2ec9-4d4d-ae38-a03c1bfa0f5d", "For {0}", orgForTerms.OH_FullNameTruncated);

			return desc;
		}

		#endregion

		#region Effective Interest Days

		class GetEffectiveInterestDaysOut
		{
			public bool Success { get; set; }
			public ZInt EffectiveDays { get; set; }
			public string ErrorMessage { get; set; }
		}

		GetEffectiveInterestDaysOut GetEffectiveInterestDays(AutoRatingCalculatorParameters parameters, ZString termType, ZInt creditTermDays)
		{
			var result = new GetEffectiveInterestDaysOut()
			{
				Success = true,
				EffectiveDays = 0,
				ErrorMessage = ""
			};

			switch (termType)
			{
				case Core.Constants.InvoiceTerms.FromInvoiceDate:
					result.EffectiveDays = creditTermDays;
					return result;

				case Core.Constants.InvoiceTerms.FromMonthEnd:
					var daysInMonth = DateTime.DaysInMonth(ZDateTime.Now.Year, ZDateTime.Now.Month);
					var daysLeftInMonth = daysInMonth - ZDateTime.Now.Day;
					result.EffectiveDays = daysLeftInMonth + creditTermDays;
					return result;

				case Core.Constants.InvoiceTerms.FromPeriodEnd:
					var periodCalc = new AccountingPeriodCalculator(Line.Factory);
					var lastDayOfPeriod = periodCalc.GetLastDayForPeriod(ZDateTime.Today);
					var daysLeftInPeriod = lastDayOfPeriod.IsValid ? (lastDayOfPeriod - ZDateTime.Today).Days : 0;
					result.EffectiveDays = daysLeftInPeriod + creditTermDays;
					return result;

				case Core.Constants.InvoiceTerms.FromShipmentDate:
					var arrivalDate = parameters.Criteria.JobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate);
					if (arrivalDate.IsEmpty)
					{
						result.Success = false;
						result.EffectiveDays = 0;
						result.ErrorMessage = Res.GetString("b1411254-0578-46d3-b342-c883f97bb8a2", "Cannot calculate disbursement interest because job arrival date is empty and it is impossible to establish the number of effective interest days.");
						return result;
					}
					else
					{
						var difference = ZDateTime.Today > arrivalDate ? ZDateTime.Today - arrivalDate : arrivalDate - ZDateTime.Today;
						result.EffectiveDays = difference.Days + creditTermDays;
						return result;
					}
			}

			result.Success = true;
			result.EffectiveDays = 0;
			return result;
		}

		#endregion

		#endregion

		public override bool CanBePrintedUsing6StandardOperatorColumnHeaders => false;

		public override bool IsMeasureTypeMatchApplicable => false;

		#region Logging

		protected override void RunActionsOnMasterSavingCore()
		{
			var header = RateLineBizO?.Parent?.Parent;
			if (header == null || !RateLineBizO.HasChanges)
			{
				return;
			}

			var rateType = header.RateTypeSafe();
			if (rateType == RatingConstants.RatingHeaderTypes.ClientRate ||
				rateType == RatingConstants.RatingHeaderTypes.Quote ||
				rateType == RatingConstants.RatingHeaderTypes.Tariff)
			{
				#region SuppressResourceStringsCheckRegion
				List<KeyValuePair<string, string>> keyValuePairs = null;
				keyValuePairs = BuildApplyToChange(keyValuePairs, "APP");
				keyValuePairs = BuildInfoChange(keyValuePairs, "CON", RateLineBizO.TL_ConditionInfo, 6);
				keyValuePairs = BuildInfoChange(keyValuePairs, "CRD", Decimal4Info, 16);
				keyValuePairs = BuildInfoChange(keyValuePairs, "CUR", RateLineBizO.TL_RX_NKCurrencyInfo, 7);
				keyValuePairs = BuildEffectiveRateChange(keyValuePairs, "EF%");
				keyValuePairs = BuildInfoChange(keyValuePairs, "OSD", Bool1Info, 1);
				keyValuePairs = BuildInfoChange(keyValuePairs, "VAT", Bool2Info, 1);
				#endregion

				if (keyValuePairs?.Count > 0)
				{
					RateLineBizO.Header.Logs.AddNew(AutoEvents.CreditControlsModified, Code, keyValuePairs.ToArray());
				}
			}
		}

		const string semicolon = ";";
		const string ellipsis = "...";

		List<KeyValuePair<string, string>> BuildApplyToChange(List<KeyValuePair<string, string>> keyValuePairs, string key)
		{
			var changedText = ApplyToRateLineItems.Cast<RateLineItem>()
				.Where(c => !c.TM_Text.IsEmpty && c.TM_TextInfo.HasChangesOrIsNew())
				.Select(c => (string)c.TM_Text);
			if (changedText.Any())
			{
				var text = string.Join(semicolon, changedText);
				keyValuePairs = BuildChange(keyValuePairs, key, text, 51);
			}

			return keyValuePairs;
		}

		List<KeyValuePair<string, string>> BuildEffectiveRateChange(List<KeyValuePair<string, string>> keyValuePairs, string key)
		{
			if (Decimal1Info.HasChangesOrIsNew() || Decimal2Info.HasChangesOrIsNew())
			{
				var text = EffectiveRate.ToString("G29", Culture.CurrentCompanyCountryCulture);
				keyValuePairs = BuildChange(keyValuePairs, key, text, 12);
			}

			return keyValuePairs;
		}

		List<KeyValuePair<string, string>> BuildInfoChange(List<KeyValuePair<string, string>> keyValuePairs, string key, ZPropertyInfo propertyInfo, int maxLength)
		{
			if (propertyInfo.HasChangesOrIsNew())
			{
				keyValuePairs = BuildChange(keyValuePairs, key, propertyInfo.Value.ToString(), maxLength);
			}

			return keyValuePairs;
		}

		List<KeyValuePair<string, string>> BuildChange(List<KeyValuePair<string, string>> keyValuePairs, string key, string text, int maxLength)
		{
			if (!string.IsNullOrEmpty(text))
			{
				keyValuePairs = keyValuePairs ?? new List<KeyValuePair<string, string>>();
				keyValuePairs.Add(new KeyValuePair<string, string>(key, SubStringWithEllipsisIfNeed(text, maxLength)));
			}
			return keyValuePairs;
		}

		string SubStringWithEllipsisIfNeed(string str, int maxLength)
		{
			if (maxLength > 3 && str.Length > maxLength)
			{
				str = str.Substring(0, maxLength - 3) + ellipsis;
			}

			return str;
		}

		#endregion
	}

	public static class ZPropertyInfoExtend
	{
		public static bool HasChangesOrIsNew(this ZPropertyInfo info)
		{
			return info.HasChanges || (!info.Value.IsEmpty && info.BizObj != null && !info.BizObj.IsInDatabase);
		}
	}
}

