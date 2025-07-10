using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefExchangeRateZZ")]
public partial class RefExchangeRateZZ
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZZN_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZZN_ExRateType { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime ZZN_StartDate { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime ZZN_EndDate { get; set; }

    [Column(TypeName = "decimal(18, 9)")]
    public decimal ZZN_Rate { get; set; }

    [Required]
    [Column(TypeName = "char(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZZN_RX_NKExCurrency { get; set; }

    [Required]
    [Column(TypeName = "char(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string ZZN_RN_NKCountry { get; set; }

    [Required]
    [Column(TypeName = "varchar(35)")]
    [StringLength(35)]
    [Unicode(false)]
    public string ZZN_AsPublished { get; set; }
}
