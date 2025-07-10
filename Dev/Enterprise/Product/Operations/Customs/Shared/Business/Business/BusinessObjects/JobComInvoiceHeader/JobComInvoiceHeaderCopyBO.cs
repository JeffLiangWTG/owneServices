using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class JobComInvoiceHeaderCopyBO : NonPersistentBusinessObject
	{
		public JobComInvoiceHeaderCopyBO(BaseJobComInvoiceHeader sourceInvoice, BaseJobComInvoiceHeader destinationInvoice)
			: base(destinationInvoice.Factory)
		{
			this.destinationInvoice = destinationInvoice;
			this.sourceInvoice = sourceInvoice;
		}

		public void CopyInvoice()
		{
			CopyInvoiceHeader();
			CopyInvoiceLines();
		}

		void CopyInvoiceHeader()
		{
			var copyHeaderArgs = new BusinessObjectCloneArgs();
			copyHeaderArgs.AddExcludedColumns(ExcludedInvoiceHeaderColumns);
			destinationInvoice.CopyPersistentValuesFrom(sourceInvoice, copyHeaderArgs);
		}

		void CopyInvoiceLines()
		{
			var copyHeaderArgs = new BusinessObjectCloneArgs();
			copyHeaderArgs.AddExcludedColumns(ExcludedInvoiceLineColumns);
			foreach (BaseJobComInvoiceLine sourceInvoiceLine in GetInvoiceLinesToCopy())
			{
				var invoiceLine = destinationInvoice.InvoiceLines.AddNew();
				invoiceLine.CopyPersistentValuesFrom(sourceInvoiceLine, copyHeaderArgs);
				AfterCopyInvoiceLine(sourceInvoiceLine, invoiceLine);
			}
		}

		protected virtual IEnumerable<BaseJobComInvoiceLine> GetInvoiceLinesToCopy() => sourceInvoice.InvoiceLines.Cast<BaseJobComInvoiceLine>();

		protected virtual void AfterCopyInvoiceLine(BaseJobComInvoiceLine sourceInvoiceLine, BaseJobComInvoiceLine destinationInvoiceLine) { }

		string[] GetExcludeColumns(string tableName, string[] includedColumns)
		{
			return ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumns(tableName).Where(x => !includedColumns.Contains(x.Name)).Select(y => y.Name).ToArray();
		}

		string[] IncludedInvoiceHeaderColumns => new[]
		{
			JobComInvoiceHeaderSchema.Constants.JZ_OH_Supplier,
			JobComInvoiceHeaderSchema.Constants.JZ_OH_Buyer,
			JobComInvoiceHeaderSchema.Constants.JZ_InvoiceDate,
			JobComInvoiceHeaderSchema.Constants.JZ_InvoiceNumber,
			JobComInvoiceHeaderSchema.Constants.JZ_InvoiceAmount,
			JobComInvoiceHeaderSchema.Constants.JZ_IncoTerm,
			JobComInvoiceHeaderSchema.Constants.JZ_Volume,
			JobComInvoiceHeaderSchema.Constants.JZ_VolumeUQ,
			JobComInvoiceHeaderSchema.Constants.JZ_Weight,
			JobComInvoiceHeaderSchema.Constants.JZ_NetWeightUQ,
			JobComInvoiceHeaderSchema.Constants.JZ_RX_NKInvoice_Currency,
			JobComInvoiceHeaderSchema.Constants.JZ_NoOfPacks,
			JobComInvoiceHeaderSchema.Constants.JZ_IncoTermPlace
		};

		string[] ExcludedInvoiceHeaderColumns => excludedInvoiceHeaderColumns ?? (excludedInvoiceHeaderColumns = GetExcludeColumns(JobComInvoiceHeaderSchema.Constants.TableName, IncludedInvoiceHeaderColumns));
		string[] excludedInvoiceHeaderColumns;

		string[] IncludedInvoiceLineColumns => new[]
		{
			JobComInvoiceLineSchema.Constants.JI_LineNo,
			JobComInvoiceLineSchema.Constants.JI_PartNo,
			JobComInvoiceLineSchema.Constants.JI_PartAttrib1,
			JobComInvoiceLineSchema.Constants.JI_PartAttrib2,
			JobComInvoiceLineSchema.Constants.JI_PartAttrib3,
			JobComInvoiceLineSchema.Constants.JI_SerialNumber,
			JobComInvoiceLineSchema.Constants.JI_Description,
			JobComInvoiceLineSchema.Constants.JI_InvoiceQuantity,
			JobComInvoiceLineSchema.Constants.JI_InvoiceUQ,
			JobComInvoiceLineSchema.Constants.JI_LinePrice,
			JobComInvoiceLineSchema.Constants.JI_Volume,
			JobComInvoiceLineSchema.Constants.JI_VolumeUQ,
			JobComInvoiceLineSchema.Constants.JI_WeightUQ,
			JobComInvoiceLineSchema.Constants.JI_NetWeight,
			JobComInvoiceLineSchema.Constants.JI_NetWeightUQ,
			JobComInvoiceLineSchema.Constants.JI_CustomsQuantity,
			JobComInvoiceLineSchema.Constants.JI_CustomsUnitQty,
			JobComInvoiceLineSchema.Constants.JI_Tariff,
			JobComInvoiceLineSchema.Constants.JI_CountryOfOrigin,
			JobComInvoiceLineSchema.Constants.JI_RH_NKCommodity_Code,
			JobComInvoiceLineSchema.Constants.JI_OP,
			JobComInvoiceLineSchema.Constants.JI_BrandName,
			JobComInvoiceLineSchema.Constants.JI_Model
		};

		string[] ExcludedInvoiceLineColumns => excludedInvoiceLineColumns ?? (excludedInvoiceLineColumns = GetExcludeColumns(JobComInvoiceLineSchema.Constants.TableName, IncludedInvoiceLineColumns));
		string[] excludedInvoiceLineColumns;

		protected readonly BaseJobComInvoiceHeader destinationInvoice;
		protected readonly BaseJobComInvoiceHeader sourceInvoice;
	}
}
