using Enterprise.Customs.Common.SG;

namespace Enterprise.Customs.SG.V4.Business
{
	public class CASCCode1Collection : Customs.Business.CusCodeDataCollection<CASCCode1>
	{
		public CASCCode1Collection(JobComInvoiceLine invoiceLine)
			: base(invoiceLine, CusCodeDataTypeList.Codes.CASCode1)
		{
			MaxCountValidationEnable(50);
		}
	}
}
