using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("UNDGAttributeZZ")]
[Index("DAZ_ParentPK", Name = "IX_UNDGAttributeZZ_DAZ_ParentPK")]
public partial class UNDGAttributeZZ
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid DAZ_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(7)")]
    [StringLength(7)]
    [Unicode(false)]
    public string DAZ_Language { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string DAZ_Type { get; set; }

    [Required]
    [Column(TypeName = "varchar(6)")]
    [StringLength(6)]
    [Unicode(false)]
    public string DAZ_Index { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string DAZ_Descriptor { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string DAZ_ParentCode { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid DAZ_ParentPK { get; set; }
}
