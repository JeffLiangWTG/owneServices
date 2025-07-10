using Enterprise.Customs.Common.SG;

namespace Enterprise.Customs.SG.V4.Business
{
	public class CASCCode3Collection : Customs.Business.CusCodeDataCollection<CASCCode3>
	{
		public CASCCode3Collection(JobComInvoiceLine invoiceLine)
			: base(invoiceLine, CusCodeDataTypeList.Codes.CASCode3)
		{
			MaxCountValidationEnable(50);
		}
	}
}
