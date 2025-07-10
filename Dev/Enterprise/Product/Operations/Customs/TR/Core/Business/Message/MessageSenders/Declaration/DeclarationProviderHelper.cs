using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.Integration.ZArchitecture;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Business
{
	public static class DeclarationProviderHelper
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Regex pattern")]
		public static ZString GetCustomsRatioCode(ZBool isExport, CodeDescriptionPairList taxOrFeeCodeList, ZString taxType)
		{
			var vatRatioCode = ZString.Empty;
			var pattern = @"\d+";

			if (!isExport && taxOrFeeCodeList.Count > 1 && !taxType.IsEmpty)
			{
				var desc = taxOrFeeCodeList.GetDescriptionFromCode(taxType);
				if (desc == null)
				{
					return vatRatioCode;
				}

				var match = Regex.Match(desc, pattern);
				if (match.Success)
				{
					vatRatioCode = CusEntryMessageConstants.CountryConstants.VATCode + match.Value;
				}
			}

			return vatRatioCode;
		}

		public static ZDecimal GetContainerCount(CusEntryLine cusEntryLine)
		{
			return cusEntryLine.InvoiceLines.Cast<JobComInvoiceLine>().SelectMany((JobComInvoiceLine x) => x.ContainersPivot)
								.Cast<Customs.Business.CusContainerInvoiceLinePivot>().GroupBy(x => x.ContainerNumber).Count();
		}

		#region Charge Methods

		public static ZString GetDefaultCurrencyCode(JobComInvoiceHeader invoice, params ZString[] chargeTypes)
		{
			return FindFromValidCharges(invoice, chargeTypes).FirstOrDefault()?.J7_RX_NKCurrency ?? ZString.Empty;
		}

		public static ZString GetDefaultCurrencyCode(CusEntryLine cusEntryLine, params ZString[] chargeTypes)
		{
			return FindFromValidCharges(cusEntryLine, chargeTypes).FirstOrDefault()?.J7_RX_NKCurrency ?? ZString.Empty;
		}

		public static ZDecimal GetChargesTotal(JobComInvoiceHeader invoice, params ZString[] chargeTypes)
		{
			var result = ZDecimal.Zero;

			var charges = FindFromValidCharges(invoice, chargeTypes);
			if (charges.Length > 0)
			{
				var converter = invoice.CurrencyConverter;
				var sumInLocal = Money.Empty;

				foreach (var charge in charges)
				{
					sumInLocal = converter.Add(sumInLocal, charge.Money);
				}
				if (!sumInLocal.IsEmpty)
				{
					result = converter.ConvertExact(sumInLocal, charges[0].Currency).Amount;
				}
			}

			return result.RoundAmount();
		}

		public static ZDecimal GetChargesTotal(CusEntryLine cusEntryLine, ZBool convertLocalCurrency, params ZString[] chargeTypes)
		{
			var charges = FindFromValidCharges(cusEntryLine, chargeTypes);
			if (charges.Length <= 0)
			{
				return ZDecimal.Zero;
			}

			var defaultInvoice = cusEntryLine.Declaration.Invoices.Cast<JobComInvoiceHeader>().Single();
			var converter = defaultInvoice.CurrencyConverter;
			var sumInLocal = Money.Empty;

			foreach (var charge in charges)
			{
				sumInLocal = converter.Add(sumInLocal, charge.Money);
			}

			var result = ZDecimal.Zero;
			if (!sumInLocal.IsEmpty)
			{
				if (convertLocalCurrency)
				{
					result = converter.ConvertExact(sumInLocal, converter.LocalCurrency).Amount;
				}
				else
				{
					result = converter.ConvertExact(sumInLocal, charges[0].Currency).Amount;
				}
			}

			return result.RoundAmount();
		}

		public static InvoiceCharge[] FindFromValidCharges(JobComInvoiceHeader invoice, params ZString[] chargeTypes)
		{
			return invoice.Charges.Find(charge => chargeTypes.Contains(charge.J7_ChargeType) && !charge.Money.IsEmpty).ToArray();
		}

		public static JobComInvCharge[] FindFromValidCharges(CusEntryLine cusEntryLine, params ZString[] chargeTypes)
		{
			var allCharges = Enumerable.Empty<JobComInvCharge>();
			allCharges = allCharges.Union(cusEntryLine.InvoiceLines.SelectMany(l => ((JobComInvoiceLine)l).Charges.Cast<JobComInvCharge>().Where(x => chargeTypes.Contains(x.J7_ChargeType))));
			allCharges = allCharges.Union(cusEntryLine.InvoiceLines.SelectMany(l => ((JobComInvoiceLine)l).ApportionedCharges.Cast<JobComInvCharge>().Where(x => chargeTypes.Contains(x.J7_ChargeType))));
			return allCharges.ToArray();
		}

		public static ZString GetChargesExplanation(CusEntryLine cusEntryLine, ZString chargeType)
		{
			var explanation = cusEntryLine.InvoiceLines.SelectMany(l => ((JobComInvoiceLine)l).Charges.Cast<InvoiceLineCharge>().Where(x => x.J7_ChargeType == chargeType)).FirstOrDefault()?.Explanation;
			return explanation ?? ZString.Empty;
		}

		#endregion

		public static ZBool Check8ThousandCodes(ZString procedureCode)
		{
			return procedureCode == CusEntryMessageConstants.ProcedureCodes._8000 || procedureCode == CusEntryMessageConstants.ProcedureCodes._8100 || procedureCode == CusEntryMessageConstants.ProcedureCodes._8200;
		}

		#region Round Methods

		public static decimal RoundAmount(this decimal amountValue)
		{
			var decValue = (ZDecimal)amountValue;
			return decValue.RoundAmount();
		}

		public static ZDecimal RoundAmount(this ZDecimal amountValue)
		{
			return amountValue.Round(2);
		}

		public static ZDecimal RoundExchangeAmount(this ZDecimal amountValue)
		{
			return amountValue.Round(6);
		}

		public static decimal RoundPerceptionQuantity(this decimal quantity)
		{
			return quantity.Round(4);
		}

		public static ZDecimal RoundWeight(this ZDecimal quantity)
		{
			return quantity.Round(4);
		}

		public static ZString RoundRatio(this ZString decimalStringVelue)
		{
			var ratioString = ZString.Empty;
			var ratioValue = ZDecimal.Zero;
			if (ZDecimal.TryParse(decimalStringVelue, out var decimalValue))
			{
				var decimal2Digit = decimalValue.Round(2);
				var decimal3Digit = decimalValue.Round(3);
				ratioValue = decimal2Digit == decimal3Digit ? decimal2Digit : decimal3Digit;
			}

			if (ratioValue % 1 == 0)
			{
				ratioString = ((int)ratioValue).ToString();
			}
			else
			{
				if (TRCustomsDataRegistry.Instance.IsTRTestingSystem && TRCustomsDataRegistry.Instance.SendTRDeclarationWithComma.Value)
				{
					ratioString = ratioValue.ToString("N2", CultureInfo.CreateSpecificCulture(Core.SharedConstants.Languages.Turkish));
				}
				else
				{
					ratioString = ratioValue.ToString();
				}
			}

			return ratioString;
		}

		#endregion

		public static ZString GetMailAddress(ZInt order) =>
			GlbStaff.CurrentUser.EmailAddresses.Cast<GlbStaffEmailAddress>().Where(x => !x.GSE_EmailAddress.IsEmpty)
			.OrderBy(email => email.GSE_Type == Core.Constants.EmailFromAddressTypes.Codes.Main ? ZString.Empty : email.GSE_EmailAddress)
			.Skip(order - 1).FirstOrDefault()?.GSE_EmailAddress ?? ZString.Empty;

		public static ZString GetCurrency(ZDecimal totalValue, ICurrency currency)
		{
			if (totalValue.IsEmpty)
			{
				return ZString.Empty;
			}

			return currency?.Code ?? ZString.Empty;
		}
	}
}
