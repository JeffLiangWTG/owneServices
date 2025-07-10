using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using AddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

namespace Enterprise.Customs.SG.V4.DataTransfer.Universal
{
	public class CommercialInvoiceHeaderDataObjectWriter : Customs.DataTransfer.Universal.CommercialInvoiceHeaderDataObjectWriter
	{
		internal CommercialInvoiceHeaderDataObjectWriter(IDataWritingManager manager, Customs.DataTransfer.Universal.UniversalDataObjectWriterHelper helper, ILandedCostDataWriter landedCostDataWriter = null, Customs.Business.CusEntryHeader relatedEntry = null)
			: base(manager, helper, landedCostDataWriter, relatedEntry)
		{
		}

		protected override List<AddInfo> GetInvoiceLineAddInfoCollection(Customs.Business.BaseJobComInvoiceLine invoiceLineBO)
		{
			var result = base.GetInvoiceLineAddInfoCollection(invoiceLineBO);
			var invoiceLine = (JobComInvoiceLine)invoiceLineBO;
			PopulateAddInfoCollection(result, PredefinedNoteTypes.Instance.SGCertificateItemDesc.Description, invoiceLine.CertItemDescription);
			PopulateAddInfoCollection(result, PredefinedNoteTypes.Instance.SGMarksAndNumbers.Description, invoiceLine.MarksAndNumbers);

			return result;
		}

		#region Implementation

		void PopulateAddInfoCollection(List<AddInfo> addInfoList, ZString key, IZType value)
		{
			if (!value.IsEmpty)
			{
				helper.Update(addInfoList, key, value);
			}
		}

		#endregion
	}
}
