using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	class OriginalEntryHeaderIReconciliationImportEntry : IReconciliationImportEntry
	{
		public OriginalEntryHeaderIReconciliationImportEntry(ReconOriginalEntryHeader originalEntry)
		{
			this.originalEntry = originalEntry;
		}

		readonly ReconOriginalEntryHeader originalEntry;

		#region IReconciliationImportEntry Members

		ZString IReconciliationImportEntry.ImportEntryFilerCodeNumber
		{
			get { return originalEntry.CH_OrigEntryReference; }
		}

		ZString IReconciliationImportEntry.EntryFilerCode => originalEntry.CH_OrigEntryReference.Left(3);
		ZString IReconciliationImportEntry.EntryNumber => originalEntry.CH_OrigEntryReference.SubstringSafe(3);

		ZString IReconciliationImportEntry.Port
		{
			get { return originalEntry.US_SchDEntry; }
		}

		ZDecimal IReconciliationImportEntry.OriginalDuty
		{
			get { return originalEntry.OriginalDuty; }
		}

		ZDecimal IReconciliationImportEntry.EstimatedReconciliationDuty
		{
			get { return originalEntry.ReconDuty; }
		}

		ZDecimal IReconciliationImportEntry.OriginalTax
		{
			get { return originalEntry.OriginalTax; }
		}

		ZDecimal IReconciliationImportEntry.EstimatedReconciliationTax
		{
			get { return originalEntry.ReconTax; }
		}

		ZDecimal IReconciliationImportEntry.EstimatedReconciliationInterest
		{
			get { return originalEntry.ReconInterest; }
		}

		IEnumerable<IReconciliationImportEntryFee> ACSFees
		{
			get
			{
				foreach (CodeDescriptionPair pair in accountingClassFeeCode)
				{
					if (!CusFeeCodeConstants.IsExciseTax(pair.Code) || originalEntry.ReconDeclaration.IsACE)
					{
						var originalFee = originalEntry.OriginalCharges.GetAmount(pair.Code);
						var reconFee = originalEntry.ReconCharges.GetAmount(pair.Code);
						if (originalFee > 0m || reconFee > 0m)
						{
							yield return new ReconEntryFeeIReconciliationImportEntryFee()
							{
								FeeType = pair.Code,
								OriginalFee = originalFee,
								ReconFee = reconFee
							};
						}
					}
				}
				if (!originalEntry.ReconDeclaration.IsACE)
				{
					foreach (ReconRefundedCharge refundedFee in originalEntry.RefundedFees)
					{
						yield return new RefundedEntryFee() { FeeClass = refundedFee.CY_Code };
					}
				}
			}
		}

		CodeDescriptionPairList accountingClassFeeCode => CusFeeCodeConstants.GetAccountingClassFeeCodeList(originalEntry.Factory);

		IEnumerable<IReconciliationImportEntryFee> ACEFees
		{
			get
			{
				List<IReconciliationImportEntryFee> aceFeesList = new List<IReconciliationImportEntryFee>();
				ZDecimal originalDuty = 0m;
				ZDecimal reconDuty = 0m;

				if (originalEntry.OriginalDuty > 0m || originalEntry.ReconDuty > 0m)
				{
					originalDuty = originalEntry.OriginalDuty;
					reconDuty = originalEntry.ReconDuty;
				}
				aceFeesList.Add(new ReconEntryFeeIReconciliationImportEntryFee()
				{
					FeeType = ReconDeclaration.Constants.DutyAccountingClassCode,
					OriginalFee = originalDuty,
					ReconFee = reconDuty
				});

				if (!ACSFees.Any(x => x.FeeClass == Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing))
				{
					aceFeesList.Add(new ReconEntryFeeIReconciliationImportEntryFee()
					{
						FeeType = Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing,
						OriginalFee = 0m,
						ReconFee = 0m
					});
				}
				aceFeesList.AddRange(ACSFees);

				foreach (JobComInvoiceLine invoiceLine in originalEntry.Invoice.InvoiceLines)
				{
					foreach (ReconRefundedCharge refundedFee in invoiceLine.ReconRefundedFees)
					{
						aceFeesList.Add(new RefundedEntryFee()
						{
							FeeClass = refundedFee.CY_Code
						});
					}

					var originTariffRequiredCodes = invoiceLine.OriginalImportTariff.GetReconTariffRequiredFeeCodes(invoiceLine.US_R_OrigCottonFeeExempt == YesNoDefaultList.Codes.Yes, originalEntry.US_R_CottonFeeMandatory);
					var reconTariffRequiredCode = invoiceLine.ImportTariff.GetReconTariffRequiredFeeCodes(invoiceLine.IsCottonFeeExemptIndicated, originalEntry.US_R_CottonFeeMandatory);
					var tariffRequiredCodes = originTariffRequiredCodes.Concat(reconTariffRequiredCode).Distinct();

					foreach (var feeCode in tariffRequiredCodes)
					{
						if (!aceFeesList.Any(x => x.FeeClass == feeCode))
						{
							aceFeesList.Add(new ReconEntryFeeIReconciliationImportEntryFee()
							{
								FeeType = feeCode,
								OriginalFee = 0m,
								ReconFee = 0m
							});
						}
					}
				}

				if (!originalEntry.ReconDeclaration.US_IsAggregate)
				{
					var reconinterestFee = originalEntry.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.ReconciliationInterest);
					if (reconinterestFee > 0m)
					{
						aceFeesList.Add(new ReconEntryFeeIReconciliationImportEntryFee()
						{
							FeeType = ReconDeclaration.Constants.InterestAccountingClassCode,
							OriginalFee = 0m,
							ReconFee = reconinterestFee
						});
					}
				}

				var totalAceFeesList = aceFeesList.GroupBy(x => x.FeeClass).Select(x => new ReconEntryFeeIReconciliationImportEntryFee()
				{
					FeeType = x.Key,
					ReconFee = x.Sum(fee => fee.EstimatedReconciliationFee),
					OriginalFee = x.Sum(fee => fee.OriginalFee)
				}).ToList();

				totalAceFeesList.Sort(new ReconEntryFeeComparer());
				return totalAceFeesList;
			}
		}

		IEnumerable<IReconciliationImportEntryFee> IReconciliationImportEntry.Fees => originalEntry.ReconDeclaration.IsACE ? ACEFees : ACSFees;

		//ARECRP1
		ZString IReconciliationImportEntry.ProtestID => originalEntry.US_ProtestID;

		//ARECRQ1
		ZString IReconciliationImportEntry.PendingActionIDType => originalEntry.US_PendingActionIDType;
		ZString IReconciliationImportEntry.PendingActionID => originalEntry.US_PendingActionID;

		class RefundedEntryFee : IReconciliationImportEntryFee
		{
			public ZString FeeClass;

			ZString IReconciliationImportEntryFee.FeeClass
			{
				get { return FeeClass; }
			}

			ZDecimal IReconciliationImportEntryFee.OriginalFee
			{
				get { return ZDecimal.Zero; }
			}

			ZDecimal IReconciliationImportEntryFee.EstimatedReconciliationFee
			{
				get { return ZDecimal.Zero; }
			}
		}

		#endregion
	}
}
