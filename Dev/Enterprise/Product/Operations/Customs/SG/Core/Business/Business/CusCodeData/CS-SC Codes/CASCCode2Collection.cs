using Enterprise.Customs.Common.SG;

namespace Enterprise.Customs.SG.V4.Business
{
	public class CASCCode2Collection : Customs.Business.CusCodeDataCollection<CASCCode2>
	{
		public CASCCode2Collection(JobComInvoiceLine invoiceLine)
			: base(invoiceLine, CusCodeDataTypeList.Codes.CASCode2)
		{
			MaxCountValidationEnable(50);
		}

		protected override bool AllowNewCore
		{
			get { return Count < 50; }
		}
	}
}
