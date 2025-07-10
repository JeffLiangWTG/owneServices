using System;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(nameof(CAH_RequestReferenceNumber))]
	[DescriptionProperty(nameof(HumanReadableName))]
	[UniversalDataContext(DataContextType.AccCashAdvanceRequest)]
	public class AccCashAdvanceRequestHeader : AutoAccCashAdvanceRequestHeader, IDataVersionLoggingSupported, IDocumentSupportable
	{
		public AccCashAdvanceRequestHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(CAH_LocalPaidAmount), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(CAH_OSPaidAmount), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(CAH_Status), ConcurrencyPolicy.Strict);
		}

		#region IDataVersionLoggingSupported

		bool IDataVersionLoggingSupported.IsDataVersionsAutoLogged => true;

		DataVersionLogValueFormatter IDataVersionLoggingSupported.DataVersionLogValueFormatter
		{
			get { return this.GetDefaultDataVersionLogFormatter(); }
		}

		#endregion

		[List("Lookups.StatusCodeList")]
		public override ZString CAH_Status
		{
			get { return base.CAH_Status; }
			set { base.CAH_Status = value; }
		}

		public ZString StatusDescription => CashAdvanceStatusCodes.RequestHeader.CodesList.ContainsCode(CAH_Status) ? $"{CAH_Status} - {CashAdvanceStatusCodes.RequestHeader.CodesList[CAH_Status].Description}" : string.Empty;

		[DecimalPlaces(nameof(OSRXDecimals))]
		public override ZDecimal CAH_OSAmount { get => base.CAH_OSAmount; set => base.CAH_OSAmount = value; }

		[DecimalPlaces(nameof(OSRXDecimals))]
		public override ZDecimal CAH_OSPaidAmount { get => base.CAH_OSPaidAmount; set => base.CAH_OSPaidAmount = value; }

		[DecimalPlaces(nameof(LocalRXDecimals))]
		public override ZDecimal CAH_LocalAmount { get => base.CAH_LocalAmount; set => base.CAH_LocalAmount = value; }

		[DecimalPlaces(nameof(LocalRXDecimals))]
		public override ZDecimal CAH_LocalPaidAmount { get => base.CAH_LocalPaidAmount; set => base.CAH_LocalPaidAmount = value; }

		public int OSRXDecimals => TransactionCurrency?.Decimals ?? LocalRXDecimals;

		public int LocalRXDecimals => GlbCompany.CurrentCompany.LocalCurrency.Decimals;

		public ZString OrganizationCode => Organization?.OH_Code ?? ZString.Empty;

		public ZString OrganizationName => Organization?.OH_FullName ?? ZString.Empty;

		public ZString JobNumber => Job?.JH_JobNum ?? ZString.Empty;

		public virtual ZString JobBranch => Factory.Load<GlbBranch>(Job?.JH_GB ?? ZGuid.Empty)?.GB_Code ?? ZString.Empty;

		public virtual ZString JobDepartment => Factory.Load<GlbDepartment>(Job?.JH_GE ?? ZGuid.Empty)?.GE_Code ?? ZString.Empty;

		public ZDateTime CreatedDateTimeLocal
		{
			get
			{
				if (CAH_SystemCreateTimeUtc.IsValid)
				{
					return EnvProxy.Instance.Time.GetLocalTimeFromUtc(CAH_SystemCreateTimeUtc.ToDateTime());
				}
				else
				{
					return ZDateTime.Empty;
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1194:Unsafe BusinessObjectCollection Creation", Justification = "Suppressing to avoid stack overflow")]
		public AccCashAdvanceRequestLineCollection Lines
		{
			get
			{
				if (lines == null)
				{
					lines = new AccCashAdvanceRequestLineCollection(Factory);
					lines.Load(new ZQuery(AccCashAdvanceRequestLineSchema.CAL_CAH_RequestHeader, PK));
				}
				return lines;
			}
		}
		AccCashAdvanceRequestLineCollection lines;

		public ZString CancelRequest()
		{
			if (CAH_Status == CashAdvanceStatusCodes.RequestHeader.Requested)
			{
				Lines.Cast<AccCashAdvanceRequestLine>().ForEach(l => l.Cancel());
				return ZString.Empty;
			}
			else if (CAH_Status == CashAdvanceStatusCodes.RequestHeader.PartiallyInvoiced || CAH_Status == CashAdvanceStatusCodes.RequestHeader.Invoiced)
			{
				return Res.GetString("30c21dbb-cd71-4c75-a83a-322a59f5a58b", "The Advance Payment Request has been invoiced. Before canceling a Advance Payment Request, the invoice and Advance Payment payment must be reversed.");
			}
			else if (CAH_Status == CashAdvanceStatusCodes.RequestHeader.PartiallyPaid || CAH_Status == CashAdvanceStatusCodes.RequestHeader.Paid)
			{
				return Res.GetString("9a4f04e2-6483-4601-be0c-71dca457440c", "The Advance Payment Request has been paid. Before canceling a Advance Payment Request, the Advance Payment payment must be reversed.");
			}
			else
			{
				return Res.GetString("e610d53f-a0fd-4729-ae4d-b28d199bb98c", "Only Advance Payment Requests in the REQ - Requested status can be canceled.");
			}
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			Factory.SetContext(BusinessContext.JobCreatedFromJobLoader);
			base.FillWithValidTestDataCore(kind, propertyPath);
			Factory.RemoveContext(BusinessContext.JobCreatedFromJobLoader);
		}
#endif

		public bool IsRequested => CAH_Status == CashAdvanceStatusCodes.RequestHeader.Requested;

		public bool CanBeMarkedAsPaid => CAH_Status == CashAdvanceStatusCodes.RequestHeader.Requested || CAH_Status == CashAdvanceStatusCodes.RequestHeader.PartiallyPaid;

		[DecimalPlaces(nameof(OSRXDecimals))]
		public ZDecimal CAH_OSOutstandingAmount => GetOutstandingAmount(CAH_OSAmount, CAH_OSPaidAmount);

		[DecimalPlaces(nameof(LocalRXDecimals))]
		public ZDecimal CAH_LocalOutstandingAmount => GetOutstandingAmount(CAH_LocalAmount, CAH_LocalPaidAmount);

		ZDecimal GetOutstandingAmount(ZDecimal amount, ZDecimal paidAmount)
		{
			switch (CAH_Status)
			{
				case CashAdvanceStatusCodes.RequestHeader.Requested:
					return amount;
				case CashAdvanceStatusCodes.RequestHeader.Cancelled:
				case CashAdvanceStatusCodes.RequestHeader.PartiallyInvoiced:
				case CashAdvanceStatusCodes.RequestHeader.Invoiced:
					return ZDecimal.Zero;
				case CashAdvanceStatusCodes.RequestHeader.Paid:
				case CashAdvanceStatusCodes.RequestHeader.PartiallyPaid:
				case "":
					return amount - paidAmount;
				default:
					throw new InvalidOperationException(FormattableString.Invariant($"Invalid status: {CAH_Status}"));
			}
		}

		public (bool IsSuccessful, string ErrorMessage) MarkAsPaid(bool updatedViaMatchingJournal = false) =>
				MarkAsPaidOrUnpaid((l) => l.MarkAsPaid()
								, Res.GetString("b5128146-0008-4494-8d08-ec01c908584c", "paid")
								, CashAdvanceStatusCodes.RequestHeader.Requested
								, Res.GetString("62ea8156-e598-48ec-99ee-dee54c36733f", "requested")
								, updatedViaMatchingJournal);

		public (bool IsSuccessful, string ErrorMessage) UndoPaidStatus(bool updatedViaMatchingJournal = false) =>
				MarkAsPaidOrUnpaid((l) => l.UndoPaidStatus()
								, Res.GetString("b5ed7fdf-ba9c-4fd0-9883-f0cf65921f61", "unpaid")
								, CashAdvanceStatusCodes.RequestHeader.Paid
								, Res.GetString("2451f241-5f9e-426c-ae92-f0aa69ce0fe5", "paid")
								, updatedViaMatchingJournal);

		(bool IsSuccessful, string ErrorMessage) MarkAsPaidOrUnpaid(Action<AccCashAdvanceRequestLine> action, string operationName, string preRequisiteStatus, string preRequisiteStatusDescription, bool updatedViaMatchingJournal)
		{
			var cashAdvanceFunctionalityChecker = ObjectFactory.Get<IAccCashAdvanceFunctionalityChecker>();
			if (!IsFunctionalityEnabled())
			{
				return (false, Res.GetString("6566b660-6e13-401c-944b-8c7b2eff4868", "Advance Payment cannot be marked as {0}, because Advance Payment functionality is disabled.", operationName));
			}
			if (!IsManualSettingOfCashAdvanceRequestStatusToPaidAllowed() && !updatedViaMatchingJournal)
			{
				return (false, Res.GetString("36eafeef-ce4f-4989-b6fc-530955659fa8", "Advance Payment cannot be marked as {0}, because manual updating of Advance Payment request to paid is not allowed.", operationName));
			}
			if (CAH_Status != preRequisiteStatus)
			{
				return (false, Res.GetString("69cb3932-846e-4011-aff1-7a9a588e3f27", "Advance Payment is not in {0} status.", preRequisiteStatusDescription));
			}

			using (UpdateFromLineSuspender.GetSuspender())
			{
				foreach (AccCashAdvanceRequestLine line in Lines)
				{
					action(line);
				}
			}

			UpdatePaidAmount();
			UpdateStatusFromLine();
			AddLogsOnMarkAsPaidOrUnPaid();
			return (true, string.Empty);

			bool IsFunctionalityEnabled()
			{
				if (CAH_Ledger == LedgerTypes.AccountsReceivable)
				{
					return cashAdvanceFunctionalityChecker.IsReceivablesCashAdvanceFunctionalityEnabled;
				}
				else if (CAH_Ledger == LedgerTypes.AccountsPayable)
				{
					return cashAdvanceFunctionalityChecker.IsPayablesCashAdvanceFunctionalityEnabled;
				}
				return false;
			}

			bool IsManualSettingOfCashAdvanceRequestStatusToPaidAllowed()
			{
				if (CAH_Ledger == LedgerTypes.AccountsReceivable)
				{
					return cashAdvanceFunctionalityChecker.IsManualSettingOfReceivablesCashAdvanceRequestStatusToPaidAllowed;
				}
				else if (CAH_Ledger == LedgerTypes.AccountsPayable)
				{
					return cashAdvanceFunctionalityChecker.IsManualSettingOfPayablesCashAdvanceRequestStatusToPaidAllowed;
				}
				return false;
			}
		}
		FunctionalitySuspender UpdateFromLineSuspender => updateFromLineSuspender ?? (updateFromLineSuspender = new FunctionalitySuspender());
		FunctionalitySuspender updateFromLineSuspender;

		public void UpdateStatusFromLine()
		{
			if (!UpdateFromLineSuspender.IsSuspended)
			{
				var calculatedStatus = ZString.Empty;
				var lines = Lines.OfType<AccCashAdvanceRequestLine>().ToList();
				var hasAnyCancelledLine = lines.Any(l => l.IsCancelled);
				if (hasAnyCancelledLine)
				{
					var allLinesAreCancelled = lines.All(l => l.IsCancelled);
					if (allLinesAreCancelled)
					{
						CAH_Status = CashAdvanceStatusCodes.RequestHeader.Cancelled;
					}
					else
					{
						if (lines.Any(l => l.IsPaid || l.IsInvoiced))
						{
							throw new InvalidOperationException("There are Paid/Invoiced Advance Payment request lines. Therefore, Advance Payment request should not be allowed to be cancelled.");
						}
						else
						{
							CAH_Status = ZString.Empty;
						}
					}
				}
				else
				{
					var hasAnyInvoicedLine = lines.Any(l => l.IsInvoiced);
					if (hasAnyInvoicedLine)
					{
						CAH_Status = lines.All(l => l.IsInvoiced) ? CashAdvanceStatusCodes.RequestHeader.Invoiced : CashAdvanceStatusCodes.RequestHeader.PartiallyInvoiced;
					}
					else
					{
						var hasAnyPaidLines = lines.Any(l => l.IsPaid);
						if (hasAnyPaidLines)
						{
							CAH_Status = lines.All(l => l.IsPaid) ? CashAdvanceStatusCodes.RequestHeader.Paid : CashAdvanceStatusCodes.RequestHeader.PartiallyPaid;
						}
						else
						{
							CAH_Status = lines.All(l => l.IsRequested) ? CashAdvanceStatusCodes.RequestHeader.Requested : string.Empty;
						}
					}
				}
			}
		}

		internal void UpdatePaidAmount()
		{
			if (!UpdateFromLineSuspender.IsSuspended)
			{
				var lines = Lines.OfType<AccCashAdvanceRequestLine>().ToList();
				CAH_OSPaidAmount = lines.Sum(l => l.CAL_OSPaidAmount);
				CAH_LocalPaidAmount = lines.Sum(l => l.CAL_LocalPaidAmount);
			}
		}

		void AddLogsOnMarkAsPaidOrUnPaid()
		{
			var lines = Lines.OfType<AccCashAdvanceRequestLine>().ToList();

			if (lines.All(l => l.IsPaid))
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Job?.Logs.AddNew(new EventValue(Events.EditedARecord, eventTime: ZDateTimeOffset.Now, reference: $"Advance Payment {CAH_RequestReferenceNumber} has been paid in full."));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
			else if (lines.All(l => l.IsRequested))
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Job?.Logs.AddNew(new EventValue(Events.EditedARecord, eventTime: ZDateTimeOffset.Now, reference: $"Advance Payment {CAH_RequestReferenceNumber} Payment Status has been reset."));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
		}

		#region IDocumentSupportable Members
		DocumentSupporter IDocumentSupportable.DocumentSupporter
		{
			get { return CashAdvanceRequestDocumentSupporter; }
		}

		public CashAdvanceRequestDocumentSupporter CashAdvanceRequestDocumentSupporter
		{
			get
			{
				if (fDocumentSupporter == null)
				{
					fDocumentSupporter = new CashAdvanceRequestDocumentSupporter(this);
				}
				return fDocumentSupporter;
			}
		}
		CashAdvanceRequestDocumentSupporter fDocumentSupporter;
		#endregion
	}
}
