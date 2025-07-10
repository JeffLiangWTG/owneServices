using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.USNMFSHarvestingDetail)]
	public class USNMFSHarvestingDetailAddInfo : AutoUSNMFSHarvestingDetailAddInfo
	{
		public USNMFSHarvestingDetailAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		protected override USNMFSHarvestingDetailAddInfoValidation GetNewValidation()
		{
			if (base.Parent is FishingInformation)
			{
				return new FishingInformationAddInfoValidation(this);
			}
			else
			{
				return new NMFSHarvestingDetailAddInfoValidation(this);
			}
		}

		public override bool HasChanges
		{
			get { return base.HasChanges; }
			set
			{
				base.HasChanges = value;
				if (HasChanges && Parent != null && !Parent.IsMarkingAsNeedingValidationSuspended)
				{
					Parent.MarkAsNeedingValidation();
				}
			}
		}
	}
}
