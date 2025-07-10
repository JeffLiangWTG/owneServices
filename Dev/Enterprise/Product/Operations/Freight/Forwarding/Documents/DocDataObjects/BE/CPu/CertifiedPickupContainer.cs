using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.BE
{
	sealed class CertifiedPickupContainer : DocDataObject
	{
		#region Ctor

		public CertifiedPickupContainer(object identifier, string formMode)
			: base(identifier)
		{
			FormMode = formMode;
		}

		#endregion

		#region Number

		public ZString Number
		{
			get => number;
			set
			{
				if (SetNonPersistentPropertyValue(NumberInfo, ref number, value))
				{
					Validate(NumberInfo);
				}
			}
		}

		ZString number;

		public ZPropertyInfo NumberInfo => GetZPropertyInfo(nameof(Number));

		#endregion

		#region ReleaseIdentification

		public ZString ReleaseIdentification
		{
			get => releaseIdentification;
			set
			{
				if (SetNonPersistentPropertyValue(ReleaseIdentificationInfo, ref releaseIdentification, value))
				{
					Validate(ReleaseIdentification);
				}
			}
		}

		ZString releaseIdentification;

		public ZPropertyInfo ReleaseIdentificationInfo => GetZPropertyInfo(nameof(ReleaseIdentification));

		#endregion

		#region CurrentStatus

		public ZString CurrentStatus
		{
			get => currentStatus;
			set
			{
				if (SetNonPersistentPropertyValue(CurrentStatusInfo, ref currentStatus, value))
				{
					Validate(CurrentStatusInfo);
				}
			}
		}

		ZString currentStatus;

		public ZPropertyInfo CurrentStatusInfo => GetZPropertyInfo(nameof(CurrentStatus));

		#endregion

		#region Action

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Action suffix")]
		public CertifiedPickupAction Action
		{
			get
			{
				if (action == null)
				{
					action = new CertifiedPickupAction(Identifier?.ToString() + "Action");
					action.ObjectValueInfo.ValueChanged += (sender, args) => Validate(ErrorPlaceHolderInfo);
				}

				return action;
			}
		}

		CertifiedPickupAction action;

		#endregion

		#region Reason

		public ZString Reason
		{
			get => reason;
			set
			{
				if (SetNonPersistentPropertyValue(ReasonInfo, ref reason, value))
				{
					Validate(Reason);
				}
			}
		}

		ZString reason;

		public ZPropertyInfo ReasonInfo => GetZPropertyInfo(nameof(Reason));

		#endregion

		#region Release From

		public ZString ReleaseFromName
		{
			get => releaseFromName;
			set
			{
				if (SetNonPersistentPropertyValue(ReleaseFromNameInfo, ref releaseFromName, value))
				{
					Validate(ReleaseFromName);
				}
			}
		}

		ZString releaseFromName;

		public ZPropertyInfo ReleaseFromNameInfo => GetZPropertyInfo(nameof(ReleaseFromName));

		public ZString ReleaseFromId
		{
			get => releaseFromId;
			set
			{
				if (SetNonPersistentPropertyValue(ReleaseFromIdInfo, ref releaseFromId, value))
				{
					Validate(ReleaseFromId);
				}
			}
		}

		ZString releaseFromId;

		public ZPropertyInfo ReleaseFromIdInfo => GetZPropertyInfo(nameof(ReleaseFromId));

		public ZString ReleaseFromCode
		{
			get => releaseFromCode;
			set
			{
				if (SetNonPersistentPropertyValue(ReleaseFromCodeInfo, ref releaseFromCode, value))
				{
					Validate(ReleaseFromCode);
				}
			}
		}

		ZString releaseFromCode;

		public ZPropertyInfo ReleaseFromCodeInfo => GetZPropertyInfo(nameof(ReleaseFromCode));

		#endregion

		#region Human Readable Status

		public ZString HumanReadableStatus
		{
			get => humanReadableStatus;
			set
			{
				if (SetNonPersistentPropertyValue(HumanReadableStatusInfo, ref humanReadableStatus, value))
				{
					Validate(HumanReadableStatus);
				}
			}
		}

		ZString humanReadableStatus;

		public ZPropertyInfo HumanReadableStatusInfo => GetZPropertyInfo(nameof(HumanReadableStatus));

		#endregion

		#region Form Mode

		public string FormMode { get; private set; }

		#endregion

		#region ErrorPlaceHolder

		public ZString ErrorPlaceHolder
		{
			get => errorPlaceHolder;
			set
			{
				if (SetNonPersistentPropertyValue(ErrorPlaceHolderInfo, ref errorPlaceHolder, value))
				{
					Validate(ErrorPlaceHolderInfo);
				}
			}
		}

		ZString errorPlaceHolder;

		public ZPropertyInfo ErrorPlaceHolderInfo => GetZPropertyInfo(nameof(ErrorPlaceHolder));

		#endregion ErrorPlaceHolder

		#region IsNonOperativeReefer

		public ZBool IsNonOperativeReefer
		{
			get => isNonOperativeReefer;
			set
			{
				if (SetNonPersistentPropertyValue(IsNonOperativeReeferInfo, ref isNonOperativeReefer, value))
				{
					Validate(IsNonOperativeReeferInfo);
				}
			}
		}

		ZBool isNonOperativeReefer;

		public ZPropertyInfo IsNonOperativeReeferInfo => GetZPropertyInfo(nameof(IsNonOperativeReefer));

		#endregion
	}
}
