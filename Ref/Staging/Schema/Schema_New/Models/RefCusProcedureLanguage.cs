using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusProcedureLanguage")]
[Index("ZXV_ZZ6_Procedure", Name = "IX_RefCusProcedureLanguage_ZXV_ZZ6_Procedure")]
public partial class RefCusProcedureLanguage
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZXV_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZXV_ZX6_NKLanguage { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid ZXV_ZZ6_Procedure { get; set; }

    [Column(TypeName = "nvarchar(500)")]
    [StringLength(500)]
    public string ZXV_Description { get; set; }

    [ForeignKey("ZXV_ZZ6_Procedure")]
    [InverseProperty("RefCusProcedureLanguages")]
    public virtual RefCusProcedure ZXV_ZZ6_ProcedureNavigation { get; set; }
}
