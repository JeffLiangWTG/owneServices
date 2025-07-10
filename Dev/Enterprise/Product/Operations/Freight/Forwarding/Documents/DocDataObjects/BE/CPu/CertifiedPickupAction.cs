using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.BE
{
	internal class CertifiedPickupAction : DocDataObject
	{
		public CertifiedPickupAction(object identifier = default)
			: base(identifier)
		{
		}

		#region ObjectValue

		public ZString ObjectValue
		{
			get => objectValue;
			internal set
			{
				if (SetNonPersistentPropertyValue(ObjectValueInfo, ref objectValue, value))
				{
					Validate(ObjectValueInfo);
				}
			}
		}
		ZString objectValue;
		public ZPropertyInfo ObjectValueInfo => GetZPropertyInfo(nameof(ObjectValue));

		#endregion

		#region IsEmpty

		public ZBool IsEmpty
		{
			get => ObjectValue.IsEmpty;
		}

		#endregion

		#region IsAccept

		public ZBool IsAccept
		{
			get => ObjectValue == Codes.Accept;
			set
			{
				if (value)
				{
					ObjectValue = Codes.Accept;
				}
				else if (IsAccept)
				{
					ObjectValue = ZString.Empty;
				}

				ObjectValueRefreshBinding();
			}
		}

		public ZPropertyInfo IsAcceptInfo => GetZPropertyInfo(nameof(IsAccept));

		#endregion

		#region IsDecline

		public ZBool IsDecline
		{
			get => ObjectValue == Codes.Decline;
			set
			{
				if (value)
				{
					ObjectValue = Codes.Decline;
				}
				else if (IsDecline)
				{
					ObjectValue = ZString.Empty;
				}

				ObjectValueRefreshBinding();
			}
		}

		public ZPropertyInfo IsDeclineInfo => GetZPropertyInfo(nameof(IsDecline));

		#endregion

		#region IsTransferToForwarder

		public ZBool IsTransferToForwarder
		{
			get => ObjectValue == Codes.TransferToForwarder;
			set
			{
				if (value)
				{
					ObjectValue = Codes.TransferToForwarder;
				}
				else if (IsTransferToForwarder)
				{
					ObjectValue = ZString.Empty;
				}

				ObjectValueRefreshBinding();
			}
		}

		public ZPropertyInfo IsTransferToForwarderInfo => GetZPropertyInfo(nameof(IsTransferToForwarder));

		#endregion

		#region IsTransferToTransporter

		public ZBool IsTransferToTransporter
		{
			get => ObjectValue == Codes.TransferToTransporter;
			set
			{
				if (value)
				{
					ObjectValue = Codes.TransferToTransporter;
				}
				else if (IsTransferToTransporter)
				{
					ObjectValue = ZString.Empty;
				}

				ObjectValueRefreshBinding();
			}
		}

		public ZPropertyInfo IsTransferToTransporterInfo => GetZPropertyInfo(nameof(IsTransferToTransporter));

		#endregion

		#region IsRevokeMode

		public ZBool IsRevokeMode { get; set; } = false;

		#endregion

		void ObjectValueRefreshBinding()
		{
			ObjectValueInfo.RefreshBinding();

			IsAcceptInfo.RefreshBinding();
			IsDeclineInfo.RefreshBinding();
			IsTransferToForwarderInfo.RefreshBinding();
			IsTransferToTransporterInfo.RefreshBinding();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		public static class Codes
		{
			public const string Accept = "Accept";
			public const string Decline = "Decline";
			public const string TransferToForwarder = "Forwarder";
			public const string TransferToTransporter = "Transporter";
		}
	}
}
