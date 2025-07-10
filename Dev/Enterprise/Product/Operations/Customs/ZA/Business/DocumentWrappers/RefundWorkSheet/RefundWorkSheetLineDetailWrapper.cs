using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers
{
	internal class RefundWorkSheetLineDetailWrapper : DA63LineDetailWrapper
	{
		public RefundWorkSheetLineDetailWrapper(CusEntryLine line, RefundWorkSheetDocumentWrapper parentWrapper) : base(line)
		{
			parent = parentWrapper;
		}

		#region Properties for Document
		public ZInt PreviousEntryLineNumber => EntryLine.PreviousEntryLineNumber;

		public ZDecimal UnitPricePerSuppliersInvoice
		{
			get
			{
				var q = DA63CustomsQuantity;
				return q == ZDecimal.Zero ? ZDecimal.Zero : new ZDecimal(FobValue / q).Round(2);
			}
		}

		public bool RefundWorkSheetRebatesUsed => EntryLine.In(EntryLine.Header.MergedLines) && (EntryLine.InvoiceLines?.OfType<JobComInvoiceLine>()?.Any(x => x.CusLineTariffDetails.Cast<CusLineTariffDetail>().Any(xy => xy.BZ_Type.Left(2) == "5P" && xy.BZ_Tariff.Left(5) == "52202" || xy.BZ_Tariff.Left(5) == "52203")) ?? false);
		public ZString ProductCodeOrDA63Description
		{
			get
			{
				if (!EntryLine.RandomLine.JI_PartNo.IsEmpty)
				{
					return EntryLine.RandomLine.JI_PartNo;
				}
				return EntryLine.RandomLine.JI_Description;
			}
		}

		public ZString DA63CustomsValueString => DA63CustomsValue.Round(0).ToString();

		public ZString DA63CustomsQuantityString => DA63CustomsQuantity.Round(2).ToString();

		public ZDecimal ConversionFactor => EntryLine.RandomLine?.JI_ConversionFactor ?? ZDecimal.Zero;

		public ZString ConversionFactorAsString
		{
			get
			{
				var cf = ConversionFactor;
				return cf == ZDecimal.Zero ? "-" : cf.ToString();
			}
		}

		public ZDecimal FobValue
		{
			get
			{
				var cf = ConversionFactor;
				return cf == ZDecimal.Zero ? ZDecimal.Zero : new ZDecimal(DA63CustomsValue / cf).Round(2);
			}
		}

		public ZString DivideOrMultiply
		{
			get
			{
				return "x";
			}
		}

		public ZDecimal OtherDA63Duties => EntryLine.OtherDA63Duties;

		public ZDecimal TotalAmountClaimed => DA63TotalAmountClaimed;
		#endregion

		public ZString FirstOtherDutyTypeForRefundSheet => !FirstOtherDutyAmountForRefundSheet.IsEmpty ? parent.FirstOtherTaxType : ZString.Empty;

		public ZString FirstOtherDutyAmountForRefundSheet => GetAmountOFDutyForThisTaxType(parent.FirstOtherTaxType);

		public ZString SecondOtherDutyTypeForRefundSheet => !SecondOtherDutyAmountForRefundSheet.IsEmpty ? parent.SecondOtherTaxType : ZString.Empty;

		public ZString SecondOtherDutyAmountForRefundSheet => GetAmountOFDutyForThisTaxType(parent.SecondOtherTaxType);

		string GetAmountOFDutyForThisTaxType(ZString taxTypeToFind)
		{
			var value = Others.FirstOrDefault(d => d.Code.Equals(taxTypeToFind))?.Value;
			return value.HasValue ? value.Value.ToString(2) : string.Empty;
		}

		public IEnumerable<ZString> TaxTypes
		{
			get
			{
				yield return FirstOtherDutyTaxType;
				yield return SecondOtherDutyTaxType;
				yield return ThirdOtherDutyTaxType;
				yield return FourthOtherDutyTaxType;
			}
		}

		readonly RefundWorkSheetDocumentWrapper parent;
	}
}
