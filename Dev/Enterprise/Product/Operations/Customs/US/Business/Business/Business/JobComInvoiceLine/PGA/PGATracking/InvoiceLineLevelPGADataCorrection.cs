using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.Business
{
	abstract class InvoiceLineLevelPGADataCorrection : IPGADataCorrection
	{
		protected InvoiceLineLevelPGADataCorrection(JobComInvoiceLine invoiceLine)
		{
			this.invoiceLine = Argument.NotNull(invoiceLine, "invoiceLine");
		}

		event EventHandler<HasChangesChangedEventArgs> IPGADataCorrection.HasChangesChanged
		{
			add { }
			remove { }
		}

		public JobComInvoiceLine InvoiceLine
		{
			get { return invoiceLine; }
		}

		public JobComInvoiceHeader InvoiceHeader
		{
			get { return InvoiceLine != null ? InvoiceLine.InvoiceHeader : null; }
		}

		readonly JobComInvoiceLine invoiceLine;

		bool IPGADataCorrection.SuspendTrackingStatusChange
		{
			get { return false; }
		}

		bool IPGADataCorrection.SettingStatusInProgress
		{
			get { return settingPGATrackingStatusInProgress; }
			set { settingPGATrackingStatusInProgress = value; }
		}
		bool settingPGATrackingStatusInProgress;

		ZPropertyInfo IPGADataCorrection.TrackingStatusInfo
		{
			get { return GetTrackingStatusInfo(); }
		}

		protected abstract ZPropertyInfo GetTrackingStatusInfo();

		string[] IPGADataCorrection.GetRelatedInvoiceLineFields()
		{
			return GetRelatedInvoiceLineFields();
		}

		protected abstract string[] GetRelatedInvoiceLineFields();

		string[] IPGADataCorrection.GetRelatedInvoiceFields()
		{
			return Array.Empty<string>();
		}

		string[] IPGADataCorrection.GetRelatedContainerFields()
		{
			return Array.Empty<string>();
		}

		string[] IPGADataCorrection.GetRelatedDeclarationFields()
		{
			return Array.Empty<string>();
		}

		string[] IPGADataCorrection.GetIndicatorFields()
		{
			return GetIndicatorFields();
		}
		protected abstract string[] GetIndicatorFields();

		string[] IPGADataCorrection.GetDislaimReasonFields()
		{
			return GetDislaimReasonFields();
		}
		protected abstract string[] GetDislaimReasonFields();

		#region IPGALineStatus Members

		ZString IPGALineStatus.PGALineStatusAgencyCode
		{
			get { return IPGALineStatusGovernmentAgencyCode; }
		}
		protected virtual ZString IPGALineStatusGovernmentAgencyCode
		{
			get { return ZString.Empty; }
		}

		ZInt IPGALineStatus.PGALineNumber
		{
			get { return IPGALineNumber; }
		}
		protected virtual ZInt IPGALineNumber
		{
			get { return ZInt.Zero; }
		}

		CusDispositionCollection IPGALineStatus.PGALineCusDispositions
		{
			get { return InvoiceLine.PGALineCusDispositions; }
		}

		#endregion
	}
}
