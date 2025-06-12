using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace eServices.Dms.Core.StorageRepository;

[Index("ST_Key", IsUnique = true)]
public partial class DmsStorageTable
{
	[Key]
	public int ST_ID { get; set; }

	[StringLength(400)]
	[Display()]
	public string ST_Key { get; set; } = null!;

	public string? ST_Document { get; set; }

	public byte[]? ST_Object { get; set; }

	public DateTime ST_LastUpdateTime { get; set; }

	[StringLength(128)]
	public string ST_LastUpdateUser { get; set; } = null!;
}
