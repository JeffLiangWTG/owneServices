using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.USPesticide)]
	public class USPSTAddInfo : AutoUSPSTAddInfo, Integration.Customs.US.IPesticideAddInfo
	{
		public USPSTAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		internal bool IsSettingAddInfoPropertyInProgress
		{
			get { return settingAddInfoProperty; }
		}

		public new Pesticide Parent
		{
			get { return (Pesticide)base.Parent; }
		}

		public override ZString US_TrackingStatus
		{
			get { return base.US_TrackingStatus; }
			set
			{
				var oldValue = US_TrackingStatus;
				base.US_TrackingStatus = value;
				if (isInitialised && !IsCopying)
				{
					var newValue = US_TrackingStatus;
					if (oldValue != newValue)
					{
						Parent.OnStatusUpdated(oldValue, newValue);
					}
				}
			}
		}
	}
}
