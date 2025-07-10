using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefTimeZoneSet")]
public partial class RefTimeZoneSet
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid R3_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(35)")]
    [StringLength(35)]
    [Unicode(false)]
    public string R3_TimeZoneSetName { get; set; }

    [Required]
    [Column(TypeName = "bit")]
    public bool? R3_IsActive { get; set; }

    [InverseProperty("R2_R3_TimeZoneSetNavigation")]
    public virtual ICollection<RefTimeZone> RefTimeZones { get; set; } = new List<RefTimeZone>();
}
