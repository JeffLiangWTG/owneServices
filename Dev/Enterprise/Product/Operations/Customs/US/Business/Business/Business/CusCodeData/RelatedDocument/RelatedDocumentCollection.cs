using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class RelatedDocumentCollection : Customs.Business.CusCodeDataCollection<RelatedDocument>
	{
		public RelatedDocumentCollection(JobComInvoiceHeader invoice)
			: base(invoice, CusCodeDataTypeList.Codes.RelatedDocument)
		{
		}

		internal void AddOrUpdate(ZString code, ZString number)
		{
			if (!number.IsEmpty)
			{
				RelatedDocument document = GetFirstElementHaving(code);
				if (document == null)
				{
					document = AddNew();
					document.CY_Code = code;
				}
				document.CY_Data = number;
			}
		}
	}
}
