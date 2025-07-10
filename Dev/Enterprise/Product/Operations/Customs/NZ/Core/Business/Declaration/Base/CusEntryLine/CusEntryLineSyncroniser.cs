using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class CusEntryLineSyncroniser : NonPersistentBusinessObject, IObsoleteValidation
	{
		public CusEntryLineSyncroniser(CusEntryLine entryLine)
			: base(entryLine.Factory)
		{
			this.entryLine = entryLine;
			var invoiceLine = this.entryLine.RandomLine;
			clonedInvoiceLine = new BusinessObjectFactory().New<JobComInvoiceLine>();
			SyncInvoiceLine(invoiceLine, clonedInvoiceLine);
		}
		readonly CusEntryLine entryLine;
		readonly JobComInvoiceLine clonedInvoiceLine;

		public InvoiceLinesForEntryLineCollection EffectedLines
		{
			get { return entryLine.InvoiceLines; }
		}

		public JobComInvoiceLine InvoiceLine
		{
			get { return clonedInvoiceLine; }
		}

		#region Syncronisers
		void SyncInvoiceLine(JobComInvoiceLine source, JobComInvoiceLine destination)
		{
			SyncCodeDataPairCollection(source.PermitCodes, destination.PermitCodes);
			SyncCodeDataPairCollection(source.ProhibitedCodes, destination.ProhibitedCodes);
			SyncCodeDataPairCollection(source.OtherInfos, destination.OtherInfos);
			destination.JI_ConcessionCode = source.JI_ConcessionCode;
		}

		void SyncCodeDataPairCollection(CodeDataPairCollection source, CodeDataPairCollection destination)
		{
			destination.RemoveAndDeleteAll();
			foreach (CodeDataPair pair in source)
			{
				destination.AddNew(pair.ZO_Code, pair.ZO_Data);
			}
		}

		public void SyncAllInvoiceLinesOnEntryLine()
		{
			foreach (JobComInvoiceLine invoiceLine in entryLine.InvoiceLines)
			{
				SyncInvoiceLine(clonedInvoiceLine, invoiceLine);
			}
		}
		#endregion
	}
}
