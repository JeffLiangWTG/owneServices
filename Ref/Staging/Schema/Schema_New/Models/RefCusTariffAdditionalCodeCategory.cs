using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusTariffAdditionalCodeCategory")]
public partial class RefCusTariffAdditionalCodeCategory
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZY3_PK { get; set; }

    [Required]
    [Column(TypeName = "char(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZY3_Category { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(200)")]
    [StringLength(200)]
    public string ZY3_Description { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZY3_ZZZ_NKDataGrouping { get; set; }
}
