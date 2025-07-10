using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusProfileAttribute")]
[Index("XXY_XX0_Profile", Name = "IX_RefCusProfileAttribute_XXY_XX0_Profile")]
public partial class RefCusProfileAttribute
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid XXY_PK { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid XXY_XX0_Profile { get; set; }

    [Required]
    [Column(TypeName = "varchar(35)")]
    [StringLength(35)]
    [Unicode(false)]
    public string XXY_Name { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string XXY_Value { get; set; }

    [ForeignKey("XXY_XX0_Profile")]
    [InverseProperty("RefCusProfileAttributes")]
    public virtual RefCusProfile XXY_XX0_ProfileNavigation { get; set; }
}
