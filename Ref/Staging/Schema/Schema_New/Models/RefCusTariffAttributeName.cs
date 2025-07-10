using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusTariffAttributeName")]
public partial class RefCusTariffAttributeName
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZY6_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(32)")]
    [StringLength(32)]
    [Unicode(false)]
    public string ZY6_Name { get; set; }

    [Required]
    [Column(TypeName = "varchar(500)")]
    [StringLength(500)]
    [Unicode(false)]
    public string ZY6_Description { get; set; }

    [Required]
    [Column(TypeName = "varchar(5)")]
    [StringLength(5)]
    [Unicode(false)]
    public string ZY6_ZZI_NKTariffType { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZY6_ZZZ_NKDataGrouping { get; set; }

    [Required]
    [Column(TypeName = "varchar(35)")]
    [StringLength(35)]
    [Unicode(false)]
    public string ZY6_ColumnCaption { get; set; }
}
