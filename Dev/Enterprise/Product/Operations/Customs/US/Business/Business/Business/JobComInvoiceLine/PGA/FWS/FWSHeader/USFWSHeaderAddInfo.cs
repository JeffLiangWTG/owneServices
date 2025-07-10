using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.USFWSHeader)]
	public class USFWSHeaderAddInfo : AutoUSFWSHeaderAddInfo
	{
		public USFWSHeaderAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
		public new FWSHeader Parent
		{
			get { return (FWSHeader)base.Parent; }
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

		protected override USFWSHeaderAddInfoValidation GetNewValidation()
		{
			if (Parent != null && Parent.IsExport)
			{
				return new USExportFWSHeaderAddInfoValidation(this);
			}
			else
			{
				return new USImportFWSHeaderAddInfoValidation(this);
			}
		}
	}
}
