using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.NZ.Business.Data.Universal
{
	public class CommercialInvoiceHeaderDataObjectReader : DataTransfer.Universal.CommercialInvoiceHeaderDataObjectReader<JobComInvoiceGroupHeader>
	{
		public CommercialInvoiceHeaderDataObjectReader(UniversalCustoms.CommercialInvoiceHeader invoiceDataObject, IXmlImportLogger logger, UniversalDataObjectReaderHelper helper, JobComInvoiceGroupHeader groupHeader, Shipment topLevelObject = null, ILandedCostDataReader landedCostDataReader = null)
			: base(invoiceDataObject, logger, helper, groupHeader, topLevelObject, landedCostDataReader)
		{
		}

		protected override ZDecimal? GetInvoiceQuantity(ZDecimal? quantity)
		{
			if (quantity.HasValue && quantity.Value > int.MaxValue)
			{
				throw new DataObjectReadFailureException(Res.GetString("20CD7763-8F32-4E15-ACD1-96B170CA4C36", "Value '{0}' of 'Invoice Quantity' is too large. It cannot be greater than {1}", quantity, int.MaxValue));
			}
			else
			{
				return base.GetInvoiceQuantity(quantity);
			}
		}
	}
}
