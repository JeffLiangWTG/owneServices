using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	public class NMFSVesselsCollection : DependentCusAddInfoCollection<NMFSVessels, NMFSHarvestingDetail>
	{
		public NMFSVesselsCollection(NMFSHarvestingDetail master)
			: base(master, CusAddInfoTypeAttribute.Codes.USNMFSHarvestingVessel)
		{
		}

		protected override bool AllowNewCore
		{
			get
			{
				var master = Master;
				return master.IsSIMPProgramType && master.US_SourceType == SourceTypeCodesList.Codes.HarvestOfCaptureFisheries;
			}
		}
	}
}
