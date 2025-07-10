using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.USNMFSHarvestingVessel)]
	public class USNMFSVesselsAddInfo : AutoUSNMFSVesselsAddInfo
	{
		public USNMFSVesselsAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		public override bool HasChanges
		{
			get { return base.HasChanges; }
			set
			{
				base.HasChanges = value;
				if (HasChanges)
				{
					var harvestingVessel = Parent as NMFSVessels;
					if (harvestingVessel != null && !harvestingVessel.IsMarkingAsNeedingValidationSuspended)
					{
						harvestingVessel.Parent?.MarkAsNeedingValidation();
					}
				}
			}
		}
	}
}
