using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.DataRegistry.Business;

namespace Enterprise.Customs.US.Business
{
	public class AutoEntrySummaryQuerySender
	{
		public void SendIfEligible(CusEntryHeader entry)
		{
			var declaration = entry.Declaration;
			if (USCustomsDataRegistry.Instance.AutoQueryEntrySummaries.GetValueWithoutFallback(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty) &&
				declaration.Liquidations.Count == 0) // sending allowed in registry and entry is not liquidated
			{
				var heldUntilDate = GetHeldUntilDate(declaration);

				var eSQ_AlreadyExists = entry.Messages.OfType<MQEDIMessage>()
					.FirstOrDefault(x => x.IsEntrySummaryQuery && x.RelatedMessage != null &&
						x.RelatedMessage.IsEntrySummaryQuerySuccessful &&
						x.RelatedMessage.EM_SystemCreateTimeUtc >= heldUntilDate) != null;

				if (!eSQ_AlreadyExists)
				{
					Send(entry, heldUntilDate);
				}
			}
		}

		void Send(CusEntryHeader entry, ZDateTime heldUntilDate)
		{
			var message = GetHeldQueryMessage(entry);

			if (message == null)
			{
				var builder = new ACEEntrySummaryQueryMessageBuilder(entry);
				message = builder.PopulateMessage();
			}

			if (message != null)
			{
				message.EM_HeldUntilDate = heldUntilDate;
			}
		}

		ZDateTime GetHeldUntilDate(JobDeclaration declaration)
		{
			var now = ZDateTime.Now;
			var timespanNow = new TimeSpan(now.Hour, now.Minute, 0);
			var year = now.Year;
			var periodicStatementMonth = ZInt.ParseSafe(declaration.US_PeriodicStatementMM, 0);

			if (PaymentTypeList.IsPeriodicPayment(declaration.US_PaymentType) && periodicStatementMonth >= 1 && periodicStatementMonth <= 12)
			{
				var periodicPaymentHeldUntilDate = new ZDateTime(year, periodicStatementMonth, EntryTypeList.IsInformal(declaration.US_EntryType) ? DateTime.DaysInMonth(year, periodicStatementMonth) : PeriodicPaymentHeldUntilDateDays);
				if (ZDateTime.Today >= periodicPaymentHeldUntilDate)
				{
					periodicPaymentHeldUntilDate = periodicPaymentHeldUntilDate.AddYears(1);
				}

				return periodicPaymentHeldUntilDate.Add(timespanNow);
			}
			else
			{
				var dueDate = declaration.US_PaymentDueDate.IsValid ? declaration.US_PaymentDueDate : ZDateTime.Today;
				return dueDate.AddDays(HeldUntilDateDays).Add(timespanNow);
			}
		}

		internal const int HeldUntilDateDays = 3;

		const int PeriodicPaymentHeldUntilDateDays = 28;

		MQEDIMessage GetHeldQueryMessage(CusEntryHeader entry)
		{
			return entry.Messages.OfType<MQEDIMessage>().
				FirstOrDefault(x => x.IsEntrySummaryQuery && x.IsTransmitMessage && x.RelatedMessage == null &&
				x.EM_HeldUntilDate.IsValid && x.EM_Status == MQEDIMessage.Status.Queued);
		}
	}
}
