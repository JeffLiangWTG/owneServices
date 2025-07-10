using System;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.Customs.US.Business
{
	public abstract class USAutoSendCustomsMessageProcessor : Customs.Business.AutoSendCustomsMessageProcessor
	{
		protected USAutoSendCustomsMessageProcessor(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
		}

		protected abstract ZString EntryType { get; }

		protected virtual ZBool IsBondedWarehouseIntegrated
		{
			get { return true; }
		}

		protected sealed override void ValidateDeclarationCore(Customs.Business.CusEntryHeader entryHeader)
		{
			var oldValidationModes = Declaration.ValidationModes;
			var entry = (CusEntryHeader)entryHeader;
			if (entry.IsFormalEntry)
			{
				Declaration.ValidationModes = ValidationModes.EntrySummary;
			}
			else if (entry.IsACECargoRelease)
			{
				Declaration.ValidationModes = ValidationModes.CargoRelease;
			}

			Declaration.RunPreSaveValidation();
			Declaration.ValidationModes = oldValidationModes;
		}

		protected sealed override ZBool SendCustomsMessageCore(INotifications notifications, Customs.Business.CusEntryHeader entryHeader)
		{
			MQEDIMessage result = null;
			var entry = (CusEntryHeader)entryHeader;
			var needsToRestoreToPreMessagingState = false;
			Action restoreToPreMessagingState = null;
			try
			{
				if (IsBondedWarehouseIntegrated)
				{
					Func<PublishToUniversalResult> preMessagingAction = null;
					var preMessagingActionResult = Declaration.PreMessagingAction(ref preMessagingAction, ref restoreToPreMessagingState, ImportMessageSendingMessageType.Original, shouldSave: false);

					var errorMessages = preMessagingActionResult.GetErrorMessageForCheckFieldsForBondedWarehouse();
					if (!errorMessages.IsEmpty)
					{
						notifications.AddWarning(errorMessages);
						return false;
					}

					var isBondedWarehouse = preMessagingActionResult.IsBondedWarehouse;
					if (isBondedWarehouse && preMessagingAction != null)
					{
						var universalResult = preMessagingAction();
						if (universalResult.ResultType == UniversalResult.HadErrors)
						{
							notifications.AddWarning(universalResult.ErrorMessage);
							return false;
						}
						needsToRestoreToPreMessagingState = true;

						((BondedWarehousingHelper)Declaration.BondedWarehousingHelper).UpdateWarehouseWithdrawal();
					}
				}

				result = SendMessagesCore(entry);
				if (result != null)
				{
					var broker = Declaration.JE_GS_NKCusAgent;
					if (!broker.IsEmpty)
					{
						result.EM_SystemCreateUser = broker;
					}

					CalculateRelatedPropertiesAfterSending(result, entry);
				}
			}
			finally
			{
				if (IsBondedWarehouseIntegrated && needsToRestoreToPreMessagingState && restoreToPreMessagingState != null)
				{
					restoreToPreMessagingState();
				}
			}

			return result != null;
		}

		protected abstract MQEDIMessage SendMessagesCore(CusEntryHeader entry);

		protected abstract void CalculateRelatedPropertiesAfterSending(MQEDIMessage message, CusEntryHeader entry);
	}
}
