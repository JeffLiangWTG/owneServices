using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class AccCashAdvanceRequestLine : AutoAccCashAdvanceRequestLine
	{
		public AccCashAdvanceRequestLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(CAL_Status), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(CAL_LocalPaidAmount), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(CAL_OSPaidAmount), ConcurrencyPolicy.Strict);
		}

		public JobCharge RelatedJobCharge
		{
			get
			{
				if (relatedJobCharge == null)
				{
					var filterColumn = GetJobChargeLinkColumn();
					if (filterColumn != null)
					{
						var query = new ZQuery(filterColumn, PK);
						relatedJobCharge = Factory.LoadTop1<JobCharge>(query);
					}
				}
				return relatedJobCharge;
			}
		}
		JobCharge relatedJobCharge;

		public ZString RelatedChargeCode => RelatedJobCharge?.ChargeCode?.AC_Code ?? ZString.Empty;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007:Customizable Data Translation Rule", Justification = "Baseline")]
		public ZString RelatedChargeDescription => RelatedJobCharge?.ChargeCode?.AC_Desc ?? ZString.Empty;

		public ZString OrganizationCode => RequestHeader?.OrganizationCode ?? ZString.Empty;

		public ZString OrganizationName => RequestHeader?.OrganizationName ?? ZString.Empty;

		public ZString Currency => RequestHeader?.CAH_RX_NKTransactionCurrency ?? ZString.Empty;

		public ZString StatusDescription => CashAdvanceStatusCodes.RequestLine.CodesList.ContainsCode(CAL_Status) ? $"{CAL_Status} - {CashAdvanceStatusCodes.RequestLine.CodesList[CAL_Status].Description}" : string.Empty;

		[DecimalPlaces(nameof(LocalRXDecimals))]
		public override ZDecimal CAL_LocalAmount { get => base.CAL_LocalAmount; set => base.CAL_LocalAmount = value; }

		[DecimalPlaces(nameof(LocalRXDecimals))]
		public override ZDecimal CAL_LocalPaidAmount
		{
			get { return base.CAL_LocalPaidAmount; }
			set
			{
				if (base.CAL_LocalPaidAmount != value)
				{
					base.CAL_LocalPaidAmount = value;
					if (RequestHeader != null && !HeaderStatusEvaluationSuspender.IsSuspended)
					{
						RequestHeader.UpdatePaidAmount();
					}
				}
			}
		}

		[DecimalPlaces(nameof(OSRXDecimals))]
		public override ZDecimal CAL_OSAmount { get => base.CAL_OSAmount; set => base.CAL_OSAmount = value; }

		[DecimalPlaces(nameof(OSRXDecimals))]
		public override ZDecimal CAL_OSPaidAmount
		{
			get { return base.CAL_OSPaidAmount; }
			set
			{
				if (base.CAL_OSPaidAmount != value)
				{
					base.CAL_OSPaidAmount = value;
					if (RequestHeader != null && !HeaderStatusEvaluationSuspender.IsSuspended)
					{
						RequestHeader.UpdatePaidAmount();
					}
				}
			}
		}

		public override ZString CAL_Status
		{
			get { return base.CAL_Status; }
			set
			{
				if (base.CAL_Status != value)
				{
					base.CAL_Status = value;
					if (RequestHeader != null && !HeaderStatusEvaluationSuspender.IsSuspended)
					{
						RequestHeader.UpdateStatusFromLine();
					}
				}
			}
		}

		public ZDecimal CAL_OSOutstandingAmount => GetOutstandingAmount(CAL_OSAmount, CAL_OSPaidAmount);

		[DecimalPlaces(nameof(LocalRXDecimals))]
		public ZDecimal CAL_LocalOutstandingAmount => GetOutstandingAmount(CAL_LocalAmount, CAL_LocalPaidAmount);

		ZDecimal GetOutstandingAmount(ZDecimal amount, ZDecimal paidAmount)
		{
			switch (CAL_Status)
			{
				case CashAdvanceStatusCodes.RequestLine.Requested:
					return amount;
				case CashAdvanceStatusCodes.RequestLine.Cancelled:
				case CashAdvanceStatusCodes.RequestLine.Invoiced:
					return ZDecimal.Zero;
				case CashAdvanceStatusCodes.RequestLine.Paid:
					return amount - paidAmount;
				default:
					throw new InvalidOperationException(FormattableString.Invariant($"Invalid status: {CAL_Status}"));
			}
		}

		public int OSRXDecimals => RequestHeader?.OSRXDecimals ?? LocalRXDecimals;

		public int LocalRXDecimals => GlbCompany.CurrentCompany.LocalCurrency.Decimals;

		public bool IsRequested => CAL_Status == CashAdvanceStatusCodes.RequestLine.Requested;

		public bool IsPaid => CAL_Status == CashAdvanceStatusCodes.RequestLine.Paid;

		public bool IsInvoiced => CAL_Status == CashAdvanceStatusCodes.RequestLine.Invoiced;

		public bool IsCancelled => CAL_Status == CashAdvanceStatusCodes.RequestLine.Cancelled;

		public void Cancel()
		{
			if (IsRequested)
			{
				CAL_Status = CashAdvanceStatusCodes.RequestLine.Cancelled;
				CAL_LocalPaidAmount = 0;
				CAL_OSPaidAmount = 0;
				if (RelatedJobCharge != null)
				{
					RelatedJobCharge.ClearCashAdvanceRequestLineLink(RequestHeader?.CAH_Ledger ?? ZString.Empty);
				}
			}
			else
			{
				throw new InvalidOperationException($"Status cannot be updated to '{CashAdvanceStatusCodes.RequestLine.Cancelled}'. Current status is '{CAL_Status}'");
			}
		}

		public void MarkAsInvoiced()
		{
			if (IsPaid &&
				CAL_OSAmount == CAL_OSPaidAmount &&
				CAL_LocalAmount == CAL_LocalPaidAmount)
			{
				CAL_Status = CashAdvanceStatusCodes.RequestLine.Invoiced;
			}
			else
			{
				throw new InvalidOperationException("Status cannot be updated to 'Invoiced', as either Advance Payment line is not paid or outstanding amount is not zero.");
			}
		}

		public void UndoInvoicedStatus()
		{
			if (IsInvoiced &&
				CAL_OSAmount == CAL_OSPaidAmount &&
				CAL_LocalAmount == CAL_LocalPaidAmount)
			{
				CAL_Status = CashAdvanceStatusCodes.RequestLine.Paid;
			}
			else
			{
				throw new InvalidOperationException("Status cannot be changed from 'Invoiced' to 'Paid', as either Advance Payment line is not in 'Invoiced' status or outstanding amount is not zero.");
			}
		}

		internal void MarkAsPaid()
		{
			if (IsRequested)
			{
				CAL_LocalPaidAmount = CAL_LocalAmount;
				CAL_OSPaidAmount = CAL_OSAmount;
				CAL_Status = CashAdvanceStatusCodes.RequestLine.Paid;
			}
			else
			{
				throw new InvalidOperationException("Status cannot be updated to 'Paid', as Advance Payment line is not in requested status.");
			}
		}

		internal void UndoPaidStatus()
		{
			CAL_LocalPaidAmount = 0M;
			CAL_OSPaidAmount = 0M;
			CAL_Status = CashAdvanceStatusCodes.RequestLine.Requested;
		}

		SchemaGuidColumn GetJobChargeLinkColumn() => RequestHeader != null ? JobCharge.GetCashAdvanceRequestLineFKColumn(RequestHeader.CAH_Ledger) : null;

		public FunctionalitySuspender HeaderStatusEvaluationSuspender => headerStatusEvaluationSuspender ?? (headerStatusEvaluationSuspender = new FunctionalitySuspender());
		FunctionalitySuspender headerStatusEvaluationSuspender;
	}
}
