using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.USTTBLine)]
	public class USTTBLineAddInfo : AutoUSTTBLineAddInfo
	{
		public USTTBLineAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		public new TTBLine Parent
		{
			get { return (TTBLine)base.Parent; }
			protected set { base.Parent = value; }
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

		protected override ZString HumanReadableNameCore
		{
			get { return "TTB Data"; }
		}

		internal bool IsSettingAddInfoPropertyInProgress
		{
			get { return settingAddInfoProperty; }
		}

		protected override USTTBLineAddInfoValidation GetNewValidation()
		{
			if (Parent != null && Parent.IsExport)
			{
				return new USExportTTBLineAddInfoValidation(this);
			}
			else
			{
				return new USImportTTBLineAddInfoValidation(this);
			}
		}
	}
}
