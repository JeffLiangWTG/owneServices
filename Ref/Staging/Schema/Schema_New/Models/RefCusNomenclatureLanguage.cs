using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusNomenclatureLanguage")]
[Index("ZX8_ZZ5_NomenclatureGroup", Name = "IX_RefCusNomenclatureLanguage_ZX8_ZZ5_NomenclatureGroup")]
public partial class RefCusNomenclatureLanguage
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZX8_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZX8_ZX6_NKLanguage { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid ZX8_ZZ5_NomenclatureGroup { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string ZX8_Description { get; set; }

    [ForeignKey("ZX8_ZZ5_NomenclatureGroup")]
    [InverseProperty("RefCusNomenclatureLanguages")]
    public virtual RefCusNomenclatureGroup ZX8_ZZ5_NomenclatureGroupNavigation { get; set; }
}
