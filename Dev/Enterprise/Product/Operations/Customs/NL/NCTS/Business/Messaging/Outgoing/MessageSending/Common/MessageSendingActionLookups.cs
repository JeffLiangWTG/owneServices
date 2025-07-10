using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NL.NCTS.Business;

public class MessageSendingActionLookups : NctsHeaderMessageSendingObjectLookups
{
	public MessageSendingActionLookups(MessageSendingAction parent) : base(parent)
	{
	}

	new MessageSendingAction Parent => (MessageSendingAction)base.Parent;

	public new CodeDescriptionPairList MessageTypeList
	{
		get
		{
			var parent = Parent;
			var movementHeader = parent.MovementHeader;
			var subApplicationCode = movementHeader.BM_SubApplicationCode;
			var additionalDeclarationType = movementHeader.BM_AdditionalDeclarationType;
			var customsStatus = movementHeader.BM_CustomsStatus;
			var phase = movementHeader.BM_Phase;
			var messageStatus = parent.Header.EffectiveMessageStatus;
			var invalidErrorFailedMessageStatus = new ZString[] { LogicalStatusList.Codes.Invalid, LogicalStatusList.Codes.Error, LogicalStatusList.Codes.Failed };

			return Factory.GetCachedValue($"NL.NCTS.Business.MessageSendingConfiguration.MessageTypeList_{subApplicationCode}.{additionalDeclarationType}.{customsStatus}.{phase}.{messageStatus}", () =>
			{
				var result = new CodeDescriptionPairList();
				if (subApplicationCode == NctsMoveHeaderType.Codes.Departure)
				{
					if ((additionalDeclarationType == NctsTypeOfAdditionalDeclarationList.Codes.A || additionalDeclarationType == NctsTypeOfAdditionalDeclarationList.Codes.D)
						&& customsStatus.IsEmpty
						&& (phase.IsEmpty || phase == NctsMovementHeaderTransactionStatusList.Codes.Declaration))
					{
						result.AddPair(NctsMessageTypeListNL.Codes.Declaration, NctsMessageTypeListNL.Descriptions.Declaration);
					}
					else if (additionalDeclarationType == NctsTypeOfAdditionalDeclarationList.Codes.D
						&& customsStatus == NCTS5DepartureCustomsStatusList.Codes.PreLodged
						&& (phase == NctsMovementHeaderTransactionStatusList.Codes.Amendment
							|| phase == NctsMovementHeaderTransactionStatusList.Codes.Cancellation
							|| phase == NctsMovementHeaderTransactionStatusList.Codes.Declaration
							|| (phase == NctsMovementHeaderTransactionStatusList.Codes.Presentation && messageStatus.In(invalidErrorFailedMessageStatus))))
					{
						result.AddPair(NctsMessageTypeListNL.Codes.Amendment, NctsMessageTypeListNL.Descriptions.Amendment);
						result.AddPair(NctsMessageTypeListNL.Codes.InvalidationCancellation, NctsMessageTypeListNL.Descriptions.InvalidationCancellation);
						result.AddPair(NctsMessageTypeListNL.Codes.PresentationNotification, NctsMessageTypeListNL.Descriptions.PresentationNotification);
					}
					else if (additionalDeclarationType == NctsTypeOfAdditionalDeclarationList.Codes.A)
					{
						if ((phase == NctsMovementHeaderTransactionStatusList.Codes.Declaration
							|| phase == NctsMovementHeaderTransactionStatusList.Codes.Amendment)
								&& (customsStatus == NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid
								|| customsStatus == NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested))
						{
							result.AddPair(NctsMessageTypeListNL.Codes.Amendment, NctsMessageTypeListNL.Descriptions.Amendment);
						}
						else if ((phase == NctsMovementHeaderTransactionStatusList.Codes.Declaration
							|| phase == NctsMovementHeaderTransactionStatusList.Codes.Amendment
							|| phase == NctsMovementHeaderTransactionStatusList.Codes.Cancellation)
								&& customsStatus == NCTS5DepartureCustomsStatusList.Codes.MrnAllocated)
						{
							result.AddPair(NctsMessageTypeListNL.Codes.Amendment, NctsMessageTypeListNL.Descriptions.Amendment);
							result.AddPair(NctsMessageTypeListNL.Codes.InvalidationCancellation, NctsMessageTypeListNL.Descriptions.InvalidationCancellation);
						}
						else if ((phase == NctsMovementHeaderTransactionStatusList.Codes.Declaration
							|| phase == NctsMovementHeaderTransactionStatusList.Codes.NonArrivedMovement)
								&& customsStatus == NCTS5DepartureCustomsStatusList.Codes.UnderEnquiry)
						{
							result.AddPair(NctsMessageTypeListNL.Codes.ResponseOnRequestForNonArrivedMovement, NctsMessageTypeListNL.Descriptions.ResponseOnRequestForNonArrivedMovement);
						}
						else if (customsStatus == NCTS5DepartureCustomsStatusList.Codes.Acknowledged
							&& (phase == NctsMovementHeaderTransactionStatusList.Codes.Amendment
								|| phase == NctsMovementHeaderTransactionStatusList.Codes.Cancellation
								|| phase == NctsMovementHeaderTransactionStatusList.Codes.Declaration))
						{
							result.AddPair(NctsMessageTypeListNL.Codes.Amendment, NctsMessageTypeListNL.Descriptions.Amendment);
							result.AddPair(NctsMessageTypeListNL.Codes.InvalidationCancellation, NctsMessageTypeListNL.Descriptions.InvalidationCancellation);
						}
						else if (phase == NctsMovementHeaderTransactionStatusList.Codes.Declaration)
						{
							if (customsStatus == NctsMovementHeaderTransactionStatusList.Codes.NoFullReleaseOfGoodsMovementRemainsOpen
								|| customsStatus == NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit)
							{
								result.AddPair(NctsMessageTypeListNL.Codes.InvalidationCancellation, NctsMessageTypeListNL.Descriptions.InvalidationCancellation);
							}
							else if (customsStatus == NCTS5DepartureCustomsStatusList.Codes.AdditionalDocumentsRequest
								|| customsStatus == NCTS5DepartureCustomsStatusList.Codes.DecisionToControl
								|| customsStatus == NCTS5DepartureCustomsStatusList.Codes.DiscrepanciesAtDestination)
							{
								result.AddPair(NctsMessageTypeListNL.Codes.RequestARelease, NctsMessageTypeListNL.Descriptions.RequestARelease);
							}
						}
					}
				}
				else if (subApplicationCode == NctsMoveHeaderType.Codes.Arrival && additionalDeclarationType.IsEmpty)
				{
					if (customsStatus.IsEmpty && (phase.IsEmpty || phase == NctsMovementHeaderTransactionStatusList.Codes.Arrival))
					{
						result.AddPair(NctsMessageTypeListNL.Codes.ArrivalNotification, NctsMessageTypeListNL.Descriptions.ArrivalNotification);
					}
					else if ((customsStatus == NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted
							&& (phase == NctsMovementHeaderTransactionStatusList.Codes.Arrival
							|| phase == NctsMovementHeaderTransactionStatusList.Codes.UnloadingPermission
							|| phase == NctsMovementHeaderTransactionStatusList.Codes.UnloadingRemarks))
						|| (customsStatus == NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks
							&& (phase == NctsMovementHeaderTransactionStatusList.Codes.UnloadingPermission
							|| phase == NctsMovementHeaderTransactionStatusList.Codes.UnloadingRemarks)))
					{
						result.AddPair(NctsMessageTypeListNL.Codes.UnloadingRemarks, NctsMessageTypeListNL.Descriptions.UnloadingRemarks);
					}
				}

				return result;
			});
		}
	}
}
