using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.USNHTSAHeader)]
	public class USNHTSAAddInfo : AutoUSNHTSAAddInfo, Integration.Customs.US.INHTSAHeaderAddInfo
	{
		public USNHTSAAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		public new USNHTSA Parent
		{
			get { return (USNHTSA)base.Parent; }
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
						var pgaDataCorrection = Parent as IPGADataCorrection;
						if (pgaDataCorrection != null)
						{
							pgaDataCorrection.OnStatusUpdated(oldValue, newValue);
						}
					}
				}
			}
		}

		public override ZString US_NHTProgramCode
		{
			get { return base.US_NHTProgramCode; }
			set
			{
				ZString oldValue = US_NHTProgramCode;
				base.US_NHTProgramCode = value;
				if (oldValue != US_NHTProgramCode && isInitialised)
				{
					var header = Parent as NHTSAHeader;
					if (header != null)
					{
						header.NHTSADetails.MarkAsNeedingValidation();
					}
				}
			}
		}

		public override ZString US_NHTBoxNumber
		{
			get { return base.US_NHTBoxNumber; }
			set
			{
				var oldValue = base.US_NHTBoxNumber;
				base.US_NHTBoxNumber = value;
				if (oldValue != US_NHTBoxNumber && isInitialised)
				{
					var header = Parent as NHTSAHeader;
					if (header != null)
					{
						header.NHTSADetails.MarkAsNeedingValidation();
					}
				}
			}
		}

		internal bool IsSettingAddInfoPropertyInProgress
		{
			get { return settingAddInfoProperty; }
		}
	}
}
