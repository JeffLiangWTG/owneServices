using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class ProcessWorkflowExceptionTypeValidation : AutoProcessWorkflowExceptionTypeValidation
	{
		public ProcessWorkflowExceptionTypeValidation(AutoProcessWorkflowExceptionType parent)
			: base(parent)
		{
		}

		protected override void CheckWET_Code()
		{
			base.CheckWET_Code();
			MandatoryValidation.CheckEntered(Parent.WET_CodeInfo);
			ValidateCodeUniqueness();
		}

		protected override void CheckWET_Description()
		{
			base.CheckWET_Description();
			MandatoryValidation.CheckEntered(Parent.WET_DescriptionInfo);
			TranslatableDataFieldAttribute.Validate(Parent.WET_DescriptionInfo);
		}

		protected override void CheckWET_Category()
		{
			base.CheckWET_Category();
			ListValidation.ErrorIfInvalidCode(Parent.WET_CategoryInfo, Parent.Lookups.ExceptionCategories);
		}

		protected override void CheckWET_JobType()
		{
			base.CheckWET_JobType();
			ListValidation.ErrorIfInvalidCode(Parent.WET_JobTypeInfo, Parent.Lookups.JobTypes);
		}

		protected override void CheckWET_DefaultDurationHours()
		{
			base.CheckWET_DefaultDurationHours();
			MandatoryValidation.CheckNotNegative(Parent.WET_DefaultDurationHoursInfo);
		}

		protected override void CheckWET_IsCauseRequired()
		{
			base.CheckWET_IsCauseRequired();

			var type = (ProcessWorkflowExceptionType)Parent;
			var hasActiveCause = type.Causes.Cast<ProcessWorkflowExceptionCause>().Any(c => c.WEC_IsActive);

			if (type.WET_IsActive && type.WET_IsCauseRequired && !hasActiveCause)
			{
				Parent.WET_IsCauseRequiredInfo.AddError(
					Res.GetString("1C89EDC5-F68D-40C0-B3A9-86FD401A16DB", "Types with Cause Required must specify at least one active Cause."));
			}
		}

		protected override void CheckWET_IsResolutionRequired()
		{
			base.CheckWET_IsResolutionRequired();

			var type = (ProcessWorkflowExceptionType)Parent;
			var hasActiveResolution = type.Resolutions.Cast<ProcessWorkflowExceptionResolution>().Any(c => c.WER_IsActive);

			if (type.WET_IsActive && type.WET_IsResolutionRequired && !hasActiveResolution)
			{
				Parent.WET_IsResolutionRequiredInfo.AddError(
					Res.GetString("ECBC7B31-B47B-4876-B4B1-F901843552FA", "Types with Resolution Required must specify at least one active Resolution."));
			}
		}

		public override void ValidateAll()
		{
			base.ValidateAll();

			if (Parent.WET_IsActive)
			{
				ValidateCauses();
				ValidateResolutions();
			}
		}

		void ValidateCodeUniqueness()
		{
			var matchingTypes = ((ProcessWorkflowExceptionType)Parent).Types.Where(t => t.PK != Parent.PK && t.WET_Code == Parent.WET_Code);
			if (matchingTypes.Any())
			{
				Parent.WET_CodeInfo.AddError(Res.GetString("193B4288-F983-4037-AC38-AC2B8793E63B",
					@"Type Code {0} already exists. Specify a different Type Code", Parent.WET_Code));
			}
		}

		void ValidateCauses()
		{
			var type = (ProcessWorkflowExceptionType)Parent;
			var causes = type.Causes.Cast<ProcessWorkflowExceptionCause>().ToList();
			causes.ForEach(c => c.ClearRowNotifications());

			var defaults = causes.Where(c => c.WEC_IsDefault).ToList();
			if (defaults.Count > 1)
			{
				defaults.ForEach(r => r.AddRowError(
					Res.GetString("2D126858-3413-4F6F-8151-1AFFB3E12B15", "There can be no more than one Default Cause for a given Type.")));
			}
			else if (type.WET_IsCauseRequired && defaults.Count == 1 && !defaults[0].WEC_IsActive)
			{
				defaults.ForEach(r => r.AddRowError(
					Res.GetString("5EDCC0E0-89B0-4321-86B6-9B8E6270053B", "The default Cause must always be active.")));
			}
			else if (type.WET_IsCauseRequired && type.WET_IsSystem && defaults.Count == 0)
			{
				causes.ForEach(r => r.AddRowError(
					Res.GetString("7C31B642-DE2A-419E-A49A-1F3A1D614107", "For System Types with Cause Required a single default Cause must be specified.")));
			}
			else if (type.WET_IsCauseRequired && !causes.Any(c => c.WEC_IsActive))
			{
				causes.ForEach(r => r.AddRowError(
					Res.GetString("1C89EDC5-F68D-40C0-B3A9-86FD401A16DB", "Types with Cause Required must specify at least one active Cause.")));
			}
		}

		void ValidateResolutions()
		{
			var type = (ProcessWorkflowExceptionType)Parent;
			var resolutions = type.Resolutions.Cast<ProcessWorkflowExceptionResolution>().ToList();
			resolutions.ForEach(r => r.ClearRowNotifications());

			var defaults = resolutions.Where(c => c.WER_IsDefault).ToList();
			if (defaults.Count > 1)
			{
				defaults.ForEach(r => r.AddRowError(
					Res.GetString("4E994ADC-39EB-4A3D-9E6F-027F3-49CFE46", "There can be no more than one Default Resolution for a given Type.")));
			}
			else if (type.WET_IsResolutionRequired && defaults.Count == 1 && !defaults[0].WER_IsActive)
			{
				defaults.ForEach(r => r.AddRowError(
					Res.GetString("3FB88947-55E3-4BCC-8B26-F0EE4F203718", "The default Resolution must always be active.")));
			}
			else if (type.WET_IsResolutionRequired && type.WET_IsSystem && defaults.Count == 0)
			{
				resolutions.ForEach(r => r.AddRowError(
					Res.GetString("BBB21021-D06C-423C-B825-DF6FF03BEA1D", "For System Types with Resolution Required a single default Resolution must be specified.")));
			}
			else if (type.WET_IsResolutionRequired && !resolutions.Any(c => c.WER_IsActive))
			{
				resolutions.ForEach(r => r.AddRowError(
					Res.GetString("ECBC7B31-B47B-4876-B4B1-F901843552FA", "Types with Resolution Required must specify at least one active Resolution.")));
			}
		}
	}
}
