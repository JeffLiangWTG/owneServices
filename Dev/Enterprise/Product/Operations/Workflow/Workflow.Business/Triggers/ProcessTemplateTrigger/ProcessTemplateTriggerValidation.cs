using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Workflow.Business
{
	public class ProcessTemplateTriggerValidation : AutoProcessTemplateTriggerValidation
	{
		public ProcessTemplateTriggerValidation(AutoProcessTemplateTrigger parent)
			: base(parent)
		{
		}

		protected override void CheckP9T_Sequence()
		{
			base.CheckP9T_Sequence();

			CompareValidation.CheckGreaterThanOrEqualTo(Parent.P9T_SequenceInfo, 1m);
		}

		protected override void CheckP9T_Description()
		{
			base.CheckP9T_Description();

			MandatoryValidation.CheckEntered(Parent.P9T_DescriptionInfo);
		}

		protected override void CheckP9T_TriggerFiredCountdown()
		{
			if (Parent.P9T_TriggerFiredCountdown <= 0 && !Parent.ReadOnly && !Parent.P9T_TriggerFiredCountdownInfo.ReadOnly)
			{
				Parent.P9T_TriggerFiredCountdownInfo.AddWarning(TriggerConditionsViewModelValidation.TriggerFiredCountZeroMessage);
			}
		}

		protected override void CheckP9T_IsEstimate()
		{
			if (!Parent.P9T_IsEstimate)
			{
				var trigger = (ProcessTemplateTrigger)Parent;

				if (trigger.TriggerActions.Cast<ProcessTaskNotification>().Any(n => n.PQ_Offset.IsValid && n.PQ_Offset.TimeSpan6MonthsFromStartOfYear < TimeSpan.Zero))
				{
					Parent.P9T_IsEstimateInfo.AddError(Res.GetString("CD6CF373-AE5E-45CE-B075-07586F20BB89", "The trigger cannot respond to actual events as there are DLY completion trigger actions set with a negative offset. Negative offsets are allowed for triggers responding to estimate events only."));
				}
			}
		}

		protected override void CheckP9T_SuppressDuplicates()
		{
			base.CheckP9T_SuppressDuplicates();
			if (Parent.P9T_SuppressDuplicates && Parent.P9T_DelayDurationSeconds == 0)
			{
				Parent.P9T_SuppressDuplicatesInfo.AddError(Res.GetString("68B48777-FABC-421E-98C9-7C0EFFA67D6E", "You cannot set 'Suppress Duplicates' when 'Delay Duration' is 0."));
			}
		}
	}
}
