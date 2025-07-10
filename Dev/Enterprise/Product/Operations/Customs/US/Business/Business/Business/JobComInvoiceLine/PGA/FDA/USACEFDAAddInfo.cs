using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.USACEFDA)]
	public class USACEFDAAddInfo : AutoUSACEFDAAddInfo, Integration.Customs.US.IUSACEFDAAddInfo
	{
		public USACEFDAAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		public new ACEFDA Parent
		{
			get { return (ACEFDA)base.Parent; }
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

		internal bool IsSettingAddInfoPropertyInProgress
		{
			get { return settingAddInfoProperty; }
		}

		protected override USACEFDAAddInfoValidation GetNewValidation()
		{
			var parent = Parent;
			if (parent != null)
			{
				if (parent.InvoiceLine != null)
				{
					return new USACEFDAAddInfoInvoiceLineValidation(this);
				}
				else if ((parent.Parent as CusClassPartPivot) != null)
				{
					return new USACEFDAAddInfoProductValidation(this);
				}
			}
			return base.GetNewValidation();
		}
	}
}
