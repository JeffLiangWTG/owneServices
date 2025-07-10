using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("UNDGAttribute")]
[Index("DA_DG", Name = "IX_UNDGAttribute_DA_DG")]
public partial class UNDGAttribute
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid DA_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string DA_Language { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string DA_Type { get; set; }

    [Required]
    [Column(TypeName = "varchar(6)")]
    [StringLength(6)]
    [Unicode(false)]
    public string DA_Index { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string DA_Descriptor { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid DA_DG { get; set; }

    [ForeignKey("DA_DG")]
    [InverseProperty("UNDGAttributes")]
    public virtual UNDGSubstance DA_DGNavigation { get; set; }
}
