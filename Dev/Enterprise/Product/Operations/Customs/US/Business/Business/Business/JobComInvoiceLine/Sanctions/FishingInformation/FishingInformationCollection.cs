using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	public class FishingInformationCollection : DependentCusAddInfoCollection<FishingInformation, JobComInvoiceLine>
	{
		public FishingInformationCollection(JobComInvoiceLine master)
			: base(master, CusAddInfoTypeAttribute.Codes.USNMFSHarvestingDetail)
		{
		}

		protected override bool AllowNewCore => !Master.US_DisclaimSanctions;
	}
}
