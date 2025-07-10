using System.Text;
using static System.FormattableString;

namespace CargoWise.RefDbRepo.GBReferenceData.Services.GVMSReferenceData.Models
{
	public class RuleFailure : IReferenceDataModel
	{
		public string RuleId { get; set; }
		public string RuleDescription { get; set; }

		public bool IsValid(StringBuilder errorCollector)
		{
			var validationErrors = new StringBuilder();
			var valid = true;

			if (string.IsNullOrEmpty(RuleId))
			{
				validationErrors.Append("PortId is required. ");
				valid = false;
			}

			if (!valid)
			{
				var msg = Invariant($"Rule validation error. Key: '{RuleId}' Errors: '{validationErrors}' ");
				errorCollector.AppendLine(msg);
			}

			return valid;
		}
	}
}
