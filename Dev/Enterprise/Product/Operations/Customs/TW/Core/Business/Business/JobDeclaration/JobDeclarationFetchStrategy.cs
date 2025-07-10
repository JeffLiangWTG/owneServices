using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Business
{
	public class JobDeclarationFetchStrategy : Customs.Business.FetchStrategies.BaseJobDeclarationFetchStrategy
	{
		public JobDeclarationFetchStrategy(JobDeclaration declaration)
			: base(declaration)
		{
		}

		JobDeclaration Declaration => BusinessObject as JobDeclaration;

		protected override void AddMergeFetchHintsFor(Customs.Business.BaseJobComInvoiceLine invoiceLine)
		{
			base.AddMergeFetchHintsFor(invoiceLine);
			Factory.AddFetchHint(CusSupportingInfoSchema.CSI_ParentID, invoiceLine.PK);
			Factory.AddFetchHint(JobComInvoiceLineTaxSchema.JLT_JI, invoiceLine.PK);
			Factory.AddFetchHint(JobComInvLineRefsSchema.JG_JI, invoiceLine.PK);
		}

		protected override void FetchForLoadChildEditableObjectsCore()
		{
			base.FetchForLoadChildEditableObjectsCore();
			Factory.AddFetchHint(CusCodeDataSchema.CY_ParentID, BusinessObject.PK);
			AddTrademarkImageFetchHints();
		}

		protected override IEnumerable<IFetchHint> GetJobComInvoiceLineRelatedFetchHints(IColumnIndexer row)
		{
			foreach (var hint in base.GetJobComInvoiceLineRelatedFetchHints(row))
			{
				yield return hint;
			}

			var invoiceLinePK = row.GetValue(JobComInvoiceLineSchema.PK);
			yield return new FetchHint(JobComInvLineRefsSchema.JG_JI, invoiceLinePK);
		}

		protected override void AddDocumentSupporterFetchHintsForJobDeclaration()
		{
			base.AddDocumentSupporterFetchHintsForJobDeclaration();
			Factory.AddFetchHint(CusEntryNumSchema.CE_ParentID, BusinessObject.PK);
		}

		protected override void FetchForValidateCore()
		{
			base.FetchForValidateCore();
			AddTrademarkImageFetchHints();
		}

		void AddTrademarkImageFetchHints()
		{
			var declaration = Declaration;
			Factory.AddFetchHint(typeof(OrgHeader), declaration.JE_OH_Importer);
			Factory.AddFetchHint(typeof(OrgHeader), declaration.JE_OH_Supplier);

			var docFactory = declaration?.TrademarkDocManagerInfo?.MasterFactory as BusinessObjectFactory;
			if (docFactory != null)
			{
				docFactory.AddFetchHint(StorageMainSchema.SM_ParentFK, declaration.PK);
				docFactory.AddFetchHint(StorageMainSchema.SM_ParentFK, declaration.JE_JS);
			}
			foreach (var invoice in declaration.Invoices.Cast<JobComInvoiceHeader>())
			{
				Factory.AddFetchHint(typeof(OrgHeader), invoice.JZ_OH_Supplier);
				docFactory?.AddFetchHint(StorageMainSchema.SM_ParentFK, invoice.PK);
			}
			foreach (var invoiceLine in Declaration.InvoiceLines.Cast<JobComInvoiceLine>())
			{
				if (!invoiceLine.JI_PartNo.IsEmpty)
				{
					Factory.AddFetchHint(OrgSupplierPartSchema.OP_PartNum, invoiceLine.JI_PartNo);
					var product = invoiceLine.Part;
					if (docFactory != null && product != null)
					{
						docFactory.AddFetchHint(StorageMainSchema.SM_ParentFK, product.PK);
					}
				}
				Factory.AddFetchHint(GenPivotSchema.XX_Relation1ID, invoiceLine.PK);
			}
		}

		protected override bool IsCusEntryInstructionRelatedColumn(string columnName)
		{
			return base.IsCusEntryInstructionRelatedColumn(columnName) || columnName == JobDeclaration.Schema.DeclarationDate;
		}

		protected override bool IsCusEntryHeaderRelatedColumn(string columnName)
		{
			return base.IsCusEntryHeaderRelatedColumn(columnName)
				|| columnName == JobDeclaration.Schema.ClearanceStatus
				|| columnName == JobDeclaration.Schema.AgencyResponseCode
				|| columnName == JobDeclaration.Schema.RequiredFormalitiesCode
				|| columnName == JobDeclaration.Schema.ClearanceCode;
		}

		protected override void FetchForViewDeclaration(TableColumn[] columns)
		{
			var declaration = Declaration;
			base.FetchForViewDeclaration(columns);
			foreach (var tableColumn in columns)
			{
				switch (tableColumn.ColumnName)
				{
					case JobDeclaration.Schema.ClearanceStatus:
						var entryHeader = declaration.EntryHeader;
						if (entryHeader != null)
						{
							Factory.AddFetchHint(CusEntryNumSchema.CE_ParentID, entryHeader.PK);
						}
						break;
					case JobDeclaration.Schema.ImporterName:
					case JobDeclaration.Schema.SupplierName:
						Factory.AddFetchHint(JobDocAddressSchema.E2_ParentID, BusinessObject.PK);
						break;
					case JobDeclaration.Schema.ImporterChineseName:
						Factory.AddFetchHint(JobDocAddressSchema.E2_ParentID, BusinessObject.PK);
						var importerAddress = declaration.ImporterDocumentaryAddress?.Address;
						if (importerAddress != null)
						{
							Factory.AddFetchHint(OrgTranslatedAddressSchema.OTA_OA, importerAddress.PK);
						}
						break;
					case JobDeclaration.Schema.SupplierChineseName:
						Factory.AddFetchHint(JobDocAddressSchema.E2_ParentID, BusinessObject.PK);
						var supplierAddress = declaration.SupplierDocumentaryAddress?.Address;
						if (supplierAddress != null)
						{
							Factory.AddFetchHint(OrgTranslatedAddressSchema.OTA_OA, supplierAddress.PK);
						}
						break;
					case JobDeclaration.Schema.DeclarationType:
						Factory.AddFetchHint(CusEntryInstructionSchema.CEI_JE, BusinessObject.PK);
						break;
					case JobDeclaration.Schema.AgencyResponseCode:
					case JobDeclaration.Schema.RequiredFormalitiesCode:
					case JobDeclaration.Schema.ClearanceCode:
						entryHeader = declaration.EntryHeader;
						if (entryHeader != null)
						{
							Factory.AddFetchHint(CusDispositionSchema.CDI_ParentID, entryHeader.PK);
						}
						break;
				}
			}
		}
	}
}
