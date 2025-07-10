using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Keyless]
public partial class RefCusCodeListAttributeUserView
{
    public Guid ZZE_PK { get; set; }

    public Guid ZZE_ZZD_CodeList { get; set; }

    [Required]
    [StringLength(32)]
    [Unicode(false)]
    public string ZZE_ZXE_NKName { get; set; }

    [Required]
    [StringLength(255)]
    public string ZZE_Value { get; set; }

    public bool ZZE_IsAir { get; set; }

    public bool ZZE_IsSea { get; set; }

    public bool ZZE_IsFix { get; set; }

    public bool ZZE_IsRai { get; set; }

    public bool ZZE_IsRoa { get; set; }

    public bool ZZE_IsMai { get; set; }

    public bool ZZE_IsInw { get; set; }

    [Required]
    [StringLength(5)]
    [Unicode(false)]
    public string ZZE_CodeType { get; set; }

    [Required]
    [StringLength(3)]
    [Unicode(false)]
    public string ZZE_CountryOrGrouping { get; set; }

    public bool ZZE_IsEditable { get; set; }
}
