using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace eServices.Dms.Core.StorageRepository;

[PrimaryKey("SC_Schema", "SC_Name")]
public partial class DmsStorageCatalog
{
	[Key]
	[StringLength(128)]
	public string SC_Schema { get; set; } = null!;

	[Key]
	[StringLength(128)]
	public string SC_Name { get; set; } = null!;

	[StringLength(50)]
	[Unicode(false)]
	public string SC_Type { get; set; } = null!;

	[StringLength(50)]
	[Unicode(false)]
	public string SC_Version { get; set; } = null!;

	[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
	public int SC_VersionMajor { get; set; }

	[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
	public int SC_VersionMinor { get; set; }

	[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
	[StringLength(50)]
	[Unicode(false)]
	public string? SC_VersionLabel { get; set; }

	public int? SC_ExpirationDays { get; set; }

	public DateTime SC_CreateTime { get; set; }

	[StringLength(250)]
	public string SC_CreateUser { get; set; } = null!;

	public DateTime? SC_DeleteTime { get; set; }

	[StringLength(250)]
	public string? SC_DeleteUser { get; set; }
}
