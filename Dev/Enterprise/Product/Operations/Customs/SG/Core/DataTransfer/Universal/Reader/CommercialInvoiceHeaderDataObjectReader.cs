using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.SG.V4.DataTransfer.Universal
{
	public class CommercialInvoiceHeaderDataObjectReader : Customs.DataTransfer.Universal.CommercialInvoiceHeaderDataObjectReader<BaseJobComInvoiceGroupHeader>
	{
		internal CommercialInvoiceHeaderDataObjectReader(CommercialInvoiceHeader invoiceDataObject, IXmlImportLogger logger, Customs.DataTransfer.Universal.UniversalDataObjectReaderHelper helper, BaseJobComInvoiceGroupHeader groupHeader, Shipment topLevelObject = null, ILandedCostDataReader landedCostDataReader = null)
			: base(invoiceDataObject, logger, helper, groupHeader, topLevelObject, landedCostDataReader, invoiceType: typeof(JobComInvoiceHeader))
		{
		}

		protected override void FillCountrySpecificAddInfoDetails(BaseJobComInvoiceLine invoiceLine)
		{
			if (invoiceLine is JobComInvoiceLine sgInvoiceLine)
			{
				var addInfoCollection = CurrentInvoiceLineData?.AddInfoCollection;
				if (addInfoCollection?.Any() ?? false)
				{
					SetNoteValue(sgInvoiceLine, addInfoCollection, PredefinedNoteTypes.Instance.SGMarksAndNumbers.Description);
					SetNoteValue(sgInvoiceLine, addInfoCollection, PredefinedNoteTypes.Instance.SGCertificateItemDesc.Description);
				}
			}
		}

		void SetNoteValue(JobComInvoiceLine invoiceLine, List<UniversalDataBuss.DataObjects.Universal.AddInfo> addInfoCollection, string noteDescription)
		{
			//At the moment all Invoice Lines are removed even when an existing declaration is matched.
			//When the WI to make Invoice Lines Partial is done you will need to load the note and either create, update or delete it depending on the AddInfo value
			var addInfoValue = addInfoCollection.GetZStringValue(noteDescription, logger);
			if (addInfoValue.HasValue)
			{
				if (noteDescription == PredefinedNoteTypes.Instance.SGMarksAndNumbers.Description)
				{
					invoiceLine.MarksAndNumbers = addInfoValue.Value;
				}
				else if (noteDescription == PredefinedNoteTypes.Instance.SGCertificateItemDesc.Description)
				{
					invoiceLine.CertItemDescription = addInfoValue.Value;
				}
			}
		}
	}
}
