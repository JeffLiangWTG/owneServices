using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Rating.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Transactions.Invoicing;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class ChangeOffBandProcessingStatusActionMethodApplicator : WhsOperationalActionMethodApplicator
	{
		#region ctor

		public ChangeOffBandProcessingStatusActionMethodApplicator(BusinessObjectFactory factory)
			: base(Res.GetString("5BEC7A74-1585-417B-BAFA-967FDF8FD156", "Change Off Band Processing Status"), factory)
		{
		}

		#endregion

		#region ApplyCore

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] invoices)
		{
			log.SetSectionProgressMax(invoices.Length);

			foreach (JobStorage invoice in invoices)
			{
				ApplyStatus(invoice, log);
				log.BumpSectionProgress();
			}
		}

		void ApplyStatus(JobStorage invoice, IOperationalActionSectionLog log)
		{
			var invoiceLink = GetInvoiceIdLink(invoice);

			invoice.ET_OffBandProcessingStatus = SelectedOffBandProcessingStatus;
			if (invoice.HasErrors)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, OutputTextFormat, invoice.ET_StorageJobNumber, invoiceLink, invoice.Notifications.ToUniqueMessageListString());
				invoice.ET_OffBandProcessingStatus = (ZString)invoice.ET_OffBandProcessingStatusInfo.OriginalValue; // undo
			}
			else
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Informational, OutputTextFormat, invoice.ET_StorageJobNumber, invoiceLink, Res.GetString("a16f5329-3b9e-44ea-bd86-cb5cf8cc2ffb", "Processing Status Was Successfully Changed."));
			}
		}

		#endregion

		#region SelectedOffBandProcessingStatus

		[MaxLength(WhsInvoice.Schema.ET_OffBandProcessingStatusMaxLength)]
		[List(nameof(StorageOffBandProcessingStatuses))]
		[ResourceStringData("ChangeOffBandProcessingStatusActionMethodApplicator|SelectedOffBandProcessingStatus", Caption = "New Off Band Processing Status")]
		public ZString SelectedOffBandProcessingStatus
		{
			get { return selectedOffBandProcessingStatus; }
			set
			{
				CheckMaximumLength(SelectedOffBandProcessingStatusInfo, value);
				SetNonPersistentPropertyValue(SelectedOffBandProcessingStatusInfo, ref selectedOffBandProcessingStatus, value, setValueOnlyIfDifferentToGetter: false);
				Validation.ValidateSelectedOffBandProcessingStatus();
			}
		}

		public ZPropertyInfo SelectedOffBandProcessingStatusInfo => GetZPropertyInfo(nameof(SelectedOffBandProcessingStatus));

		ZString selectedOffBandProcessingStatus;

		#endregion

		#region Lookups

		public StorageOffBandProcessingStatus StorageOffBandProcessingStatuses => Factory.GetCachedValue(nameof(StorageOffBandProcessingStatus), () => new StorageOffBandProcessingStatus());

		#endregion

		#region Validation

		public ChangeOffBandProcessingStatusValidation Validation => new ChangeOffBandProcessingStatusValidation(this);

		#endregion

		#region OutputTextFormat

		const string OutputTextFormat = "{0} {1} - {2}"; // eg. I00000001 [HL I00000001] - Processing Status Was Successfully Changed.

		#endregion
	}
}
