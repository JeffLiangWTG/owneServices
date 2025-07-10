using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	public class NMFSHarvestingDetailCollection : DependentCusAddInfoCollection<NMFSHarvestingDetail, NMFSLine>
	{
		public NMFSHarvestingDetailCollection(NMFSLine master)
			: base(master, CusAddInfoTypeAttribute.Codes.USNMFSHarvestingDetail)
		{
		}

		protected override bool AllowNewCore
		{
			get
			{
				var master = Master;
				return !master.IsAMRProgramType;
			}
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			var lineCollection = Master.InvoiceLine?.NMFSLines ?? Master.Pivot?.NMFSLines;
			var isDefaultingSuspended = lineCollection != null && lineCollection.IsNMFSLineDefaultingSuspended;
			if (!isDefaultingSuspended)
			{
				var harvestingDetail = child as NMFSHarvestingDetail;
				if (Master.IsSIMProgramType && Master.US_SourceType == SourceTypeCodesList.Codes.HatcheryBasedAquaculture)
				{
					harvestingDetail.US_ContactPartyType = EntityRoleCodeList.Codes.AquacultureFacility;
				}
			}
		}
	}
}
