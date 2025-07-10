using System.Collections.Generic;

namespace CargoWise.RefDbRepo.GBReferenceData.Business.UKOfficeCodes
{
	public class UKOfficeCode
	{
		public string Code { get; set; }
		public string UsualName { get; set; }
		public string City { get; set; }
		public string Region { get; set; }
		public IEnumerable<UKOfficeCodeRole> Roles { get; set; }

		public string Description => $"{UsualName ?? string.Empty} {City ?? string.Empty}".Trim();
		public bool IsValid => !string.IsNullOrWhiteSpace(Code) && !string.IsNullOrWhiteSpace(Description);
	}
}
