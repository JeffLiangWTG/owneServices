using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class SecureContainerReleaseContainer : DocDataObject
	{
		#region SecureContainerReleaseContainer

		public SecureContainerReleaseContainer(object identifier, string formMode)
			: base(identifier)
		{
			FormMode = formMode;
		}

		#endregion

		#region Form Mode

		public string FormMode { get; private set; }

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

		#region IsTranferToForwarder

		public ZBool IsTranferToForwarder
		{
			get => isTranferToForwarder;
			set
			{
				if (SetNonPersistentPropertyValue(IsTranferToForwarderInfo, ref isTranferToForwarder, value))
				{
					Validate(IsTranferToForwarderInfo);
				}
			}
		}

		ZBool isTranferToForwarder;

		public ZPropertyInfo IsTranferToForwarderInfo => GetZPropertyInfo(nameof(IsTranferToForwarder));

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
	}
}
