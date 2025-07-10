using System.Linq;
using System.Text;
using static System.FormattableString;

namespace CargoWise.RefDbRepo.GBReferenceData.Services.GVMSReferenceData.Models
{
	public class Carrier : IReferenceDataModel
	{
		public string CarrierId { get; set; }
		public string CarrierName { get; set; }
		public string CountryCode { get; set; }

		public bool IsValid(StringBuilder errorCollector)
		{
			var validationErrors = new StringBuilder();
			var valid = true;

			if (string.IsNullOrEmpty(CarrierId))
			{
				validationErrors.Append("CarrierId is required. ");
				valid = false;
			}

			if (!int.TryParse(CarrierId, out _))
			{
				validationErrors.Append("CarrierId is invalid. ");
				valid = false;
			}

			if (CountryCode.Length != 2 || !CountryCode.All(char.IsLetter))
			{
				validationErrors.Append("CountryCode is invalid. ");
				valid = false;
			}

			if (!valid)
			{
				var msg = Invariant($"Carrier validation error. Key: '{CarrierId}' Errors: '{validationErrors}'");
				errorCollector.AppendLine(msg);
			}

			return valid;
		}
	}
}
