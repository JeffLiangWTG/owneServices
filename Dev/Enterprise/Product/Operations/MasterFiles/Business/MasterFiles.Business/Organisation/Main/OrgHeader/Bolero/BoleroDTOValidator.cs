using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public static class BoleroDTOValidator
	{
		public static bool ValidateBoleroDTOFields(OnboardingRequestDTO dto, out List<string> errorMessages)
		{
			errorMessages = new List<string>();
			var validationResults = new List<ValidationResult>();
			var missingFieldStr = (NoResString)"Missing Field";
			var missingFieldsTip = Res.GetString("E64711FC-1CA8-48E3-82E1-D39F8CCA3D72", "are required to Enroll for Electronic Bills of Lading.");

			Validator.TryValidateObject(dto.CompanyInformationDto, new ValidationContext(dto.CompanyInformationDto), validationResults, true);
			Validator.TryValidateObject(dto.InvitationDetailsDto, new ValidationContext(dto.InvitationDetailsDto), validationResults, true);

			var missingFields = validationResults.Where(result => result.ErrorMessage.Equals(missingFieldStr)).Select(result => result.MemberNames.First());
			if (missingFields.Any())
			{
				errorMessages.Insert(0, $"{string.Join(", ", missingFields)} {missingFieldsTip}");
			}

			validationResults.RemoveAll(result => result.ErrorMessage.Equals(missingFieldStr));
			errorMessages.AddRange(validationResults.Select(result => result.ErrorMessage));
			return !errorMessages.Any();
		}
	}
}
