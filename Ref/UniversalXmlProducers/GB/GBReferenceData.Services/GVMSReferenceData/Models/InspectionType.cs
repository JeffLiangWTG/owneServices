using System.Text;
using static System.FormattableString;

namespace CargoWise.RefDbRepo.GBReferenceData.Services.GVMSReferenceData.Models
{
	public class InspectionType : IReferenceDataModel
	{
		public string InspectionTypeId { get; set; }
		public string Description { get; set; }

		public bool IsValid(StringBuilder errorCollector)
		{
			var validationErrors = new StringBuilder();
			var valid = true;

			if (string.IsNullOrEmpty(InspectionTypeId))
			{
				validationErrors.Append($"{nameof(InspectionTypeId)} is required. ");
				valid = false;
			}

			if (string.IsNullOrEmpty(Description))
			{
				validationErrors.Append($"{nameof(Description)} is required. ");
				valid = false;
			}

			if (!valid)
			{
				var msg = Invariant($"Inspection Type validation error. Key: '{InspectionTypeId}' Errors: '{validationErrors}'");
				errorCollector.AppendLine(msg);
			}

			return valid;
		}
	}
}
