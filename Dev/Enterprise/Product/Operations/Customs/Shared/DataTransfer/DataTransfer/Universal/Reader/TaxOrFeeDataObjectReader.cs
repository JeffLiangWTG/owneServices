using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class TaxOrFeeDataObjectReader : DataObjectReader<UniversalCustoms.TaxOrFee>  // Thank you Ding Dong
	{
		internal TaxOrFeeDataObjectReader(UniversalCustoms.TaxOrFee taxOrFee, IXmlImportLogger logger, UniversalObjectFactory factory, ZGuid parentPK)
			: base(taxOrFee, logger, factory)
		{
			this.parentPK = parentPK;
		}

		public IColumnIndexer ReadIntoDataRow()
		{
			IColumnIndexer row = null;
			if (dataObject.Type != null && (dataObject.Amount > 0m || dataObject.BaseQuantity.HasValue || dataObject.BaseValue.HasValue || dataObject.MethodOfCalculation != null || dataObject.MethodOfPayment != null || dataObject.RateReasonOverride != null || dataObject.Rate.HasValue))
			{
				row = factory.New(typeof(JobComInvoiceLineTax));

				if (row != null)
				{
					SetValue(row, JobComInvoiceLineTaxSchema.JLT_JI, parentPK);
					SetValue(row, JobComInvoiceLineTaxSchema.JLT_Amount, dataObject.Amount);
					SetValue(row, JobComInvoiceLineTaxSchema.JLT_BaseQuantity, dataObject.BaseQuantity);
					SetValue(row, JobComInvoiceLineTaxSchema.JLT_BaseValue, dataObject.BaseValue);
					SetValue(row, JobComInvoiceLineTaxSchema.JLT_MethodOfCalculation, dataObject.MethodOfCalculation);
					SetValue(row, JobComInvoiceLineTaxSchema.JLT_MethodOfPayment, dataObject.MethodOfPayment);
					SetValue(row, JobComInvoiceLineTaxSchema.JLT_RateOverrideReasonCode, dataObject.RateReasonOverride);
					SetValue(row, JobComInvoiceLineTaxSchema.JLT_Type, dataObject.Type);
					SetValue(row, JobComInvoiceLineTaxSchema.JLT_BaseQuantityUQ, dataObject.BaseQuantityUQ);
					SetValue(row, JobComInvoiceLineTaxSchema.JLT_Rate, dataObject.Rate);
				}
			}
			return row;
		}

		readonly ZGuid parentPK;
	}
}
