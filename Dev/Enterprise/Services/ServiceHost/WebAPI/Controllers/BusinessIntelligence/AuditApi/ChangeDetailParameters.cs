using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Enterprise.Services.ServiceHost.WebAPI.Controllers.BusinessIntelligence.Audit
{
	public class ChangeDetailParameters : AuditApiParameters
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Regex")]
		internal const string LsnRegex = "(:?0x)?[0-9a-fA-F]{20}";

		internal const string AfterLsnRegexErrorMessage = "The After_Lsn parameter must be a valid LSN (22 characters long starting with '0x').";
		internal const string MaxLsnRegexErrorMessage = "The Max_Lsn parameter must be a valid LSN (22 characters long starting with '0x').";
		internal const string SeqValRegexErrorMessage = "The After_Seqval parameter must be a valid LSN (22 characters long starting with '0x').";

		[Required]
		public string SchemaName { get; set; }

		[Required]
		public string TableName { get; set; }

		[Required]
		[RegularExpression(LsnRegex, ErrorMessage = AfterLsnRegexErrorMessage)]
		public string After_Lsn { get; set; }

		[RegularExpression(LsnRegex, ErrorMessage = MaxLsnRegexErrorMessage)]
		public string Max_Lsn { get; set; }

		[RegularExpression(LsnRegex, ErrorMessage = SeqValRegexErrorMessage)]
		public string After_Seqval { get; set; }

		public int? After_Command_Id { get; set; }

		public int? After_Operation { get; set; }

		public int? Page_Size { get; set; }
	}
}
