using Enterprise.Customs.Business;

namespace Enterprise.Customs.TW.Business
{
	public class GovernmentUniformInvoiceCollection : CusCodeDataCollection<GovernmentUniformInvoiceData>
	{
		public GovernmentUniformInvoiceCollection(JobDeclaration declaration)
		: base(declaration, CusCodeDataTypeList.Codes.GOVUniformInvoice)
		{
		}
	}
}
