using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.USAMS)]
	public class AMSAddInfo : AutoUSAMSAddInfo
	{
		public AMSAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		internal bool IsSettingAddInfoPropertyInProgress
		{
			get { return settingAddInfoProperty; }
		}

		public new AMS Parent
		{
			get { return (AMS)base.Parent; }
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

		public OrgAddress CertifyingBodyAddress
		{
			get
			{
				return Factory.GetCachedValue(US_OA_CertifyingBody.ToString(), () =>
				{
					return Factory.Load<OrgAddress>(US_OA_CertifyingBody);
				});
			}
		}

		public OrgAddress RecipientAddress
		{
			get
			{
				return Factory.GetCachedValue(US_OA_Recipient.ToString(), () =>
				{
					return Factory.Load<OrgAddress>(US_OA_Recipient);
				});
			}
		}
	}
}
