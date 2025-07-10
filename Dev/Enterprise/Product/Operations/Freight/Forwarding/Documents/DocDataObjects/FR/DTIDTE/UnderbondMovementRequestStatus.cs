using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR
{
	internal class UnderbondMovementRequestStatus : DocDataObject
	{
		public UnderbondMovementRequestStatus()
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

		#region IsProvisional

		public ZBool IsProvisional
		{
			get => ObjectValue == Codes.Provisional;
			set
			{
				if (value)
				{
					ObjectValue = Codes.Provisional;
				}
				else if (IsProvisional)
				{
					ObjectValue = ZString.Empty;
				}

				ObjectValueRefreshBinding();
			}
		}

		public ZPropertyInfo IsProvisionalInfo => GetZPropertyInfo(nameof(IsProvisional));

		#endregion

		#region IsFinalized

		public ZBool IsFinalized
		{
			get => ObjectValue == Codes.Finalized;
			set
			{
				if (value)
				{
					ObjectValue = Codes.Finalized;
				}
				else if (IsFinalized)
				{
					ObjectValue = ZString.Empty;
				}

				ObjectValueRefreshBinding();
			}
		}

		public ZPropertyInfo IsFinalizedInfo => GetZPropertyInfo(nameof(IsFinalized));

		#endregion

		#region IsWaitingForAuthorization

		public ZBool IsWaitingForAuthorization
		{
			get => ObjectValue == Codes.WaitingForAuthorization;
			set
			{
				if (value)
				{
					ObjectValue = Codes.WaitingForAuthorization;
				}
				else if (IsWaitingForAuthorization)
				{
					ObjectValue = ZString.Empty;
				}

				ObjectValueRefreshBinding();
			}
		}

		public ZPropertyInfo IsWaitingForAuthorizationInfo => GetZPropertyInfo(nameof(IsWaitingForAuthorization));

		#endregion

		#region IsAgentAuthorizationPending

		public ZBool IsAgentAuthorizationPending
		{
			get => ObjectValue == Codes.AgentAuthorizationPending;
			set
			{
				if (value)
				{
					ObjectValue = Codes.AgentAuthorizationPending;
				}
				else if (IsAgentAuthorizationPending)
				{
					ObjectValue = ZString.Empty;
				}

				ObjectValueRefreshBinding();
			}
		}

		public ZPropertyInfo IsAgentAuthorizationPendingInfo => GetZPropertyInfo(nameof(IsAgentAuthorizationPending));

		#endregion

		#region IsCustomsClearancePending

		public ZBool IsCustomsClearancePending
		{
			get => ObjectValue == Codes.CustomsClearancePending;
			set
			{
				if (value)
				{
					ObjectValue = Codes.CustomsClearancePending;
				}
				else if (IsCustomsClearancePending)
				{
					ObjectValue = ZString.Empty;
				}

				ObjectValueRefreshBinding();
			}
		}

		public ZPropertyInfo IsCustomsClearancePendingInfo => GetZPropertyInfo(nameof(IsCustomsClearancePending));

		#endregion

		#region IsClosed

		public ZBool IsClosed
		{
			get => ObjectValue == Codes.Closed;
			set
			{
				if (value)
				{
					ObjectValue = Codes.Closed;
				}
				else if (IsClosed)
				{
					ObjectValue = ZString.Empty;
				}

				ObjectValueRefreshBinding();
			}
		}

		public ZPropertyInfo IsClosedInfo => GetZPropertyInfo(nameof(IsClosed));

		#endregion

		#region IsInvalidated

		public ZBool IsInvalidated
		{
			get => ObjectValue == Codes.Invalidated;
			set
			{
				if (value)
				{
					ObjectValue = Codes.Invalidated;
				}
				else if (IsInvalidated)
				{
					ObjectValue = ZString.Empty;
				}

				ObjectValueRefreshBinding();
			}
		}

		public ZPropertyInfo IsInvalidatedInfo => GetZPropertyInfo(nameof(IsInvalidated));

		#endregion

		#region IsUnavailableTransfer

		public ZBool IsUnavailableTransfer
		{
			get => ObjectValue == Codes.UnavailableTransfer;
			set
			{
				if (value)
				{
					ObjectValue = Codes.UnavailableTransfer;
				}
				else if (IsUnavailableTransfer)
				{
					ObjectValue = ZString.Empty;
				}

				ObjectValueRefreshBinding();
			}
		}

		public ZPropertyInfo IsUnavailableTransferInfo => GetZPropertyInfo(nameof(IsUnavailableTransfer));

		#endregion

		#region IsWaitingForLogisticsData

		public ZBool IsWaitingForLogisticsData
		{
			get => ObjectValue == Codes.WaitingForLogisticsData;
			set
			{
				if (value)
				{
					ObjectValue = Codes.WaitingForLogisticsData;
				}
				else if (IsWaitingForLogisticsData)
				{
					ObjectValue = ZString.Empty;
				}

				ObjectValueRefreshBinding();
			}
		}

		public ZPropertyInfo IsWaitingForLogisticsDataInfo => GetZPropertyInfo(nameof(IsWaitingForLogisticsData));

		#endregion

		void ObjectValueRefreshBinding()
		{
			ObjectValueInfo.RefreshBinding();

			IsProvisionalInfo.RefreshBinding();
			IsFinalizedInfo.RefreshBinding();
			IsWaitingForAuthorizationInfo.RefreshBinding();
			IsAgentAuthorizationPendingInfo.RefreshBinding();
			IsCustomsClearancePendingInfo.RefreshBinding();
			IsClosedInfo.RefreshBinding();
			IsInvalidatedInfo.RefreshBinding();
			IsUnavailableTransferInfo.RefreshBinding();
			IsWaitingForLogisticsDataInfo.RefreshBinding();
		}

		public static class Codes
		{
			public const string Provisional = "PRO";
			public const string WaitingForAuthorization = "AAD";
			public const string Finalized = "VAL";
			public const string AgentAuthorizationPending = "AAG";
			public const string CustomsClearancePending = "ADO";
			public const string Closed = "CLO";
			public const string Invalidated = "INV";
			public const string UnavailableTransfer = "ITR";
			public const string WaitingForLogisticsData = "ADL";
		}
	}
}
