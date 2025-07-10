using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.Business
{
	public class MiningInformationCollection : CusCodeDataCollection<MiningInformation>
	{
		public MiningInformationCollection(JobComInvoiceLine master)
			: base(master, CusCodeDataTypeList.Codes.MiningInformationForSanctions)
		{
		}

		protected override bool AllowNewCore => !((JobComInvoiceLine)Master).US_DisclaimSanctions;
	}
}
