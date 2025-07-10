using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Keyless]
public partial class SourceDataUserView
{
    public Guid SDA_PK { get; set; }

    [Required]
    [StringLength(75)]
    [Unicode(false)]
    public string SDA_SubSource { get; set; }

    [Required]
    [StringLength(3)]
    [Unicode(false)]
    public string SDA_Source { get; set; }

    public DateTime? SDA_SourceTime { get; set; }

    [Unicode(false)]
    public string SDA_Filename { get; set; }

    public DateTime SDA_CreatedTime { get; set; }

    [StringLength(3)]
    [Unicode(false)]
    public string SDA_Status { get; set; }

    public DateTime? SDA_NotProcessedUntil { get; set; }
}
