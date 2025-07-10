using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ZA.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.DataTransfer.Universal
{
	public class CommercialInvoiceHeaderDataObjectReaderForWOT : CommercialInvoiceHeaderDataObjectReader
	{
		public CommercialInvoiceHeaderDataObjectReaderForWOT(CommercialInvoiceHeader invoiceDataObject, IXmlImportLogger logger, UniversalDataObjectReaderHelper helper, JobComInvoiceGroupHeader groupHeader, Shipment topLevelObject = null, ILandedCostDataReader landedCostDataReader = null)
			: base(invoiceDataObject, logger, helper, groupHeader, topLevelObject, landedCostDataReader)
		{
		}

		protected override void FillBondedWarehouseProperties(CommercialInvoiceLine invoiceLineData, BaseJobComInvoiceLine invoiceLine, Dictionary<string, ValueSetter> delaySetters)
		{
			base.FillBondedWarehouseProperties(invoiceLineData, invoiceLine, delaySetters);

			if (ZGuid.TryParse(invoiceLineData.DataImportMatchingKey, out var guid))
			{
				var invoiceLineRow = GetColumnIndexer(invoiceLine);
				SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_ParentTableCode, "WOL", delaySetters);
				SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_ParentID, guid, delaySetters);
			}
		}

		protected override void FillMatchingKey(CommercialInvoiceLine invoiceLineData, IColumnIndexer invoiceLineRow)
		{
		}

		protected override IEnumerable<ZString> GetDuplicateKeys() => Enumerable.Empty<ZString>();
	}
}
