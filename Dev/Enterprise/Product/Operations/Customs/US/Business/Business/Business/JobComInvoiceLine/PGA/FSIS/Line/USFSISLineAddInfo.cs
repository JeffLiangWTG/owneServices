using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.USFSISCertificate)]
	public class USFSISLineAddInfo : AutoUSFSISLineAddInfo
	{
		public USFSISLineAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		public new USFSISLine Parent
		{
			get { return (USFSISLine)base.Parent; }
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

		protected override USFSISLineAddInfoValidation GetNewValidation()
		{
			var validation = new USFSISLineAddInfoValidation(this);

			if (Parent.B7_ParentTableCode == "JI")
			{
				validation = new USInvoiceLineFSISLineAddInfoValidation(this);
			}
			else if (Parent.B7_ParentTableCode == "JE")
			{
				validation = new USDeclarationFSISLineAddInfoValidation(this);
			}

			return validation;
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
	}
}
