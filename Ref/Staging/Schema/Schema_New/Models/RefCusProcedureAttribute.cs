using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusProcedureAttribute")]
[Index("ZXB_ZZ6_ProcedureCode", "ZXB_Name", Name = "IX_RefCusProcedureAttribute_ZXB_ZZ6_ProcedureCode_ZXB_Name")]
public partial class RefCusProcedureAttribute
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZXB_PK { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid ZXB_ZZ6_ProcedureCode { get; set; }

    [Required]
    [Column(TypeName = "varchar(50)")]
    [StringLength(50)]
    [Unicode(false)]
    public string ZXB_Name { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string ZXB_Value { get; set; }

    [ForeignKey("ZXB_ZZ6_ProcedureCode")]
    [InverseProperty("RefCusProcedureAttributes")]
    public virtual RefCusProcedure ZXB_ZZ6_ProcedureCodeNavigation { get; set; }
}
