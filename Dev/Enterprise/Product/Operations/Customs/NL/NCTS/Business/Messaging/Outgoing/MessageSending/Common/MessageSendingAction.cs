using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.NL.NCTS.Business;

public class MessageSendingAction : NctsHeaderMessageSendingObject
{
	public MessageSendingAction(NctsCommonMovementHeader movementHeader) : base(Argument
		.NotNull(movementHeader, nameof(movementHeader)).Header)
	{
		using (GetValidationDataSuspender())
		using (SuspendSettingHasChanges())
		{
			MovementHeader = movementHeader;
			Header = (NctsHeader)MovementHeader.Header;
			SetDefaultMessageType();
		}
	}

	public readonly NctsCommonMovementHeader MovementHeader;
	public readonly NctsHeader Header;

	public new class Schema : NctsHeaderMessageSendingObject.Schema
	{
		public const string PresentationDateTime = nameof(MessageSendingAction.PresentationDateTime);
		public const string TCI11 = nameof(MessageSendingAction.TCI11);
		public const string AgreeWithMinorDiscrepancies = nameof(MessageSendingAction.AgreeWithMinorDiscrepancies);
		public const string IsTestDeclaration = nameof(MessageSendingAction.IsTestDeclaration);
	}

	protected void SetDefaultMessageType()
	{
		MessageType = ZString.Empty;
		var list = Lookups.MessageTypeList;
		if (list.Count == 1)
		{
			MessageType = list[0].Code;
		}
	}

	[ResourceStringData("Enterprise.Customs.NL.NCTS.Business.MessageSendingAction|IsTestDeclaration", Caption = "Test?")]
	public ZBool IsTestDeclaration
	{
		get => isTestDeclaration;
		set => SetNonPersistentPropertyValue(IsTestDeclarationInfo, ref isTestDeclaration, value);
	}
	ZBool isTestDeclaration;
	public ZPropertyInfo IsTestDeclarationInfo => GetZPropertyInfo(Schema.IsTestDeclaration);

	#region Lookups

	public new MessageSendingActionLookups Lookups => (MessageSendingActionLookups)base.Lookups;

	protected override NctsHeaderMessageSendingObjectLookups GetNewLookups() => new MessageSendingActionLookups(this);

	#endregion

	#region Validation

	public new MessageSendingActionValidation Validation => (MessageSendingActionValidation)base.Validation;

	protected override NctsHeaderMessageSendingObjectValidation GetNewValidation() =>
		new MessageSendingActionValidation(this);

	#endregion

	#region PresentationDateTime

	[ResourceStringData("Enterprise.Customs.NL.NCTS.Business.MessageSendingAction|PresentationDateTime", Caption = "Presentation Date And Time")]
	[ReadOnlyMember(nameof(PresentationDateTimeReadOnly))]
	public ZDateTime PresentationDateTime
	{
		get => MovementHeader.BM_ArrivalDate;
		set
		{
			MovementHeader.BM_ArrivalDate = value;
			PresentationDateTimeInfo.RefreshBinding();
		}
	}

	public ZPropertyInfo PresentationDateTimeInfo => GetZPropertyInfo(Schema.PresentationDateTime);

	public bool PresentationDateTimeReadOnly => Header.IsDepartureMovement
		? MessageType == NctsMessageTypeListNL.Codes.Amendment
			? HasLogWithGIVReference
			: !IsPresentationDateTimeEnabledForCustomsStatusAndPhase
		: MovementHeader.BM_AdditionalDeclarationType != NctsTypeOfAdditionalDeclarationList.Codes.D;

	bool HasLogWithGIVReference => Header.Logs.HasLogWith(l => l.SL_Reference == NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid);

	bool IsPresentationDateTimeEnabledForCustomsStatusAndPhase
	{
		get
		{
			var enabledForCustomsStatus = false;
			var enabledForPhase = true;

			switch (MovementHeader.BM_CustomsStatus)
			{
				case NctsTransitStatusList.Codes.DeclarationAccepted:
				case NctsTransitStatusList.Codes.DeclarationRejected:
				case NctsTransitStatusList.Codes.RequestForAmendment:
				case NctsMessageTypeListNL.Codes.Amendment:
				case NctsTransitStatusList.Codes.Unknown:
					enabledForCustomsStatus = true;
					break;
			}

			if (enabledForCustomsStatus)
			{
				switch (MovementHeader.BM_Phase)
				{
					case NctsMovementHeaderTransactionStatusList.Codes.AmendmentSent:
					case NctsMovementHeaderTransactionStatusList.Codes.InvalidationSent:
					case NctsMovementHeaderTransactionStatusList.Codes.DeclarationSent:
					case NctsMovementHeaderTransactionStatusList.Codes.RequestForReleaseSent:
					case NctsMovementHeaderTransactionStatusList.Codes.InformationNonArrivedMovementSent:
					case NctsMovementHeaderTransactionStatusList.Codes.PresentationNotificationSent:
					case NctsMovementHeaderTransactionStatusList.Codes.ArrivalNotificationSent:
					case NctsMovementHeaderTransactionStatusList.Codes.UnloadingRemarksSent:
						enabledForPhase = false;
						break;
				}
			}

			return enabledForCustomsStatus && enabledForPhase;
		}
	}

	#endregion

	[ResourceStringData("Enterprise.Customs.NL.NCTS.Business.MessageSendingAction|TCI11", Caption = "TCI11 Date")]
	public ZDateTime TCI11
	{
		get => tci11Date;
		set => SetNonPersistentPropertyValue(TCI11Info, ref tci11Date, value);
	}

	ZDateTime tci11Date;

	public ZPropertyInfo TCI11Info => GetZPropertyInfo(Schema.TCI11);

	[ResourceStringData("Enterprise.Customs.NL.NCTS.Business.MessageSendingAction|AgreeWithMinorDiscrepancies", Caption = "Agree with minor discrepancies?")]
	public ZBool AgreeWithMinorDiscrepancies
	{
		get => agreeWithMinorDiscrepancies;
		set => SetNonPersistentPropertyValue(AgreeWithMinorDiscrepanciesInfo, ref agreeWithMinorDiscrepancies, value);
	}
	ZBool agreeWithMinorDiscrepancies;

	public ZPropertyInfo AgreeWithMinorDiscrepanciesInfo => GetZPropertyInfo(Schema.AgreeWithMinorDiscrepancies);

	protected override bool Justification_ReadOnly => false;

	public bool ShowValidationErrors => (string)MessageType switch
	{
		NctsMessageTypeListNL.Codes.InvalidationCancellation or NctsMessageTypeListNL.Codes.ResponseOnRequestForNonArrivedMovement or NctsMessageTypeListNL.Codes.RequestARelease => false,
		_ => true,
	};
}
