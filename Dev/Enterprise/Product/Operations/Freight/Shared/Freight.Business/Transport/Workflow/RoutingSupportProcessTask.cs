using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.Business
{
	public abstract class RoutingSupportProcessTask : ProcessTask
	{
		public RoutingSupportProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "String comparison")]
		public static ZGuid GetExceptionDeletedPKFromTriggeringEvent(IStmALog triggeringEvent)
		{
			ZGuid sourcePK = ZGuid.Empty;

			var isTriggeredByExceptionDeleted = triggeringEvent != null
				&& triggeringEvent.Parameters.TryGetValue(Params.FieldChange, out var fieldChange)
				&& fieldChange == "Exception Deleted"
				&& triggeringEvent.Parameters.TryGetValue(Params.JobNumber, out var source)
				&& ZGuid.TryParse(source, out sourcePK);

			return isTriggeredByExceptionDeleted ? sourcePK : ZGuid.Empty;
		}

		protected virtual bool IsTransportDepartureEvent(ZString eventCode)
		{
			return eventCode == Events.Departure.Code;
		}

		protected virtual bool IsTransportArrivalEvent(ZString eventCode)
		{
			return eventCode == Events.Arrival.Code;
		}

		protected virtual bool IsTransportCutOffDateEvent(ZString eventCode)
		{
			return eventCode == Events.CutOffDate.Code;
		}

		protected virtual bool IsTransportStorageCommencedEvent(ZString eventCode)
		{
			return eventCode == Events.StorageCommenced.Code;
		}

		protected virtual bool IsTransportReceiptCommencedEvent(ZString eventCode)
		{
			return eventCode == Events.ReceiptCommenced.Code;
		}

		protected virtual bool IsTransportLinkedEvent(ZString eventCode)
		{
			return IsTransportDepartureEvent(eventCode) || IsTransportArrivalEvent(eventCode) || IsTransportCutOffDateEvent(eventCode)
				|| IsTransportStorageCommencedEvent(eventCode) || IsTransportReceiptCommencedEvent(eventCode);
		}

		protected abstract Transport GetTransportLegToAttach();

		#region Validation

		protected override ProcessTasksValidation GetNewValidation()
		{
			ProcessTasksValidation result;
			if (IsMilestone)
			{
				result = new RoutingSupportMilestoneValidation(this);
			}
			else if (IsWorkflowTrigger)
			{
				result = new RoutingSupportWorkflowTriggerValidation(this);
			}
			else
			{
				result = base.GetNewValidation();
			}
			return result;
		}

		#endregion

		#region Related Business Objects

#if DEBUG
		virtual
#endif
 public new IRoutingSupport Parent
		{
			get { return (IRoutingSupport)base.Parent; }
		}

		public Transport Transport
		{
			get
			{
				Transport result = (Transport)Factory.Load(TransportType, P9_ReferencedID);

				if (result != null)
				{
					result.ParentType = TransportParentType;
				}

				return result;
			}
		}

		protected abstract Type TransportType { get; }
		protected abstract Type TransportParentType { get; }

		#endregion

		protected override bool P9_ActualDate_ReadOnly
		{
			get { return base.P9_ActualDate_ReadOnly || (Transport != null && Transport.JW_IsLinked && IsTransportLinkedEvent(P9_SE_NKMilestoneEvent)); }
		}
		#region PortPair Property

		public ZString PortPair
		{
			get { return ReferenceCode; }
			set { ReferenceCode = value; }
		}

		public CodeDescriptionPairList PortPairList
		{
			get { return ReferenceCodeList; }
		}

		public override ZString ReferenceCode
		{
			get { return Transport == null ? lastReferenceCode : (ZString)(Transport.JW_RL_NKLoadPort + "->" + Transport.JW_RL_NKDiscPort); }
			set
			{
				Match match = ReferenceCodeRegex.Match(value);
				ZString loadPort = "";
				ZString dischargePort = "";
				if (match.Success)
				{
					loadPort = match.Groups[1].Value;
					dischargePort = match.Groups[3].Value;
				}

				Transport transport = FindTransportLeg(loadPort, dischargePort);
				P9_ReferencedID = transport == null ? ZGuid.Empty : transport.PK;
				if (!P9_ReferencedID.IsValid)
				{
					lastReferenceCode = value;
				}
				ReferenceCodeInfo.RefreshBinding();
			}
		}
		ZString lastReferenceCode;

		static readonly Regex ReferenceCodeRegex = new Regex(@"^([a-z]{5})\s*(\-\>)?\s*([a-z]{5})$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		protected bool ReferenceCode_ReadOnly
		{
			get { return !CanBeLinkedToTransport; }
		}

		public override CodeDescriptionPairList ReferenceCodeList
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				foreach (Transport transport in TransportLegsInMovementOrder)
				{
					ZString portPair = transport.JW_RL_NKLoadPort + "->" + transport.JW_RL_NKDiscPort;
					result.AddPair(portPair, portPair);
				}
				return result;
			}
		}

		Transport FindTransportLeg(ZString loadPort, ZString dischargePort)
		{
			foreach (Transport transport in TransportLegsInMovementOrder)
			{
				if (transport.JW_RL_NKLoadPort == loadPort && transport.JW_RL_NKDiscPort == dischargePort)
				{
					return transport;
				}
			}
			return null;
		}

		#endregion

		#region Linking to Transport

		protected internal bool CanBeLinkedToTransport
		{
			get
			{
				return
					IsTransportLinkedEvent(P9_SE_NKMilestoneEvent) ||
					P9_TriggerField.StartsWith(JobConsolTransportSchema.Constants.Prefix + "_");
			}
		}

		public override ZGuid P9_ParentID
		{
			get { return base.P9_ParentID; }
			set
			{
				base.P9_ParentID = value;
				EnsureTransportLegAttached();
			}
		}

		public override ZString P9_Condition1
		{
			get { return base.P9_Condition1; }
			set
			{
				base.P9_Condition1 = value;
				EnsureTransportLegAttached();
			}
		}

		public override ZString P9_Condition2
		{
			get { return base.P9_Condition2; }
			set
			{
				base.P9_Condition2 = value;
				EnsureTransportLegAttached();
			}
		}

		public override ZString P9_SE_NKMilestoneEvent
		{
			get { return base.P9_SE_NKMilestoneEvent; }
			set
			{
				base.P9_SE_NKMilestoneEvent = value;
				EnsureTransportLegAttached();
				if (ReferenceCodeInfo.ReadOnly)
				{
					P9_ReferencedID = ZGuid.Empty;
				}
			}
		}

		public override ZString P9_TriggerField
		{
			get { return base.P9_TriggerField; }
			set
			{
				base.P9_TriggerField = value;
				EnsureTransportLegAttached();
				if (ReferenceCodeInfo.ReadOnly)
				{
					P9_ReferencedID = ZGuid.Empty;
				}
			}
		}

		public override ZGuid P9_ReferencedID
		{
			get { return base.P9_ReferencedID; }
			set
			{
				base.P9_ReferencedID = value;
				if (value.IsEmpty)
				{
					P9_ReferencedTableCode = ZString.Empty;
				}
			}
		}

		void EnsureTransportLegAttached()
		{
			if (Parent != null)
			{
				Transport transport = GetTransportLegToAttach();
				P9_ReferencedID = transport != null ? transport.PK : ZGuid.Empty;
				P9_ReferencedTableCode = transport != null ? JobConsolTransportSchema.Constants.Prefix : string.Empty;
				Validation.ValidateReferenceCode();
			}
		}

		#endregion

		#region TransportLegsInMovementOrder

		protected Transport[] TransportLegsInMovementOrder
		{
			get
			{
				if (transportLegsInMovementOrder == null)
				{
					transportLegsInMovementOrder = new CachedProperty<Transport[]>(Factory, delegate
					{
						Transport[] transports = (Transport[])Parent.Transports.ToArray(typeof(Transport));
						MovementLegComparer.SortMovementLegsByPorts(transports);
						return transports;
					});
				}
				return Parent?.Transports == null ? Array.Empty<Transport>() : transportLegsInMovementOrder.Value;
			}
		}
		CachedProperty<Transport[]> transportLegsInMovementOrder;

		#endregion

		public override void OnSaving()
		{
			base.OnSaving();
			LogRDDByExceptionChangedIfNecessary(false);
		}

		public override void Delete()
		{
			LogRDDByExceptionChangedIfNecessary(true);
			base.Delete();
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);

			if (saveSucceeded)
			{
				exceptionDurationHoursChangeTracker = null;
			}
		}

		#region Exceptions

		public override ZDateTimeOffset P9_ExceptionEndDate
		{
			get { return base.P9_ExceptionEndDate; }
			set
			{
				base.P9_ExceptionEndDate = value;
				UpdateExceptionDurationHours();
			}
		}

		[ReadOnlyMember(nameof(P9_ExceptionDurationHours_ReadOnly))]
		public override ZInt P9_ExceptionDurationHours
		{
			get => base.P9_ExceptionDurationHours;
			set
			{
				if (base.P9_ExceptionDurationHours != value)
				{
					SetChangingExceptionDurationHours(base.P9_ExceptionDurationHours, value);

					base.P9_ExceptionDurationHours = value;
				}
			}
		}

		public override void SetAdditionalFieldExceptionTypeCode()
		{
			if (IsException && ExceptionType != null)
			{
				if (P9_ExceptionDurationHours.IsEmpty)
				{
					P9_ExceptionDurationHours = ExceptionType.WET_DefaultDurationHours;
				}
			}
		}

		protected override void OnSetActualDateCore(ZDateTimeOffset value)
		{
			base.OnSetActualDateCore(value);
			UpdateExceptionDurationHours();
		}

		protected void LogRDDByExceptionChangedIfNecessary(bool isDeleted)
		{
			if (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.Value.IsActive &&
				IsException
				&& (isDeleted
					|| (exceptionDurationHoursChangeTracker != null
						&& exceptionDurationHoursChangeTracker.NewValue != exceptionDurationHoursChangeTracker.OriginalValue)))
			{
				LogRDDByExceptionChanged(isDeleted);
			}
		}

		protected override void SetExceptionLocation()
		{
			if (IsException && Globals.IsUserInteractive)
			{
				P9_RL_NKExceptionLocation = Env.CurrentBranch.NKUNLOCO;
			}
		}

		protected void SetChangingExceptionDurationHours(ZInt originalValue, ZInt newValue)
		{
			originalValue = exceptionDurationHoursChangeTracker?.OriginalValue ?? originalValue;
			if (!IsInDatabase || EqualityComparer<ZInt>.Default.Equals(originalValue, newValue))
			{
				exceptionDurationHoursChangeTracker = null;
			}
			exceptionDurationHoursChangeTracker = new ValueChangeTracker<ZInt>(originalValue, newValue);
		}

		protected ValueChangeTracker<ZInt> exceptionDurationHoursChangeTracker;

		public static MultilingualString ExceptionDeletedReason { get { return ResString.GetMultilingualString("510dd549-f558-43a7-ba3f-638592449e6f", "Exception Deleted"); } }
		public static MultilingualString ExceptionHoursChangedReason { get { return ResString.GetMultilingualString("eb253aff-1479-4699-b220-21312e506e9d", "Exception Duration Hours Changed"); } }
		public static MultilingualString UnknownReason { get { return ResString.GetMultilingualString("a821945d-7919-4a85-b398-6b4cfed456d4", "Unknown"); } }

		void LogRDDByExceptionChanged(bool isDeleted)
		{
			var reason = UnknownReason;

			if (IsInDatabase)
			{
				reason = isDeleted
					? ExceptionDeletedReason
					: ExceptionHoursChangedReason;
			}
			else if (!isDeleted && !ExceptionTypeCode.IsEmpty)
			{
				reason = ExceptionType?.WET_DescriptionMultilingual;
			}

			var reasonStr = (reason != null) ? ((ZString)reason) : ZString.Empty;

			if (!reasonStr.Equals(UnknownReason.ToString()))
			{
				var parameters = new Dictionary<string, string>
				{
					{ Params.FieldChange, reason },
					{ Params.JobNumber, PK.ToString() },
				};

				if (Parent is IStmALogParent logParent)
				{
					logParent.Logs.AddNew(Events.CalculateDeliveryDateWithExceptionsRequested, ZDateTimeOffset.Now, parameters.ToArray());
				}
			}
		}

		void UpdateExceptionDurationHours()
		{
			if (!P9_ActualDateOffset.IsValid || !P9_ExceptionEndDate.IsValid
				|| ExceptionType == null || !ExceptionType.WET_UseStartEndToCalculateDuration)
			{
				return;
			}

			var diff = P9_ExceptionEndDate - P9_ActualDateOffset;
			if (diff.TotalHours > 0)
			{
				P9_ExceptionDurationHours = (ZInt)Math.Ceiling(diff.TotalHours);
			}
		}

		#endregion

		#region Implementation

		protected class ValueChangeTracker<T>
		{
			public ValueChangeTracker(T originalValue, T newValue)
			{
				OriginalValue = originalValue;
				NewValue = newValue;
			}

			public T OriginalValue { get; }
			public T NewValue { get; }
		}

		#endregion
	}
}
