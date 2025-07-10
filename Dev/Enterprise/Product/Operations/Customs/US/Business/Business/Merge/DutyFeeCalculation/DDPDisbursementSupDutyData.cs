using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.US.Business.DDPDisbursementCalculation;

namespace Enterprise.Customs.US.Business
{
	class DDPDisbursementSupDutyData : DDPDisbursementDutyDataBase
	{
		public DDPDisbursementSupDutyData(JobComInvoiceLine invoiceLine, Dictionary<JobComInvoiceLine, Dictionary<string, DDPCalculationResultData>> chargesAndFees, Dictionary<JobComInvoiceLine, CustomsValues> customsValues, bool calculateMPF)
			: base(invoiceLine, invoiceLine.SupplementaryParentTariffIDutyData, chargesAndFees, customsValues, calculateMPF)
		{
		}

		protected override IEnumerable<IEntryLineOrInvoiceLineDutyData> SecondaryLinesCore
		{
			get
			{
				if (invoiceLine.US_SupAdditionalTariff1.IsEmpty && !invoiceLine.HasEmptySupTariff && !invoiceLine.IsSecondaryTariffLine)
				{
					yield return new DDPDisbursementLineDutyData(invoiceLine, chargesAndFees, customsValues, calculateMPF);

					foreach (JobComInvoiceLine secondaryLine in invoiceLine.SecondaryTariffLines)
					{
						if (!secondaryLine.HasEmptySupTariff)
						{
							yield return new DDPDisbursementSupDutyData(secondaryLine, chargesAndFees, customsValues, calculateMPF);
						}

						yield return new DDPDisbursementLineDutyData(secondaryLine, chargesAndFees, customsValues, calculateMPF);
					}
				}
			}
		}

		protected override IDutyData ParentTariffLineCore
		{
			get
			{
				if (!invoiceLine.US_SupAdditionalTariff1.IsEmpty && invoiceLine.ImportSupAdditionalTariff1 != null)
				{
					return new DDPDisbursementSupAdditionalDutyData(invoiceLine, invoiceLine.US_SupAdditionalTariff1, invoiceLine.ImportSupAdditionalTariff1, chargesAndFees, customsValues, IDutyDataExtensionMethod.TariffTypeForDDP.AdditionalTariff1, calculateMPF);
				}
				else if (invoiceLine.ParentTariffLine is JobComInvoiceLine parentLine)
				{
					return new DDPDisbursementSupDutyData(parentLine, chargesAndFees, customsValues, calculateMPF);
				}

				return null;
			}
		}

		protected override IEntryLineOrInvoiceLineDutyData ParentLineCore
		{
			get
			{
				if (!invoiceLine.US_SupAdditionalTariff1.IsEmpty && invoiceLine.ImportSupAdditionalTariff1 != null)
				{
					return new DDPDisbursementSupAdditionalDutyData(invoiceLine, invoiceLine.US_SupAdditionalTariff1, invoiceLine.ImportSupAdditionalTariff1, chargesAndFees, customsValues, IDutyDataExtensionMethod.TariffTypeForDDP.AdditionalTariff1, calculateMPF);
				}
				else if (invoiceLine.ParentTariffLine is JobComInvoiceLine parentLine)
				{
					return new DDPDisbursementSupDutyData(parentLine, chargesAndFees, customsValues, calculateMPF);
				}

				return null;
			}
		}

		protected override bool IsSup
		{
			get { return true; }
		}

		public override ZDecimal? OverriddenTaxRate
		{
			get { return null; }
		}

		public override ZString OverriddenTaxRateUQ
		{
			get { return ZString.Empty; }
		}

		public override ZString TaxCode
		{
			get { return ZString.Empty; }
		}

		public override ZString TaxComputationCode
		{
			get { return ZString.Empty; }
		}

		public override ZString TaxRateType
		{
			get { return ZString.Empty; }
		}

		public override ZDecimal TaxRateQuantity
		{
			get { return ZDecimal.Zero; }
		}

		public override IEnumerable<IFeeCalculationDataProvider> FeeDataProviders
		{
			get
			{
				if (!this.IsCombinedLine())
				{
					yield return this;

					yield return new DDPDisbursementLineDutyData(invoiceLine, chargesAndFees, customsValues, calculateMPF);
				}
			}
		}

		protected override ZDecimal GetSupCustomsValue()
		{
			return invoiceLine.GetSupCustomsValue(true, false, false, false, false, false);
		}

		public override void CalculateNormalDutyIfRequired(ILineDutyFeeCalculator dutyFeeCalculator)
		{
			if (dutyData.ImportTariff is USCTariff tariff && tariff.Applies(TariffRuleList.Codes.AdditionalTariffs, dutyData.DateForDutyCalculation))
			{
				var dutyData = new DDPDisbursementLineDutyData(invoiceLine, chargesAndFees, customsValues, calculateMPF);
				dutyFeeCalculator.CalculateNormalDuty(dutyData);
			}
		}
	}
}
