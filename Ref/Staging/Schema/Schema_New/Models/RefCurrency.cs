using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCurrency")]
public partial class RefCurrency
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid RX_PK { get; set; }

    [Required]
    [Column(TypeName = "char(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string RX_Code { get; set; }

    [Required]
    [Column(TypeName = "bit")]
    public bool? RX_IsActive { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(3)")]
    [StringLength(3)]
    public string RX_Symbol { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(35)")]
    [StringLength(35)]
    public string RX_Desc { get; set; }

    [Required]
    [Column(TypeName = "varchar(20)")]
    [StringLength(20)]
    [Unicode(false)]
    public string RX_UnitName { get; set; }

    [Required]
    [Column(TypeName = "varchar(20)")]
    [StringLength(20)]
    [Unicode(false)]
    public string RX_SubUnitName { get; set; }

    [Column(TypeName = "int")]
    public int RX_SubUnitRatio { get; set; }

    [Column(TypeName = "int")]
    public int RX_ISOSubUnitRatio { get; set; }
}
