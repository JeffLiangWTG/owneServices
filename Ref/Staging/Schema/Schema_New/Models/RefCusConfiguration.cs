using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusConfiguration")]
public partial class RefCusConfiguration
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZZJ_PK { get; set; }

    [Required]
    [Column(TypeName = "char(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string ZZJ_RN_NKCustomsCountry { get; set; }

    [Required]
    [Column(TypeName = "char(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZZJ_TariffDataSource { get; set; }

    [Column(TypeName = "bit")]
    public bool ZZJ_IsGenericCountry { get; set; }

    [Column(TypeName = "bit")]
    public bool ZZJ_AllowRiskManagement { get; set; }

    [Column(TypeName = "bit")]
    public bool ZZJ_IsTransitDeclarationCounty { get; set; }

    [Column(TypeName = "bit")]
    public bool ZZJ_TurnOnASYDCUDAManifest { get; set; }

    [Column(TypeName = "bit")]
    public bool ZZJ_TurnOnASYCUDACustoms { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZZJ_ZZZ_NKDefaultDataGrouping { get; set; }

    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZZJ_ZZZ_NKAlternateTariffOnlyDataGrouping { get; set; }

    [Column(TypeName = "date")]
    public DateTime ZZJ_StartDate { get; set; }

    [Column(TypeName = "date")]
    public DateTime? ZZJ_EndDate { get; set; }
}
