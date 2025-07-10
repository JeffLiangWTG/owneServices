using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow
{
	public abstract class DeclarationResponse : TSWResponse
	{
		protected DeclarationResponse(BaseTSWResponse response)
			: base(response)
		{
		}

		public IEnumerable<string> DeliveryInstructions
		{
			get { return deliveryInstructions ?? (deliveryInstructions = GetInstructions("DIN").ToList()); }
		}

		public CusEntryHeader EntryHeader
		{
			get
			{
				var entryHeader = LinkedObject as CusEntryHeader;
				entryHeader = entryHeader ?? (ConsolidatedDeclaration?.LeadDeclaration as JobDeclaration)?.CusEntryHeader;
				return entryHeader;
			}
		}

		public ForwardingConsol Consol
		{
			get
			{
				var consol = LinkedObject as ForwardingConsol;
				return consol;
			}
		}

		public ConsolidatedDeclaration ConsolidatedDeclaration
		{
			get
			{
				var consolidatedDeclaration = LinkedObject as ConsolidatedDeclaration;
				return consolidatedDeclaration;
			}
		}

		public INZManifestHeader ManifestConsol => (INZManifestHeader)LinkedObject;

		public IEnumerable<CusEntryLine> EntryLinesWithErrors
		{
			get
			{
				if (entryLinesWithErrors == null)
				{
					entryLinesWithErrors = new List<CusEntryLine>();
					foreach (var elementValue in GetElementsValues($"p:Error/p:Pointer[p:DocumentSectionCode='{GovernmentAgencyGoodsItemWCOID}']/p:SequenceNumeric"))
					{
						if (!string.IsNullOrEmpty(elementValue) && int.TryParse(elementValue, out var lineNumber))
						{
							var entryLine = EntryHeader.MergedLines.FindByLineNumber(lineNumber);
							if (entryLine != null)
							{
								entryLinesWithErrors.Add(entryLine);
							}
						}
					}
				}
				return entryLinesWithErrors;
			}
		}

		public ZDecimal ExpectedTotalAmount
		{
			get
			{
				var expectedTotalAmount = ZDecimal.Zero;
				var consolidatedDec = ConsolidatedDeclaration;
				if (consolidatedDec != null)
				{
					foreach (JobDeclaration declaration in consolidatedDec.JobDeclarations)
					{
						if (declaration.IsLeadDeclarationOfConsolidatedDeclarations)
						{
							expectedTotalAmount += declaration.CusEntryHeader.TotalAmountPayableIncludingEntryFee;
						}
						else
						{
							expectedTotalAmount += declaration.CusEntryHeader.TotalAmountPayable;
						}
					}
				}
				else
				{
					expectedTotalAmount = EntryHeader.TotalAmountPayableIncludingEntryFee;
				}
				return expectedTotalAmount;
			}
		}

		public bool IsClearedStatus(ZString status)
		{
			return status == FormalEntryStatusList.Codes.DeliveryOrderReceived
				|| status == FormalEntryStatusList.Codes.DeliveryOnPayment
				|| status == FormalEntryStatusList.Codes.EntryCleared;
		}

		public bool ThisResponseHasInspectionOrAuditRequirements
		{
			get { return MessageType == TransactionTypeList.Codes.Inspection; }
		}

		public string PaymentMethod
		{
			get { return paymentMethod ?? (paymentMethod = GetElementValue("p:OverallDeclaration/p:Declaration/p:DutyTaxFee[1]/p:Payment[1]/p:MethodCode")); }
		}

		public string PaymentMethodDescription
		{
			get { return paymentMethodDescription ?? (paymentMethodDescription = GetPaymentMethodDescription(PaymentMethod)); }
		}

		public bool IsMPIFoodResponse
		{
			get { return ResponsibleGovernmentAgency == ResponsibleGovernmentAgencyList.Codes.MPIFOOD; }
		}

		public bool IsMPIBiosecurityResponse
		{
			get { return ResponsibleGovernmentAgency == ResponsibleGovernmentAgencyList.Codes.MPIBIO; }
		}

		public bool IsCustomsResponse
		{
			get { return ResponsibleGovernmentAgency == ResponsibleGovernmentAgencyList.Codes.NZCS; }
		}

		public bool IsTSWAcknowledgementResponse
		{
			get { return ResponsibleGovernmentAgency == ResponsibleGovernmentAgencyList.Codes.TSW && Status == StatusList.Codes.Acknowledgement; }
		}

		public bool IsTSWResponse
		{
			get { return ResponsibleGovernmentAgency == ResponsibleGovernmentAgencyList.Codes.TSW; }
		}

		public decimal TotalAmount => NullableTotalAmount ?? 0m;

		public decimal? NullableTotalAmount
		{
			get
			{
				if (nullableTotalAmount == null)
				{
					nullableTotalAmount = decimal.TryParse(GetElementValue("p:OverallDeclaration/p:Declaration/p:DutyTaxFee[2]/p:Payment[1]/p:TaxAssessedAmount"), out var amount) ? amount : null;
				}
				return nullableTotalAmount;
			}
		}
		decimal? nullableTotalAmount;

		#region Overrides

		protected override string GetJobID()
		{
			if (Consol != null)
			{
				return Consol.JobNumber;
			}
			else if (ConsolidatedDeclaration != null)
			{
				return ConsolidatedDeclaration.CRD_JobReferenceNumber;
			}
			else
			{
				return EntryHeader.Declaration.JE_DeclarationReference;
			}
		}

		protected override string GetEnterpriseStatus()
		{
			switch (MessageType)
			{
				case TransactionTypeList.Codes.ClearanceInstructions:
					if (StatusList.IsDeliveryOnPayment(Status))
					{
						return TSWStatus.StatusCodes.PendingPayment;
					}
					else
					{
						return TSWStatus.StatusCodes.Cleared;
					}
				case TransactionTypeList.Codes.Inspection:
					return TSWStatus.StatusCodes.Inspection;
				case TransactionTypeList.Codes.Confirmation:
					return TSWStatus.StatusCodes.AdjustmentAccepted;
				case TransactionTypeList.Codes.CreditAdvice:
					return TSWStatus.StatusCodes.CreditAdvice;
				case TransactionTypeList.Codes.Receipt:
					return TSWStatus.StatusCodes.ResponseReceipt;
				case TransactionTypeList.Codes.Error:
					return TSWStatus.StatusCodes.Error;
				case TransactionTypeList.Codes.Cancel:
					return TSWStatus.StatusCodes.Cancelled;
				default:
					return "";
			}
		}

		protected override string GetEnterpriseStatusDescription()
		{
			var responseType = ResponseTypeList.GetDescriptionFromCode(MessageType);
			var agency = "";

			if (IsMPIFoodResponse)
			{
				agency = "Ministry for Primary Industries (Food) - ";
			}
			else if (IsMPIBiosecurityResponse)
			{
				agency = "Ministry for Primary Industries (Biosecurity) - ";
			}
			else
			{
				agency = "New Zealand Customs Service - ";
			}

			return agency + responseType;
		}

		public override void SetRelevantEntryStatus()
		{
			if (IsMPIFoodResponse)
			{
				EntryHeader.CH_MPIFoodStatus = new ZString(Status).Left(EntryHeader.CH_MPIFoodStatusInfo.MaxLength);
			}
			else if (IsMPIBiosecurityResponse)
			{
				EntryHeader.CH_MPIBioStatus = new ZString(Status).Left(EntryHeader.CH_MPIBioStatusInfo.MaxLength);
			}
			else
			{
				EntryHeader.CH_NZCSStatus = new ZString(Status).Left(EntryHeader.CH_NZCSStatusInfo.MaxLength);
			}
		}

		CodeDescriptionPairList ResponseTypeList
		{
			get { return Factory.GetCachedValue<TransactionTypeList>(); }
		}

		#endregion // Overrides

		#region Implementation

		const string GovernmentAgencyGoodsItemWCOID = "68A";

		#region Cached Fields

		List<string> deliveryInstructions;
		List<CusEntryLine> entryLinesWithErrors;
		string paymentMethod;
		string paymentMethodDescription;

		#endregion // Cached Fields

		NZ.TradeSingleWindow.PaymentMethodList PaymentMethodList
		{
			get { return Factory.GetCachedValue<NZ.TradeSingleWindow.PaymentMethodList>(); }
		}

		string GetPaymentMethodDescription(string code)
		{
			return PaymentMethodList.GetDescriptionFromCode(code) ?? "No Payment Terms Specified.";
		}

		#endregion // Implementation
	}
}
