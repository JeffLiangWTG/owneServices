using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Registry.Business;
using static Enterprise.Registry.Business.HVLVPreScreeningField;

namespace Enterprise.eTail.Business
{
	public abstract class PreScreeningAction
	{
		public PreScreeningAction(HVLVPreScreeningField field, HVLVConsignment consignment)
		{
			this.field = Argument.NotNull(field, nameof(field));
			this.consignment = Argument.NotNull(consignment, nameof(consignment));
		}

		protected readonly HVLVPreScreeningField field;
		protected readonly HVLVConsignment consignment;

		public void PerformPreScreening(HVLVConsignmentPreScreeningResult result)
		{
			PerformPreScreeningAndPopulateResult(result);
		}

		protected abstract void PerformPreScreeningAndPopulateResult(HVLVConsignmentPreScreeningResult result);

		public enum MessageLevel
		{
			None,
			NotifyOnlyWarning,
			Warning,
			Error
		}

		protected void AddPreScreeningDetailsCore(ZString preScreeningDetails, HVLVConsignmentPreScreeningResult result, MessageLevel? messageLevelOverride = null)
		{
			MessageLevel messageLevel;
			if (messageLevelOverride.HasValue)
			{
				messageLevel = messageLevelOverride.Value;
			}
			else
			{
				switch (field.ValidationRule)
				{
					case ValidationRuleCodes.Error:
						messageLevel = MessageLevel.Error;
						break;
					case ValidationRuleCodes.Warning:
						messageLevel = MessageLevel.Warning;
						break;
					default:
						messageLevel = MessageLevel.None;
						break;
				}
			}

			if (messageLevel == MessageLevel.Warning)
			{
				consignment.AddPreScreeningWarningDetails(preScreeningDetails);
				result.AddWarningMessage(preScreeningDetails, field.EmailNotificationGroup, field.NotifyStaffMember);
			}
			else if (messageLevel == MessageLevel.Error)
			{
				consignment.AddPreScreeningErrorDetails(preScreeningDetails);
				result.AddErrorMessage(preScreeningDetails, field.EmailNotificationGroup, field.NotifyStaffMember);
			}
			else if (messageLevel == MessageLevel.NotifyOnlyWarning)
			{
				consignment.AddPreScreeningNotifyOnlyWarningDetails(preScreeningDetails);
				result.AddNotifyOnlyWarningMessage(preScreeningDetails, field.EmailNotificationGroup, field.NotifyStaffMember);
			}
		}

		protected string ScreeningError => Res.GetString("4961b7a3-cb58-4003-9a4c-d58b4c9326a0", "Screening Error");
		protected string NormalNotificationMessage => Res.GetString("59bf065c-8a1b-42e3-89fb-cf5e567d4806", "has been listed for screening");
	}
}
