using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Keyless]
public partial class RefAccTaxRateUserView
{
    public Guid ZAT_PK { get; set; }

    [Required]
    [StringLength(2)]
    [Unicode(false)]
    public string ZAT_RN_NKCountry { get; set; }

    [Required]
    [StringLength(10)]
    [Unicode(false)]
    public string ZAT_ReferenceRateType { get; set; }

    [Column(TypeName = "date")]
    public DateTime ZAT_StartDate { get; set; }

    [Column(TypeName = "date")]
    public DateTime ZAT_EndDate { get; set; }

    public int ZAT_RateNumerator { get; set; }

    public int ZAT_RateDenominator { get; set; }

    public bool? ZAT_IsSystem { get; set; }

    public bool? ZAT_IsPublished { get; set; }

    public bool ZAT_IsEditable { get; set; }
}
