using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.US.Business.DDPDisbursementCalculation;
using TariffType = Enterprise.Customs.US.Business.IDutyDataExtensionMethod.TariffTypeForDDP;

namespace Enterprise.Customs.US.Business
{
	internal class DDPDisbursementSupAdditionalDutyData : DDPDisbursementSupDutyData
	{
		public DDPDisbursementSupAdditionalDutyData(JobComInvoiceLine invoiceLine, ZString tariffNumber, USCTariff importTariff, Dictionary<JobComInvoiceLine, Dictionary<string, DDPCalculationResultData>> chargesAndFees, Dictionary<JobComInvoiceLine, CustomsValues> customsValues, TariffType tariffType, bool calculateMPF)
			: base(invoiceLine, chargesAndFees, customsValues, calculateMPF)
		{
			this.tariffNumber = tariffNumber;
			this.importTariff = importTariff;
			this.tariffType = tariffType;
		}
		readonly ZString tariffNumber;
		readonly USCTariff importTariff;
		readonly TariffType tariffType;

		protected override ZString TariffCore => tariffNumber;

		protected override USCTariff ImportTariffCore => importTariff;

		protected override IEnumerable<IEntryLineOrInvoiceLineDutyData> SecondaryLinesCore
		{
			get
			{
				if (tariffType == TariffType.AdditionalTariff1)
				{
					foreach (var dutyData in invoiceLine.GetDutyDataListFromInvoiceLine(chargesAndFees, customsValues, calculateMPF, TariffType.AdditionalTariff1))
					{
						yield return dutyData;
					}

					foreach (var secondaryLine in invoiceLine.SecondaryTariffLines)
					{
						foreach (var dutyData in secondaryLine.GetDutyDataListFromInvoiceLine(chargesAndFees, customsValues, calculateMPF, TariffType.None))
						{
							yield return dutyData;
						}
					}
				}
			}
		}

		protected override IEntryLineOrInvoiceLineDutyData ParentLineCore
		{
			get
			{
				if (tariffType != TariffType.AdditionalTariff1 && !invoiceLine.US_SupAdditionalTariff1.IsEmpty && invoiceLine.ImportSupAdditionalTariff1 != null)
				{
					return new DDPDisbursementSupAdditionalDutyData(invoiceLine, invoiceLine.US_SupAdditionalTariff1, invoiceLine.ImportSupAdditionalTariff1, chargesAndFees, customsValues, TariffType.AdditionalTariff1, calculateMPF);
				}
				else if (invoiceLine.IsCombinedLine() && invoiceLine.IsCombineSecondaryTariffLine && invoiceLine.ParentTariffLine is JobComInvoiceLine parentTariffLine && parentTariffLine.JI_Tariff.IsEmpty)
				{
					if (!parentTariffLine.US_SupAdditionalTariff1.IsEmpty && parentTariffLine.ImportSupAdditionalTariff1 != null)
					{
						return new DDPDisbursementSupAdditionalDutyData(parentTariffLine, parentTariffLine.US_SupAdditionalTariff1, parentTariffLine.ImportSupAdditionalTariff1, chargesAndFees, customsValues, TariffType.AdditionalTariff1, calculateMPF);
					}
					else if (!parentTariffLine.US_SupTariff.IsEmpty && parentTariffLine.ImportSupTariff != null)
					{
						return new DDPDisbursementSupDutyData(parentTariffLine, chargesAndFees, customsValues, calculateMPF);
					}
				}

				return null;
			}
		}

		protected override IDutyData ParentTariffLineCore => this.ParentLineCore;

		protected override ZDecimal GetSupCustomsValue()
		{
			return invoiceLine.GetSupCustomsValue(false, tariffType == TariffType.AdditionalTariff1, tariffType == TariffType.AdditionalTariff2, tariffType == TariffType.AdditionalTariff3, tariffType == TariffType.AdditionalTariff4, tariffType == TariffType.AdditionalTariff5);
		}

		public override void CalculateNormalDutyIfRequired(ILineDutyFeeCalculator dutyFeeCalculator)
		{
			if (dutyData.ImportTariff is USCTariff tariff && tariff.Applies(TariffRuleList.Codes.AdditionalTariffs, dutyData.DateForDutyCalculation))
			{
				foreach (var dutyData in invoiceLine.GetDutyDataListFromInvoiceLine(chargesAndFees, customsValues, calculateMPF, tariffType))
				{
					dutyFeeCalculator.CalculateNormalDuty(dutyData);
				}
			}
		}
	}
}
