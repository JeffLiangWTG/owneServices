using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Customs.US;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class ReconDeclarationIReconciliation : IReconciliation
	{
		public ReconDeclarationIReconciliation(ReconDeclaration reconDeclaration)
		{
			this.reconDeclaration = reconDeclaration;
			if (reconDeclaration.IsACE)
			{
				new ReconChangedLinesMerger(ReconDeclaration, ReconMergeContext.Messaging).DoMerge(needCountDecreaseLine: false);
			}
		}

		readonly ReconDeclaration reconDeclaration;

		public ReconDeclaration ReconDeclaration
		{
			get { return reconDeclaration; }
		}

		#region IReconciliation Members

		public BusinessObjectFactory Factory
		{
			get { return reconDeclaration.Factory; }
		}

		public ZString EntryFilerCode
		{
			get { return reconDeclaration.US_EntryFilerCode; }
		}

		public ZString OfficeCode
		{
			get { return USCustomsDataRegistry.Instance.BRecordOfficeCode.GetValueWithoutFallback(reconDeclaration.RegistryCompanyPK, Guid.Empty, Guid.Empty); }
		}

		//For a broker who is located in Chicago and transmits recon entries to Detroit,
		//PreparerDistrictPort is Chicago and ProcessingDistrictPort is Detroit for the purpose of reconcilidation
		public ZString PreparerDistrictPort
		{
			get { return reconDeclaration.PreparerDistrictPort; }
		}

		public ZString MessageStatus
		{
			set { reconDeclaration.MessageStatus = value; }
		}

		public ZString US_Paid
		{
			get
			{
				var declaration = reconDeclaration.ReconWrappedJobDeclaration;
				return declaration == null ? ZString.Empty : declaration.US_Paid;
			}
			set
			{
				var declaration = reconDeclaration.ReconWrappedJobDeclaration;
				if (declaration != null)
				{
					declaration.US_Paid = value;
				}
			}
		}

		public ZString EntryNumber
		{
			get
			{
				if (reconDeclaration.IsACE)
				{
					return reconDeclaration.ReconEntryNumber.IsEmpty ? new ZString(MQEDIMessage.USEntryNumberPlaceHolder) : reconDeclaration.ReconEntryNumber;
				}
				else
				{
					return reconDeclaration.US_EntryFilerCode + reconDeclaration.ReconEntryNumber;
				}
			}
		}

		public ZString ProcessingDistrictPort
		{
			get { return reconDeclaration.US_SchDEntry; }
		}

		public ZString TeamNumber
		{
			get { return reconDeclaration.US_TeamNo; }
		}

		public ZString ImporterID
		{
			get { return reconDeclaration.ImporterOfRecordNumber; }
		}

		public ZString SuretyCode
		{
			get { return reconDeclaration.US_SuretyCode; }
		}

		public ZDate EstimatedReconciliationEntrySummaryDate
		{
			get { return reconDeclaration.US_EstimatedEntryDate.Date; }
		}

		public ZString IssueCode
		{
			get { return reconDeclaration.US_IssueCode; }
		}

		public bool AggregateReconciliationIndicator
		{
			get { return reconDeclaration.US_IsAggregate; }
		}

		public bool IsNoChangeAggregate
		{
			get { return reconDeclaration.IsNoChangeAggregate; }
		}

		public bool IsWaiveRefund
		{
			get { return reconDeclaration.US_R_Waive; }
		}

		public IEnumerable<ZString> AggregateRefundedFees
		{
			get
			{
				return from ReconRefundedCharge charge in reconDeclaration.AggregateRefundedFees
					   select charge.CY_Code;
			}
		}

		public ZString IncreaseRefundIndicator
		{
			get
			{
				if (IsWaiveRefund)
				{
					return IncreaseRefundIndicatorCodes.IncreaseOrNoChange;
				}

				ZDecimal orginalDuty = 0m, reconDuty = 0m;
				ZDecimal originalTax = 0m, reconTax = 0m;
				ZDecimal originalFee = 0m, reconFee = 0m;

				foreach (ReconOriginalEntryHeader originalEntry in reconDeclaration.OriginalEntries)
				{
					orginalDuty += originalEntry.OriginalDuty;
					originalTax += originalEntry.OriginalTax;
					originalFee += originalEntry.OriginalFee;

					reconDuty += originalEntry.ReconDuty;
					reconTax += originalEntry.ReconTax;
					reconFee += originalEntry.ReconFee;
				}

				ZDecimal dutyDifference = reconDuty - orginalDuty;
				ZDecimal taxDifference = reconTax - originalTax;
				ZDecimal feeDifference = reconFee - originalFee;

				if (dutyDifference >= 0m && taxDifference >= 0m && feeDifference >= 0m)
				{
					return IncreaseRefundIndicatorCodes.IncreaseOrNoChange;
				}
				else if (dutyDifference < 0m && taxDifference < 0m && feeDifference < 0m)
				{
					return IncreaseRefundIndicatorCodes.Decrease;
				}
				else
				{
					return IncreaseRefundIndicatorCodes.Combination;
				}
			}
		}

		public ZDate EarliestImportDate
		{
			get
			{
				ZDate result = ZDate.Empty;

				if (IssueCode == ReconIssueCodeList.Codes.FTA)
				{
					result = GetEarliestImportDate();
				}

				return result;
			}
		}

		public ZDate EarliestEntrySummaryDate
		{
			get
			{
				ZDate result = ZDate.Empty;

				if (IssueCode != ReconIssueCodeList.Codes.FTA)
				{
					result = reconDeclaration.EarliestEntryDate;
				}

				return result;
			}
		}

		public ZString AgentBrokerReferenceID
		{
			get { return ""; }
		}

		public ZString BrokerReferenceNumber
		{
			get { return reconDeclaration.JE_DeclarationReference.Right(9); }
		}

		public ZInt ImportEntrySource
		{
			get { return ZInt.ParseSafe(reconDeclaration.US_ImportEntrySource, 0); }
		}

		public ZString TextComment
		{
			get { return reconDeclaration.US_Comment.Trim(); }
		}

		public ZString PaymentTypeIndicator
		{
			get { return reconDeclaration.US_PaymentType; }
		}

		public ZDate PreliminaryStatementPrintDate
		{
			get { return reconDeclaration.US_PreliminaryStatementPrintDate.Date; }
		}

		public ZString ClientBranchDesignation
		{
			get { return reconDeclaration.US_ClientBranchDesignation; }
		}

		/// <summary>
		/// Difference between the total original duty and the total recon duty. If negative, zero-fill
		/// </summary>
		public ZDecimal DutyPaymentAmount
		{
			get
			{
				var result = ZDecimal.Zero;

				if (!IsNoChangeAggregate)
				{
					result =
						reconDeclaration.OriginalEntries.GetReconChargeAmount(Core.Constants.USCustoms.FeeCodes.Duty) -
						reconDeclaration.OriginalEntries.GetOriginalChargeAmount(Core.Constants.USCustoms.FeeCodes.Duty);

					result = result < 0 ? ZDecimal.Zero : result;
				}

				return result;
			}
		}

		/// <summary>
		/// Difference between the total original tax and the total recon tax. If negative, zero-fill
		/// </summary>
		public ZDecimal TaxPaymentAmount
		{
			get
			{
				var result = ZDecimal.Zero;

				if (!IsNoChangeAggregate)
				{
					result =
					  reconDeclaration.OriginalEntries.GetReconChargeAmount(CusFeeCodeConstants.GetTaxCodes()) -
					  reconDeclaration.OriginalEntries.GetOriginalChargeAmount(CusFeeCodeConstants.GetTaxCodes());

					result = result < 0 ? ZDecimal.Zero : result;
				}

				return result;
			}
		}

		/// <summary>
		/// Difference between the total original fee and the total recon fee. If negative, zero-fill
		/// </summary>
		public ZDecimal FeePaymentAmount
		{
			get
			{
				var result = ZDecimal.Zero;

				if (!IsNoChangeAggregate)
				{
					result =
						reconDeclaration.OriginalEntries.GetReconChargeAmount(EntryChargeTypeList.GetFeeCodes()) -
						reconDeclaration.OriginalEntries.GetOriginalChargeAmount(EntryChargeTypeList.GetFeeCodes());

					result = result < 0 ? ZDecimal.Zero : result;
				}

				return result;
			}
		}

		public ZDecimal InterestPaymentAmount
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;
				if (AggregateReconciliationIndicator)
				{
					result = reconDeclaration.US_R_AggregateInterest;
				}
				else
				{
					result = reconDeclaration.OriginalEntries.GetReconChargeAmount(Core.Constants.USCustoms.FeeCodes.ReconciliationInterest);
				}
				return result;
			}
		}

		public IEnumerable<IReconciliationImportEntry> ImportEntries
		{
			get
			{
				foreach (ReconOriginalEntryHeader entry in reconDeclaration.OriginalEntries)
				{
					yield return new OriginalEntryHeaderIReconciliationImportEntry(entry);
				}
			}
		}

		public void AddMessage(MQEDIMessage message)
		{
			((IMessageAttachee)reconDeclaration).Messages.Add(message);
		}

		void IReconciliation.LogCustomsCommencedIfNeeded()
		{
			if (reconDeclaration.ReconWrappedJobDeclaration != null)
			{
				reconDeclaration.ReconWrappedJobDeclaration.LogCustomsCommencedIfNeeded();
			}
		}

		public ZGuid CompanyPK => reconDeclaration.Branch.GB_GC;

		//ARECR10
		public ZString DesignatedNotifyParty4811Number => reconDeclaration.NotifyPartyID;
		public ZBool PriorDisclosureIndicator => reconDeclaration.PriorDisclosureIndicator;

		//ARECR11
		public ZString ContactName => reconDeclaration.CusAgent?.GS_FullName ?? ZString.Empty;
		public ZString ContactPhone => reconDeclaration.BrokerPhone;
		public ZString ContactEmail => reconDeclaration.CusAgent?.GS_EmailAddress ?? ZString.Empty;

		//ARECR15
		public ZBool QualifyingGoodFreeTradeDec => reconDeclaration.US_IssueCode == ReconIssueCodeList.Codes.FTA;
		public ZBool SummaryDocProvidedStatement => reconDeclaration.SummaryDocProvidedStatement;
		public ZBool NAFTA303ClaimStatement => reconDeclaration.NAFTA303ClaimStatement;
		public ZBool ProtestOrPetitionFiledStatement => reconDeclaration.ProtestOrPetitionFiledStatement;

		//ARECRD1 D2 D3
		public ZString DocumentRecipientID => reconDeclaration.DocRecipientID;
		public ZDate DocumentProvidedDate => reconDeclaration.US_DocProvidedDate.Date;
		public JobDocAddress DocumentRecipientAddress => reconDeclaration.SummaryDocRecipientAddress;

		//ARECRC1 C2 C3
		public ZString ClaimentID => reconDeclaration.ClaimantID;
		public ZDate ClaimDate => reconDeclaration.US_ClaimDate.Date;
		public ZString ClaimIdentifier => reconDeclaration.US_ClaimID;
		public JobDocAddress ClaimentAddress => reconDeclaration.ClaimantAddress;

		public IEnumerable<IReconEntryLineGroup> EntryLineGroups
		{
			get
			{
				if (entryLineGroups == null)
				{
					List<IReconEntryLine> changeLines = new List<IReconEntryLine>();
					changeLines.AddRange(reconDeclaration.ChangedLines.Cast<IReconEntryLine>().Select(x => x));
					var lineGroups = changeLines.GroupBy(x => x.GroupLineNumber);
					var entryLineGroupsList = new List<IReconEntryLineGroup>();
					foreach (var lineGroup in lineGroups)
					{
						entryLineGroupsList.Add(new ReconEntryLineGroup(lineGroup.ToList()));
					}
					entryLineGroups = entryLineGroupsList;
				}
				return entryLineGroups;
			}
		}

		IEnumerable<IReconEntryLineGroup> entryLineGroups;
		#endregion

		#region Implementation

		ZDate GetEarliestImportDate()
		{
			ZDateTime result = ZDateTime.Empty;

			foreach (ReconOriginalEntryHeader originalEntry in reconDeclaration.OriginalEntries)
			{
				if (result.IsEmpty || originalEntry.US_ImportDate.IsValid && originalEntry.US_ImportDate < result)
				{
					result = originalEntry.US_ImportDate;
				}
			}

			return result.Date;
		}

		#endregion
	}
}
