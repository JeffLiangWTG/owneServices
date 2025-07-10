using System.ComponentModel.DataAnnotations.Schema;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

public partial class QRTZ_JOB_DETAILS
{
	[NotMapped]
	public string CountryCode { get; set; }

	[NotMapped]
	public string ProgramArgs { get; set; }

	[NotMapped]
	public string ProgramExePath { get; set; }
}
