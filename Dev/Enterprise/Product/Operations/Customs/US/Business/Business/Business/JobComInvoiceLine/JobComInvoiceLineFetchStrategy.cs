using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class JobComInvoiceLineFetchStrategy : Customs.Business.FetchStrategies.JobComInvoiceLineFetchStrategy
	{
		public JobComInvoiceLineFetchStrategy(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
		}

		JobComInvoiceLine invoiceLine => BusinessObject as JobComInvoiceLine;

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(StmNoteSchema.ST_ParentID, invoiceLine.JI_OP);
			Factory.AddFetchHint(JobDocAddressSchema.E2_ParentID, BusinessObject.PK);
			Factory.AddFetchHint(CusLineTariffDetailSchema.BZ_ParentID, invoiceLine.PK);
			if (invoiceLine.IsDrawback && invoiceLine.UseViewForDrawbackEntryLine)
			{
				var query = new ZQuery(USImportEntryLineSchema.USE_EntryFilerCode, invoiceLine.DrawbackImportDeclarationEntryFilerCode);
				query.AddToFilter(USImportEntryLineSchema.USE_EntryNum, invoiceLine.DrawbackImportDeclarationNumber);
				query.AddToFilter(USImportEntryLineSchema.USE_LineNumber, (ZShort)invoiceLine.DrawbackImportDeclarationLine);
				query.AddToFilter(USImportEntryLineSchema.USE_SupLine, ZBool.False);
				Factory.AddFetchHint(USImportEntryLineSchema.Instance, query);
			}
		}

		protected override void FetchForLoadChildEditableObjectsCore()
		{
			base.FetchForLoadChildEditableObjectsCore();
			var invoiceLine = (JobComInvoiceLine)BusinessObject;
			var dec = invoiceLine.Declaration;
			if (dec == null || !dec.IsRecon)
			{
				Factory.AddFetchHint(typeof(CusDisposition), new ZQuery(CusDispositionSchema.CDI_Type, CusDispositionTypeCodeList.Codes.USPGALineStatus), new ZQuery(CusDispositionSchema.CDI_ParentID, BusinessObject.PK));
			}

			Factory.AddFetchHint(JobComInvoiceLineSchema.JI_ParentID, BusinessObject.PK);
			Factory.AddFetchHint(JobComInvoiceLineSchema.PK, invoiceLine.US_JI_ParentProduct);
			Factory.AddFetchHint(JobComInvoiceLineSchema.PK, BusinessObject.PK);
			Factory.AddFetchHint(CusCodeDataSchema.CY_ParentID, invoiceLine.PK);
		}

		protected override void FetchForValidateCore()
		{
			base.FetchForValidateCore();
			Factory.AddFetchHint(CusEntryLineSchema.PK, invoiceLine.JI_CL);
			Factory.AddFetchHint(CusUnderbondDecSchema.BU_JI, invoiceLine.PK);
			Factory.AddFetchHint(CusClassificationSchema.PK, invoiceLine.JI_CC);
			Factory.AddFetchHint(CusLineTariffDetailSchema.BZ_ParentID, invoiceLine.PK);
		}

		protected override void FetchForDeleteCore()
		{
			base.FetchForDeleteCore();
			Factory.AddFetchHint(StmNoteSchema.ST_ParentID, BusinessObject.PK);
			Factory.AddFetchHint(StmUniversalCopySchema.SUC_CopyObjectId, BusinessObject.PK);
		}
	}
}
