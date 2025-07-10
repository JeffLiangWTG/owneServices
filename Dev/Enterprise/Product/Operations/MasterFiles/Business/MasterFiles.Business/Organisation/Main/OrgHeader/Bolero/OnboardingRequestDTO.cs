using System;
using Newtonsoft.Json;

namespace Enterprise.MasterFiles.Business
{
	public class OnboardingRequestDTO
	{
		public OnboardingRequestDTO(BoleroInvitationDetails boleroInvitationdetails)
		{
			if (boleroInvitationdetails == null)
			{
				throw new ArgumentNullException(nameof(boleroInvitationdetails));
			}

			InvitationDetailsDto = new InviterDetailsDTO(boleroInvitationdetails);
			CompanyInformationDto = new CompanyInformationDTO(boleroInvitationdetails);
			RequestMetaDataDto = new RequestMetaDataDTO();
		}

		[JsonProperty("inviterDetailsDto")]
		public InviterDetailsDTO InvitationDetailsDto { get; set; }

		[JsonProperty("companyInformationDto")]
		public CompanyInformationDTO CompanyInformationDto { get; set; }

		[JsonProperty("requestMetaDataDto")]
		public RequestMetaDataDTO RequestMetaDataDto { get; set; }
	}
}
