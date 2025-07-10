using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business.MessageBuilders;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers
{
	class DA63LineDetailWrapper : NonPersistentBusinessObject, IDocumentWrapper
	{
		public DA63LineDetailWrapper(CusEntryLine line) : base(line?.Factory ?? new BusinessObjectFactory())
		{
			EntryLine = line;
		}

		#region Related BO

		internal CusEntryLine EntryLine { get; }

		internal ILineLevelInformation LineLevelInformationProvider => EntryLine;

		#endregion

		#region Properties for Document

		public ZString LineNumber => LineLevelInformationProvider?.LineNumber.TrimStart(' ', '0') ?? ZString.Empty;

		public ZString DA63LineNumber { get; set; }

		public ZString PreviousProcedureMRN => LineLevelInformationProvider?.PreviousProcedureMRN ?? ZString.Empty;

		public ZString AlphaOfficeCode => PreviousProcedureMRN.Left(3);

		public ZString PreviousMRNLineNumber => LineLevelInformationProvider?.WarehousingMRNLineNumber ?? ZString.Empty;

		public ZString CountryOfOrigin => LineLevelInformationProvider?.CountryOfOrigin ?? ZString.Empty;

		public ZString DA63ImportTariffCode
		{
			get
			{
				return DocumentWrapperHelper.FormatTariffCode(EntryLine?.RandomLine?.ImportTariff?.GetTariffCodeWithCheckDigit() ?? ZString.Empty);
			}
		}

		#region Quantity and Units

		public ZDecimal DA63CustomsQuantity
		{
			get
			{
				if (!da63CustomsQuantity.HasValue)
				{
					GetCalcFeeValues();
				}
				return da63CustomsQuantity.Value;
			}
		}
		ZDecimal? da63CustomsQuantity;

		public ZString DA63CustomsUnitQty
		{
			get
			{
				if (!da63CustomsUnitQty.HasValue)
				{
					GetCalcFeeValues();
				}
				return da63CustomsUnitQty.Value;
			}
		}
		ZString? da63CustomsUnitQty;

		public ZDecimal DA63AdditionalQuantity1
		{
			get
			{
				if (!da63AdditionalQuantity1.HasValue)
				{
					GetCalcFeeValues();
				}
				return da63AdditionalQuantity1.Value;
			}
		}
		ZDecimal? da63AdditionalQuantity1;

		public ZString DA63AdditionalUnitQty1
		{
			get
			{
				if (!da63AdditionalUnitQty1.HasValue)
				{
					GetCalcFeeValues();
				}
				return da63AdditionalUnitQty1.Value;
			}
		}
		ZString? da63AdditionalUnitQty1;

		public ZDecimal DA63AdditionalQuantity2
		{
			get
			{
				if (!da63AdditionalQuantity2.HasValue)
				{
					GetCalcFeeValues();
				}
				return da63AdditionalQuantity2.Value;
			}
		}
		ZDecimal? da63AdditionalQuantity2;

		public ZString DA63AdditionalUnitQty2
		{
			get
			{
				if (!da63AdditionalUnitQty2.HasValue)
				{
					GetCalcFeeValues();
				}
				return da63AdditionalUnitQty2.Value;
			}
		}
		ZString? da63AdditionalUnitQty2;

		#endregion

		#region DA63 Amounts

		public ZDecimal DA63CustomsValue
		{
			get
			{
				if (!da63CustomsValue.HasValue)
				{
					GetCalcFeeValues();
				}
				return da63CustomsValue.Value;
			}
		}
		ZDecimal? da63CustomsValue;

		CalcFeeValues? da63CalcFeeValues;
		public ZDecimal DA63CustomsDutyExcluding12B
		{
			get
			{
				if (!da63CalcFeeValues.HasValue)
				{
					GetCalcFeeValues();
				}
				return da63CalcFeeValues.Value.CustomsDutyExcluding12B;
			}
		}

		public ZDecimal DA63S1P2BDuty
		{
			get
			{
				if (!da63CalcFeeValues.HasValue)
				{
					GetCalcFeeValues();
				}
				return da63CalcFeeValues.Value.S1P2BDuty;
			}
		}

		public ZDecimal DA63ValueAddedTax
		{
			get
			{
				if (!da63CalcFeeValues.HasValue)
				{
					GetCalcFeeValues();
				}
				return da63CalcFeeValues.Value.ValueAddedTax;
			}
		}

		internal ZDecimal DA63TotalAmountClaimed
		{
			get
			{
				if (!da63CalcFeeValues.HasValue)
				{
					GetCalcFeeValues();
				}
				var calcValues = da63CalcFeeValues.Value;
				return calcValues.CustomsDutyExcluding12B
					+ calcValues.S1P2BDuty
					+ calcValues.ValueAddedTax
					+ calcValues.ProvisionalPayment
					+ calcValues.Penalty
					+ TotalOtherAmount;
			}
		}

		public ZDecimal TotalOtherAmount => FirstOtherDutyAmount + SecondOtherDutyAmount + ThirdOtherDutyAmount + FourthOtherDutyAmount;

		ZDecimal FirstOtherDutyAmount => Others.ElementAtOrDefault(0)?.Value ?? ZDecimal.Zero;

		public ZString FirstOtherDutyAmountStr => FirstOtherDutyAmount.ToString(2);

		public ZString FirstOtherDutyTaxType => Others.ElementAtOrDefault(0)?.Code ?? ZString.Empty;

		ZDecimal SecondOtherDutyAmount => Others.ElementAtOrDefault(1)?.Value ?? ZDecimal.Zero;

		public ZString SecondOtherDutyAmountStr => SecondOtherDutyAmount.ToString(2);

		public ZString SecondOtherDutyTaxType => Others.ElementAtOrDefault(1)?.Code ?? ZString.Empty;

		ZDecimal ThirdOtherDutyAmount => Others.ElementAtOrDefault(2)?.Value ?? ZDecimal.Zero;

		public ZString ThirdOtherDutyAmountStr => ThirdOtherDutyAmount.ToString(2);

		public ZString ThirdOtherDutyTaxType => Others.ElementAtOrDefault(2)?.Code ?? ZString.Empty;

		ZDecimal FourthOtherDutyAmount => Others.ElementAtOrDefault(3)?.Value ?? ZDecimal.Zero;

		public ZString FourthOtherDutyAmountStr => FourthOtherDutyAmount.ToString(2);

		public ZString FourthOtherDutyTaxType => Others.ElementAtOrDefault(3)?.Code ?? ZString.Empty;

		internal IEnumerable<IDutyFeeInformation> Others
		{
			get
			{
				if (!da63CalcFeeValues.HasValue)
				{
					GetCalcFeeValues();
				}
				var calcValues = da63CalcFeeValues.Value;
				var excluding12B = calcValues.CustomsDutiesExcluding12B
					.Concat(calcValues.Penalties)
					.Concat(calcValues.ProvisionalPayments)
					.OrderBy(x => x.Code);

				return excluding12B;
			}
		}

		#endregion

		public ZString GoodsDescription => LineLevelInformationProvider?.GoodsDescription ?? ZString.Empty;
		#endregion

		public ZString RefundDrawbackItemNumber => EntryLine?.RandomLine?.CusLineTariffDetails.FirstOrDefault(x => x.BZ_Type.Left(2) == "5P")?.BZ_Tariff.Left(5) ?? ZString.Empty;

		#region Implementation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void GetCalcFeeValues()
		{
			var result = new CalcFeeValues();
			da63CustomsValue = ZDecimal.Zero;
			da63CustomsQuantity = ZDecimal.Zero;
			da63AdditionalQuantity1 = ZDecimal.Zero;
			da63AdditionalQuantity2 = ZDecimal.Zero;

			da63CustomsUnitQty = ZString.Empty;
			da63AdditionalUnitQty1 = ZString.Empty;
			da63AdditionalUnitQty2 = ZString.Empty;

			var customsDutiesExcluding12B = new List<DA63AdditionalDuty>();
			var penalties = new List<DA63AdditionalDuty>();
			var provisionalPayments = new List<DA63AdditionalDuty>();
			if (EntryLine != null)
			{
				foreach (var invoiceLine in EntryLine.InvoiceLines.OfType<JobComInvoiceLine>())
				{
					if (da63CustomsUnitQty.Value.IsEmpty)
					{
						da63CustomsUnitQty = invoiceLine.JI_ImportCustomsQtyUQ;
					}
					if (da63AdditionalUnitQty1.Value.IsEmpty)
					{
						da63AdditionalUnitQty1 = invoiceLine.JI_ImportCustomsQty2UQ;
					}
					if (da63AdditionalUnitQty2.Value.IsEmpty)
					{
						da63AdditionalUnitQty2 = invoiceLine.JI_ImportCustomsQty3UQ;
					}

					da63CustomsValue += invoiceLine.JI_ImportCustomsValue;
					da63CustomsQuantity += invoiceLine.JI_ImportCustomsQty;
					da63AdditionalQuantity1 += invoiceLine.JI_ImportCustomsQty2;
					da63AdditionalQuantity2 += invoiceLine.JI_ImportCustomsQty3;
					result.CustomsDutyExcluding12B += invoiceLine.JI_ImportDutyPaid;
					result.S1P2BDuty += invoiceLine.JI_ImportSch1P2BPaid;
					result.ValueAddedTax += invoiceLine.JI_ImportVATPaid;
					result.ProvisionalPayment += invoiceLine.JI_ImportProvisionalPayment;
					result.Penalty += invoiceLine.JI_ImportPenalty;

					customsDutiesExcluding12B.AddRange(invoiceLine.DA63AdditionalDuties.CustomsDutiesExcluding12B);
					penalties.AddRange(invoiceLine.DA63AdditionalDuties.Penalties);
					provisionalPayments.AddRange(invoiceLine.DA63AdditionalDuties.ProvisionalPayments);
				}
			}
			result.CustomsDutiesExcluding12B = customsDutiesExcluding12B.GroupBy(x => x.CY_Code).Select(x => new DutyFeeInformationDocWrapper(x.Key, x.Sum(y => y.CY_Value)));
			result.ProvisionalPayments = provisionalPayments.GroupBy(x => x.CY_Code).Select(x => new DutyFeeInformationDocWrapper(x.Key, x.Sum(y => y.CY_Value)));
			result.Penalties = penalties.GroupBy(x => x.CY_Code).Select(x => new DutyFeeInformationDocWrapper(x.Key, x.Sum(y => y.CY_Value)));
			da63CalcFeeValues = result;
		}

		#endregion
	}
}
