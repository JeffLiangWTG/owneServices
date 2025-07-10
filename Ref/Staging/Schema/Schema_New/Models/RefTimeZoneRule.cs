using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefTimeZoneRule")]
[Index("R4_R2", Name = "IX_RefTimeZoneRule_R4_R2")]
public partial class RefTimeZoneRule
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid R4_PK { get; set; }

    [Column(TypeName = "int")]
    public int R4_FromYear { get; set; }

    [Column(TypeName = "int")]
    public int R4_ToYear { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string R4_StartOrEndRule { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string R4_DaylightSavingDayWeekDate { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime? R4_DaylightSavingDate { get; set; }

    [Column(TypeName = "tinyint")]
    public byte R4_DaylightSavingDayCount { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string R4_DaylightSavingDayName { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string R4_DaylightSavingMonth { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string R4_TypeOfTime { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid R4_R2 { get; set; }

    [ForeignKey("R4_R2")]
    [InverseProperty("RefTimeZoneRules")]
    public virtual RefTimeZone R4_R2Navigation { get; set; }
}
