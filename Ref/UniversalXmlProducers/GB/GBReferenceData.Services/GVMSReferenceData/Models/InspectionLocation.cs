using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using static System.FormattableString;

namespace CargoWise.RefDbRepo.GBReferenceData.Services.GVMSReferenceData.Models
{
	public class InspectionLocation : IReferenceDataModel
	{
		public string LocationId { get; set; }
		public string LocationDescription { get; set; }
		public InspectionLocationAddress Address { get; set; }
		public string LocationType { get; set; }
		public List<string> SupportedDirections { get; set; }
		public DateTime LocationEffectiveFrom { get; set; }
		public DateTime LocationEffectiveTo { get; set; }
		public List<string> SupportedInspectionTypeIds { get; set; }
		public List<string> RequiredInspectionLocations { get; set; }

		public bool IsValid(StringBuilder errorCollector)
		{
			var validationErrors = new StringBuilder();
			var valid = true;

			if (string.IsNullOrEmpty(LocationId))
			{
				validationErrors.Append($"{nameof(LocationId)} is required. ");
				valid = false;
			}

			if (string.IsNullOrEmpty(LocationDescription))
			{
				validationErrors.Append($"{nameof(LocationDescription)} is required. ");
				valid = false;
			}

			if (string.IsNullOrEmpty(Address.ToString()))
			{
				validationErrors.Append($"{nameof(Address)} is required (Line(s) and postcode)");
				valid = false;
			}

			if (string.IsNullOrEmpty(LocationType))
			{
				validationErrors.Append($"{nameof(LocationType)} is required. ");
				valid = false;
			}

			if (SupportedDirections.Count == 0)
			{
				validationErrors.Append($"{nameof(SupportedDirections)} is required. ");
				valid = false;
			}

			if (SupportedInspectionTypeIds.Count == 0)
			{
				validationErrors.Append($"{nameof(SupportedInspectionTypeIds)} is required. ");
				valid = false;
			}

			if (RequiredInspectionLocations.Count == 0)
			{
				validationErrors.Append($"{nameof(RequiredInspectionLocations)} is required. ");
				valid = false;
			}


			if (!valid)
			{
				var msg = Invariant($"Inspection Location validation error. Key: '{LocationId}' Errors: '{validationErrors}'");
				errorCollector.AppendLine(msg);
			}

			return valid;
		}

		public class InspectionLocationAddress
		{
			public List<string> Lines { get; set; }
			public string Town { get; set; }
			public string Postcode { get; set; }

			public override string ToString()
			{
				var result = new StringBuilder();
				foreach (var line in Lines.Where(l => !string.IsNullOrEmpty(l)))
				{
					result.Append(CultureInfo.InvariantCulture, $"{line}, ");
				}

				if (!string.IsNullOrEmpty(Town))
				{
					result.Append(CultureInfo.InvariantCulture, $"{Town}, ");
				}

				if (!string.IsNullOrEmpty(Postcode))
				{
					result.Append(CultureInfo.InvariantCulture, $"{Postcode}, ");
				}

				return result.ToString().TrimEnd(',', ' ');
			}
		}
	}
}
