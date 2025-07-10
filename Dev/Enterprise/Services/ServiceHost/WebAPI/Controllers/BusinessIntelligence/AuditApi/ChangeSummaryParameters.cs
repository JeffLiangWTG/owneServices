using System.ComponentModel.DataAnnotations;

namespace Enterprise.Services.ServiceHost.WebAPI.Controllers.BusinessIntelligence.Audit
{
	public class ChangeSummaryParameters : AuditApiParameters
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Regex")]
		internal const string LsnRegex = "(:?0x)?[0-9a-fA-F]{20}";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Error message")]
		internal const string LsnRegexErrorMessage = "The After_Lsn parameter must be a valid LSN (22 characters long starting with '0x').";

		[Required]
		[RegularExpression(LsnRegex, ErrorMessage = LsnRegexErrorMessage)]
		public string After_Lsn { get; set; }
	}
}
