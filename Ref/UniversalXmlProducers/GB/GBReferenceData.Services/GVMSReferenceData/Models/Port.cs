using System.Text;
using static System.FormattableString;

namespace CargoWise.RefDbRepo.GBReferenceData.Services.GVMSReferenceData.Models
{
	public class Port : IReferenceDataModel
	{
		public string PortId { get; set; }
		public string PortRegion { get; set; }
		public string PortEffectiveFrom { get; set; }
		public string PortDescription { get; set; }
		public string ChiefPortCode { get; set; }
		public string CdsPortCode { get; set; }
		public string OfficeOfTransitCustomsOfficeCode { get; set; }
		public string TimezoneId { get; set; }

		public bool IsValid(StringBuilder errorCollector)
		{
			var validationErrors = new StringBuilder();
			var valid = true;

			if (string.IsNullOrEmpty(PortId))
			{
				validationErrors.Append("PortId is required. ");
				valid = false;
			}

			if (!int.TryParse(PortId, out _))
			{
				validationErrors.Append("PortId is invalid. ");
				valid = false;
			}

			if (!valid)
			{
				var msg = Invariant($"Port validation error. Key: '{PortId}' Errors: '{validationErrors}'");
				errorCollector.AppendLine(msg);
			}

			return valid;
		}
	}
}
