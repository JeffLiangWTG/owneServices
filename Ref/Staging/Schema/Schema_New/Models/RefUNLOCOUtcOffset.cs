using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefUNLOCOUtcOffset")]
public partial class RefUNLOCOUtcOffset
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid RLO_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(5)")]
    [StringLength(5)]
    [Unicode(false)]
    public string RLO_RL_NKCode { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime RLO_StartTimeUtc { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime RLO_EndTimeUtc { get; set; }

    [Column(TypeName = "smallint")]
    public short RLO_OffsetMinutesFromUtc { get; set; }
}
