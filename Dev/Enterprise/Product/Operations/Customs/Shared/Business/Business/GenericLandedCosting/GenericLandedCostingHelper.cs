using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public class GenericLandedCostingHelper : LandedCostingHelper
	{
		protected GenericLandedCostingHelper()
		{
		}

		public GenericLandedCostingHelper(string countryCode)
		{
			config = GetGenericLandedCostingConfig(countryCode);
		}
		protected GenericLandedCostingConfig config;

		protected override DutyTaxEntryFee GetLineDutyTaxEntryFeeItemsCore(BaseJobComInvoiceLine invoiceLine)
		{
			var result = new DutyTaxEntryFee();

			if (config != null)
			{
				foreach (var mapping in config.LCEntryCustomsDisbursementCodeMappings)
				{
					result[mapping.LCDisbursementCode] += GetInvoiceLineAmount(invoiceLine, mapping).Round(4);
				}
			}

			return result;
		}

		protected override DutyTaxEntryFee GetTotalDutyTaxEntryFeeItemsCore(BaseJobDeclaration declaration)
		{
			var result = new DutyTaxEntryFee();

			if (config != null && config.LCEntryCustomsDisbursementCodeMappings.Any())
			{
				foreach (var entryConfig in config.FormalEntryConfigs)
				{
					var entryHeaders = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().Where(x => x.CH_MessageType == entryConfig.EntryType);
					foreach (var mapping in config.LCEntryCustomsDisbursementCodeMappings)
					{
						foreach (var entryHeader in entryHeaders)
						{
							if (mapping.HeaderLevelFee)
							{
								result[mapping.LCDisbursementCode] += GetEntryHeaderAmount(entryHeader, mapping).Round(4);
							}
							else
							{
								foreach (CusEntryLine entryLine in entryHeader.MergedLines)
								{
									result[mapping.LCDisbursementCode] += GetEntryLineAmount(entryLine, mapping).Round(4);
								}
							}
						}
					}
				}
			}

			return result;
		}

		ZDecimal GetInvoiceLineAmount(BaseJobComInvoiceLine invoiceLine, LCEntryCustomsDisbursementCodeMapping mapping)
		{
			var result = ZDecimal.Zero;
			if (!mapping.JobComInvoiceLineAmountPropertyName.IsEmpty)
			{
				result = invoiceLine.GetPropertyValue<ZDecimal>(mapping.JobComInvoiceLineAmountPropertyName);
			}
			else
			{
				foreach (var entryConfig in config.FormalEntryConfigs)
				{
					var entryLine = entryConfig.IsAdditionalEntryLine ?
					invoiceLine.AdditionalEntryLineLinks.FirstOrDefault(x => x.EntryLine.Header != null && x.EntryLine.Header.CH_MessageType == entryConfig.EntryType)?.EntryLine : invoiceLine.CusEntryLine;
					if (entryLine != null)
					{
						var entryLineAmount = GetEntryLineAmount(entryLine, mapping);
						result = invoiceLine.GetAmountApportionedFromCusEntryLine(entryLine, entryLineAmount);
					}
				}
			}
			return result;
		}

		ZDecimal GetEntryHeaderAmount(CusEntryHeader entryHeader, LCEntryCustomsDisbursementCodeMapping mapping)
		{
			return entryHeader.Charges.GetAmount(mapping.EntryDisbursementCode);
		}

		ZDecimal GetEntryLineAmount(CusEntryLine entryLine, LCEntryCustomsDisbursementCodeMapping mapping)
		{
			return mapping.HeaderLevelFee ?
				entryLine.GetAmountApportionedFromCusEntryHeader(GetEntryHeaderAmount(entryLine.Header, mapping), mapping.CusEntryLineApplicablePropertyName) : entryLine.Fees.GetAmount(mapping.EntryDisbursementCode);
		}

		GenericLandedCostingConfig GetGenericLandedCostingConfig(string countryCode)
		{
			return GenericLandedCostingConfigProvider.Instance.GetGenericLandedCostingConfig(countryCode);
		}
	}
}
