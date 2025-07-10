using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.ResourceStrings.Grammar;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

using NotificationType = CargoWise.EntityFramework.NotificationType;

namespace Enterprise.Freight.Forwarding.GUI
{
	public class UpdateLastKnownTransitWarehouseApplicator : OperationalActionMethodApplicator
	{
		public UpdateLastKnownTransitWarehouseApplicator(BusinessObjectFactory factory)
			: base("UpdateLastKnownTransitWarehouseApplicator", factory)
		{
		}

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			if (targets.Length == 0)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Error, Res.GetString("FC3B680D-D62D-4573-B25D-63C8CF28625C", "No shipments selected."));
			}
			else
			{
				foreach (ForwardingShipment shipment in targets)
				{
					ProcessShipment(log, shipment, out var didNotificationErrorOccur);

					if (didNotificationErrorOccur)
					{
						break;
					}
				}
			}
		}

		void ProcessShipment(IOperationalActionSectionLog log, ForwardingShipment shipment, out bool didErrorNotificationOccur)
		{
			AddLog(log, Res.GetString("B4BA3CAE-4F34-45A1-8830-78F3901DA49E", "Processing shipment {0}:", shipment.JS_UniqueConsignRef));

			didErrorNotificationOccur = false;
			foreach (ForwardingPackLine packLine in shipment.OuterPackLines)
			{
				ProcessPackLine(log, packLine, out didErrorNotificationOccur);

				if (didErrorNotificationOccur)
				{
					var message = Res.GetString("9D896E89-474B-40A4-A46A-2FF941EFBE38", "A save-preventing error occurred while processing the shipments, so the operational action must be aborted.", shipment.JS_UniqueConsignRef);
					AddLog(log, message, errorLevel: OperationalActionLogErrorLevel.Error);
					return;
				}
			}

			AddLog(log, Res.GetString("0B0EED34-CDC1-49BF-842B-C32E1B53ABA2", "Shipment processed.", shipment.JS_UniqueConsignRef));
		}

		void ProcessPackLine(IOperationalActionSectionLog log, ForwardingPackLine packLine, out bool didErrorNotificationOccur)
		{
			AddLog(log, Res.GetString("EDFD6395-A438-4EF6-BA30-B52AFDB860A0", "Processing pack line:"), indentLevel: 1);

			UpdateLastKnownTransitWarehouseAddress(packLine, log, out didErrorNotificationOccur);

			if (!didErrorNotificationOccur)
			{
				UpdateLastKnownTransitWarehouseStatus(packLine, log);
				UpdateLastKnownTransitWarehouseStatusDateTime(packLine, log);

				AddLog(log, Res.GetString("D72C55AA-77B8-4B3D-A3A5-8E08F88AF19B", "Pack line processed."), indentLevel: 1);
			}
		}

		void UpdateLastKnownTransitWarehouseAddress(ForwardingPackLine packLine, IOperationalActionSectionLog log, out bool didNotificationErrorOccur)
		{
			didNotificationErrorOccur = false;

			if (packLine.JL_OA_LastKnownTransitWarehouseAddress != TransitWarehouseAddressPK)
			{
				packLine.JL_OA_LastKnownTransitWarehouseAddress = TransitWarehouseAddressPK;
				AddLog(log, Res.GetString("BEAFB735-D705-4DF7-91BC-EFFC940F7931", "Last known transit warehouse address updated."), indentLevel: 2);

				packLine.Validation.ValidateJL_OA_LastKnownTransitWarehouseAddress();

				foreach (var notification in packLine.JL_OA_LastKnownTransitWarehouseAddressInfo.Notifications)
				{
					var message = Res.GetString("67C93B5D-014C-455E-A0C9-F41CD83C12F4", "{0}: {1}", notification.Type.NotificationTypeName(PluralState.NonPlural), notification.Message);
					var errorLevel = GetErrorLevelFromNotification(notification);
					AddLog(log, message, errorLevel: errorLevel, indentLevel: 3);

					didNotificationErrorOccur |= errorLevel == OperationalActionLogErrorLevel.Error;
				}
			}
			else
			{
				AddLog(log, Res.GetString("42C60144-6845-41A6-A3F2-D22685E182F7", "Skipped updating last known transit warehouse address as it is the same."), indentLevel: 2);
			}
		}

		void UpdateLastKnownTransitWarehouseStatus(ForwardingPackLine packLine, IOperationalActionSectionLog log)
		{
			if (packLine.JL_LastKnownTransitWarehouseStatus != TransitWarehouseStatus)
			{
				packLine.JL_LastKnownTransitWarehouseStatus = TransitWarehouseStatus;
				AddLog(log, Res.GetString("B5487DD9-7A9B-40C0-90AC-6C2485D024FF", "Last known transit warehouse status updated."), indentLevel: 2);
			}
			else
			{
				AddLog(log, Res.GetString("2815294D-CA6B-414B-BA81-4B756C3A82E1", "Skipped updating last known transit warehouse status as it is the same."), indentLevel: 2);
			}
		}

		void UpdateLastKnownTransitWarehouseStatusDateTime(ForwardingPackLine packLine, IOperationalActionSectionLog log)
		{
			if (packLine.JL_LastKnownTransitWarehouseStatusDateTime != TransitWarehouseStatusDateTime)
			{
				packLine.JL_LastKnownTransitWarehouseStatusDateTime = TransitWarehouseStatusDateTime;
				AddLog(log, Res.GetString("4B0A8915-4CE1-4907-9E1A-81B779063921", "Last known transit warehouse status date updated."), indentLevel: 2);
			}
			else
			{
				AddLog(log, Res.GetString("F0C7DEAF-7F22-41FB-A4A3-388EA144B227", "Skipped updating last known transit warehouse status date as it is the same."), indentLevel: 2);
			}
		}

		void AddLog(IOperationalActionSectionLog log, string message, OperationalActionLogErrorLevel errorLevel = OperationalActionLogErrorLevel.Informational, int indentLevel = 0)
		{
			log.NotifyFormat(errorLevel, new string('\t', indentLevel) + message);
		}

		OperationalActionLogErrorLevel GetErrorLevelFromNotification(INotification notification)
		{
			var severity = notification.Type.Severity;

			if (severity == NotificationType.Error.Severity)
			{
				return OperationalActionLogErrorLevel.Error;
			}
			else if (severity == NotificationType.Warning.Severity || severity == NotificationType.MessageError.Severity)
			{
				return OperationalActionLogErrorLevel.Warning;
			}

			return OperationalActionLogErrorLevel.Informational;
		}

		#region TransitWarehouseAddress

		[RelatedBusinessObject("TransitWarehouseAddress")]
		[List("BindToLists.OrgAddress_List")]
		public ZGuid TransitWarehouseAddressPK
		{
			get => transitWarehouseAddressPK;
			set
			{
				SetNonPersistentPropertyValue(TransitWarehouseAddressInfo, ref transitWarehouseAddressPK, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateTransitWarehouseAddress();
				}
			}
		}
		ZGuid transitWarehouseAddressPK;

		public OrgAddress TransitWarehouseAddress => Factory.Load<OrgAddress>(TransitWarehouseAddressPK);

		#region ZAddress

		public ZAddress TransitWarehouseAddressPK_ZAddress
		{
			get
			{
				if (transitWarehouseAddressPK_ZAddress == null)
				{
					transitWarehouseAddressPK_ZAddress = new ZAddress(TransitWarehouseAddressInfo);
				}
				return transitWarehouseAddressPK_ZAddress;
			}
		}
		ZAddress transitWarehouseAddressPK_ZAddress;

		#endregion

		public ZPropertyInfo TransitWarehouseAddressInfo => GetZPropertyInfo(nameof(TransitWarehouseAddressPK));

		#endregion

		#region TransitWarehouseStatus

		[MaxLength(3)]
		[List("TransitWarehouseStatusList")]
		public ZString TransitWarehouseStatus
		{
			get => transitWarehouseStatus;
			set
			{
				CheckMaximumLength(TransitWarehouseStatusInfo, value);
				SetNonPersistentPropertyValue(TransitWarehouseStatusInfo, ref transitWarehouseStatus, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateTransitWarehouseStatus();
				}
			}
		}
		ZString transitWarehouseStatus;

		public ZPropertyInfo TransitWarehouseStatusInfo => GetZPropertyInfo(nameof(TransitWarehouseStatus));

		#endregion

		#region TransitWarehouseStatusDateTime

		public ZDateTime TransitWarehouseStatusDateTime
		{
			get => transitWarehouseStatusDateTime;
			set
			{
				SetNonPersistentPropertyValue(TransitWarehouseStatusDateTimeInfo, ref transitWarehouseStatusDateTime, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateTransitWarehouseStatusDateTime();
				}
			}
		}
		ZDateTime transitWarehouseStatusDateTime;

		public ZPropertyInfo TransitWarehouseStatusDateTimeInfo => GetZPropertyInfo(nameof(TransitWarehouseStatusDateTime));

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		public UpdateLastKnownTransitWarehouseValidation Validation => new UpdateLastKnownTransitWarehouseValidation(this);

		#endregion

		#region Lookups

		public CodeDescriptionPairList TransitWarehouseStatusList
		{
			get
			{
				if (transitWarehouseStatusList == null)
				{
					transitWarehouseStatusList = new CodeDescriptionPairList();
					transitWarehouseStatusList.Add(new CodeDescriptionPair(FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received, FreightConstants.PacklineLastKnownTransitWarehouseStatus.Descriptions.Received));
					transitWarehouseStatusList.Add(new CodeDescriptionPair(FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched, FreightConstants.PacklineLastKnownTransitWarehouseStatus.Descriptions.Dispatched));
				}

				return transitWarehouseStatusList;
			}
		}
		CodeDescriptionPairList transitWarehouseStatusList;

		public BindToLists BindToLists => BindToLists.GetCachedLists(Factory);

		#endregion
	}

	public class UpdateLastKnownTransitWarehouseValidation : ZValidation
	{
		public UpdateLastKnownTransitWarehouseValidation(UpdateLastKnownTransitWarehouseApplicator parent)
			: base(parent)
		{
			this.parent = parent;
		}

		public void Add(UpdateLastKnownTransitWarehouseValidation validation)
		{
			ZValidationInternals.Add(validation);
		}

		public void Remove(UpdateLastKnownTransitWarehouseValidation validation)
		{
			ZValidationInternals.Remove(validation);
		}

		#region ValidateAll

		public override void ValidateAll()
		{
			using (ParentListInternals.SuspendListChanged())
			{
				ValidateAllCore();
			}
		}

		protected void ValidateAllCore()
		{
			ValidateTransitWarehouseAddress();
			ValidateTransitWarehouseStatus();
			ValidateTransitWarehouseStatusDateTime();
		}

		#endregion

		#region TransitWarehouseAddress

		public void ValidateTransitWarehouseAddress()
		{
			ZValidationInternals.Validate(Parent.TransitWarehouseAddressInfo, new RunValidationInvoker(CheckTransitWarehouseAddress));
		}

		void CheckTransitWarehouseAddress()
		{
			MandatoryValidation.CheckEntered(Parent.TransitWarehouseAddressInfo);
		}

		#endregion

		#region TransitWarehouseStatus

		public void ValidateTransitWarehouseStatus()
		{
			ZValidationInternals.Validate(Parent.TransitWarehouseStatusInfo, new RunValidationInvoker(CheckTransitWarehouseStatus));
		}

		void CheckTransitWarehouseStatus()
		{
			MandatoryValidation.CheckEntered(Parent.TransitWarehouseStatusInfo);
			ListValidation.ErrorIfInvalidCode(Parent.TransitWarehouseStatusInfo, Parent.TransitWarehouseStatusList);
		}

		#endregion

		#region TransitWarehouseStatusDateTime

		public void ValidateTransitWarehouseStatusDateTime()
		{
			ZValidationInternals.Validate(Parent.TransitWarehouseStatusDateTimeInfo, new RunValidationInvoker(CheckTransitWarehouseStatusDateTime));
		}

		void CheckTransitWarehouseStatusDateTime()
		{
			MandatoryValidation.CheckEntered(Parent.TransitWarehouseStatusDateTimeInfo);

			if (!Parent.TransitWarehouseStatusDateTime.IsEmpty)
			{
				if (!Parent.TransitWarehouseStatusDateTime.IsValid)
				{
					Parent.TransitWarehouseStatusDateTimeInfo.AddError(Res.GetString("26673639-3751-423B-953B-13911D3DA8B5", "Please enter a valid value."));
				}
				else if (Parent.TransitWarehouseStatusDateTime.Date > ZDateTime.Now.Date)
				{
					Parent.TransitWarehouseStatusDateTimeInfo.AddWarning(Res.GetString("1E3A7989-7F24-43D7-98AA-3C13EA6D723A", "Last Known TW Date should only allow current or past date."));
				}
			}
		}

		#endregion

		public override Type AutoValidationType => typeof(UpdateLastKnownTransitWarehouseValidation);

		public UpdateLastKnownTransitWarehouseApplicator Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get => parent;
		}
		readonly UpdateLastKnownTransitWarehouseApplicator parent;

		IValidationInternals ZValidationInternals => this;
		ISingleElementListInternal ParentListInternals => Parent;
	}
}
