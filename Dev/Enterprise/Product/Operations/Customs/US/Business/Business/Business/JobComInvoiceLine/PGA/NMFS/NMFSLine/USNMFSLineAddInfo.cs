using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.USNMFSLine)]
	public class USNMFSLineAddInfo : AutoUSNMFSLineAddInfo
	{
		public USNMFSLineAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		public new NMFSLine Parent
		{
			get { return (NMFSLine)base.Parent; }
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

		internal bool IsSettingAddInfoPropertyInProgress
		{
			get { return settingAddInfoProperty; }
		}

		protected override USNMFSLineAddInfoValidation GetNewValidation()
		{
			if (Parent != null && Parent.IsExport)
			{
				return new USExportNMFSAddInfoValidation(this);
			}
			else
			{
				return new USImportNMFSLineAddInfoValidation(this);
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
					Parent.MarkAsNeedingValidationIncludingChildren();
				}
			}
		}
	}
}
