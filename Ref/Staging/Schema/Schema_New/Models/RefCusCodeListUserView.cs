using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Keyless]
public partial class RefCusCodeListUserView
{
    public Guid ZZD_PK { get; set; }

    [Required]
    [StringLength(3)]
    [Unicode(false)]
    public string ZZD_CountryOrGrouping { get; set; }

    [Required]
    [StringLength(5)]
    [Unicode(false)]
    public string ZZD_CodeType { get; set; }

    [Required]
    [StringLength(35)]
    [Unicode(false)]
    public string ZZD_Code { get; set; }

    [Required]
    [StringLength(2000)]
    public string ZZD_Description { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime ZZD_StartDate { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime ZZD_EndDate { get; set; }

    public bool ZZD_IsAir { get; set; }

    public bool ZZD_IsSea { get; set; }

    public bool ZZD_IsFix { get; set; }

    public bool ZZD_IsRai { get; set; }

    public bool ZZD_IsRoa { get; set; }

    public bool ZZD_IsMai { get; set; }

    public bool ZZD_IsInw { get; set; }

    public bool ZZD_IsSystem { get; set; }

    public bool? ZZD_IsPublished { get; set; }

    public bool ZZD_IsEditable { get; set; }
}
