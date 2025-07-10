using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("DataSourceInformation")]
[Index("DSI_SubSource", Name = "UX_DataSourceInformation_DSI_SubSource", IsUnique = true)]
public partial class DataSourceInformation
{
    [Key]
    public Guid DSI_PK { get; set; }

    [Required]
    [StringLength(75)]
    [Unicode(false)]
    public string DSI_SubSource { get; set; }

    public bool DSI_EnableAutoExpiration { get; set; }
}
