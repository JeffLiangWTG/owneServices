using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.USDEAHeader)]
	public class DEAHeaderAddInfo : AutoUSDEAHeaderAddInfo
	{
		public DEAHeaderAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		internal bool IsSettingAddInfoPropertyInProgress
		{
			get { return settingAddInfoProperty; }
		}

		public new DEAHeader Parent
		{
			get { return (DEAHeader)base.Parent; }
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

		protected override USDEAHeaderAddInfoValidation GetNewValidation()
		{
			if (Parent != null && Parent.IsExport)
			{
				return new USExportDEAHeaderAddInfoValidation(this);
			}
			else
			{
				return new USImportDEAHeaderAddInfoValidation(this);
			}
		}
	}
}
