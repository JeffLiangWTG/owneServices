using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusMap")]
public partial class RefCusMap
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZZM_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(5)")]
    [StringLength(5)]
    [Unicode(false)]
    public string ZZM_ZZP_NKMapType { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(35)")]
    [StringLength(35)]
    public string ZZM_CW1orCommercialValue { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(35)")]
    [StringLength(35)]
    public string ZZM_CustomsValue { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime ZZM_StartDate { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime ZZM_EndDate { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZZM_ZZZ_NKDataGrouping { get; set; }
}
