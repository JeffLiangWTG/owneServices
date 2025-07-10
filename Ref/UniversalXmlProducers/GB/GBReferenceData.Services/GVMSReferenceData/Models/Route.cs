using System;
using System.Text;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Common;
using static System.FormattableString;

namespace CargoWise.RefDbRepo.GBReferenceData.Services.GVMSReferenceData.Models
{
	public class Route : IReferenceDataModel
	{
		public string RouteId { get; set; }
		public DateTime RouteEffectiveFrom { get; set; }
		public DateTime RouteEffectiveTo { get; set; } = CommonHelper.DefaultValues.MaximumDateTime;
		public string DeparturePortId { get; set; }
		public string RouteDirection { get; set; }
		public string ArrivalPortId { get; set; }
		public string CarrierId { get; set; }

		public bool IsValid(StringBuilder errorCollector)
		{
			var validationErrors = new StringBuilder();
			var valid = true;

			if (string.IsNullOrEmpty(RouteId))
			{
				validationErrors.Append("RouteId is required. ");
				valid = false;
			}

			if (string.IsNullOrEmpty(ArrivalPortId))
			{
				validationErrors.Append("ArrivalPortId is required. ");
				valid = false;
			}

			if (string.IsNullOrEmpty(DeparturePortId))
			{
				validationErrors.Append("DeparturePortId is required. ");
				valid = false;
			}

			if (string.IsNullOrEmpty(CarrierId))
			{
				validationErrors.Append("CarrierId is required. ");
				valid = false;
			}

			if (!int.TryParse(RouteId, out _))
			{
				validationErrors.Append("RouteId is invalid. ");
				valid = false;
			}

			if (!valid)
			{
				var msg = Invariant($"Route validation error. Key: '{RouteId}' Errors: '{validationErrors}'");
				errorCollector.AppendLine(msg);
			}

			return valid;
		}
	}
}
