using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefAccTaxRate")]
public partial class RefAccTaxRate
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZAT_PK { get; set; }

    [Required]
    [Column(TypeName = "char(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string ZAT_RN_NKCountry { get; set; }

    [Required]
    [Column(TypeName = "varchar(10)")]
    [StringLength(10)]
    [Unicode(false)]
    public string ZAT_ReferenceRateType { get; set; }

    [Column(TypeName = "date")]
    public DateTime ZAT_StartDate { get; set; }

    [Column(TypeName = "date")]
    public DateTime ZAT_EndDate { get; set; }

    [Column(TypeName = "int")]
    public int ZAT_RateNumerator { get; set; }

    [Column(TypeName = "int")]
    public int ZAT_RateDenominator { get; set; }
}
