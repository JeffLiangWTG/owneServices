using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefTimeZone")]
[Index("R2_R3_TimeZoneSet", Name = "IX_RefTimeZone_R2_R3_TimeZoneSet")]
public partial class RefTimeZone
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid R2_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(80)")]
    [StringLength(80)]
    [Unicode(false)]
    public string R2_CivilianTimeZoneFullName { get; set; }

    [Required]
    [Column(TypeName = "varchar(10)")]
    [StringLength(10)]
    [Unicode(false)]
    public string R2_CivilianTimeZoneCode { get; set; }

    [Required]
    [Column(TypeName = "varchar(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string R2_MilitaryTimeZoneCode { get; set; }

    [Column(TypeName = "smallint")]
    public short R2_OffsetMinutesFromUTC { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid R2_R3_TimeZoneSet { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string R2_Type { get; set; }

    [ForeignKey("R2_R3_TimeZoneSet")]
    [InverseProperty("RefTimeZones")]
    public virtual RefTimeZoneSet R2_R3_TimeZoneSetNavigation { get; set; }

    [InverseProperty("R4_R2Navigation")]
    public virtual ICollection<RefTimeZoneRule> RefTimeZoneRules { get; set; } = new List<RefTimeZoneRule>();
}
