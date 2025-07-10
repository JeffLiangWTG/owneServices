using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.US.Business.DDPDisbursementCalculation;

namespace Enterprise.Customs.US.Business
{
	internal class DDPDisbursementLineDutyData : DDPDisbursementDutyDataBase, IEntryLineOrInvoiceLineDutyData, IFeeCalculationDataProvider
	{
		public DDPDisbursementLineDutyData(JobComInvoiceLine invoiceLine, Dictionary<JobComInvoiceLine, Dictionary<string, DDPCalculationResultData>> chargesAndFees, Dictionary<JobComInvoiceLine, CustomsValues> customsValues, bool calculateMPF)
			: base(invoiceLine, invoiceLine, chargesAndFees, customsValues, calculateMPF)
		{
		}

		protected override IEnumerable<IEntryLineOrInvoiceLineDutyData> SecondaryLinesCore
		{
			get
			{
				if (invoiceLine.HasEmptySupTariff && !invoiceLine.IsSecondaryTariffLine)
				{
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
				else if (!invoiceLine.US_SupTariff.IsEmpty && invoiceLine.ImportSupTariff != null)
				{
					return new DDPDisbursementSupDutyData(invoiceLine, chargesAndFees, customsValues, calculateMPF);
				}
				else if (invoiceLine.ParentTariffLine is JobComInvoiceLine parentLine)
				{
					return new DDPDisbursementLineDutyData(invoiceLine.ParentTariffLine, chargesAndFees, customsValues, calculateMPF);
				}

				return null;
			}
		}

		protected override bool IsSup
		{
			get { return this.IsCombinedLine(); }
		}

		public override ZDecimal? OverriddenTaxRate
		{
			get { return ((IFeeCalculationDataProvider)invoiceLine).OverriddenTaxRate; }
		}

		public override ZString OverriddenTaxRateUQ
		{
			get { return ((IFeeCalculationDataProvider)invoiceLine).OverriddenTaxRateUQ; }
		}

		public override ZString TaxCode
		{
			get { return ((IFeeCalculationDataProvider)invoiceLine).TaxCode; }
		}

		public override ZString TaxComputationCode
		{
			get { return ((IFeeCalculationDataProvider)invoiceLine).TaxComputationCode; }
		}

		public override ZString TaxRateType
		{
			get { return ((IFeeCalculationDataProvider)invoiceLine).TaxRateType; }
		}

		public override ZDecimal TaxRateQuantity
		{
			get { return ((IFeeCalculationDataProvider)invoiceLine).TaxRateQuantity; }
		}

		public override IEnumerable<IFeeCalculationDataProvider> FeeDataProviders
		{
			get { yield return this; }
		}

		protected override IEntryLineOrInvoiceLineDutyData ParentLineCore
		{
			get
			{
				if (!invoiceLine.US_SupAdditionalTariff1.IsEmpty && invoiceLine.ImportSupAdditionalTariff1 != null)
				{
					return new DDPDisbursementSupAdditionalDutyData(invoiceLine, invoiceLine.US_SupAdditionalTariff1, invoiceLine.ImportSupAdditionalTariff1, chargesAndFees, customsValues, IDutyDataExtensionMethod.TariffTypeForDDP.AdditionalTariff1, calculateMPF);
				}
				else if (!invoiceLine.US_SupTariff.IsEmpty && invoiceLine.ImportSupTariff != null)
				{
					return new DDPDisbursementSupDutyData(invoiceLine, chargesAndFees, customsValues, calculateMPF);
				}
				else if (invoiceLine.ParentTariffLine is JobComInvoiceLine parentLine)
				{
					return new DDPDisbursementLineDutyData(invoiceLine.ParentTariffLine, chargesAndFees, customsValues, calculateMPF);
				}

				return null;
			}
		}

		bool IFeeCalculationDataProvider.IsACS
		{
			get => ((IFeeCalculationDataProvider)invoiceLine).IsACS;
		}

		void IFeeCalculationDataProvider.SetFeeResult(string chargeCode, decimal amount, FeeCalculationInternalData feeCalculationInternalData)
		{
			var amountToSet = amount;

			if (!invoiceLine.HasEmptySupTariff)
			{
				amountToSet += chargesAndFees.GetChargeOrFeeAmount(invoiceLine, chargeCode);
			}

			SetFeeResult(chargeCode, amountToSet, feeCalculationInternalData);
		}
	}
}
