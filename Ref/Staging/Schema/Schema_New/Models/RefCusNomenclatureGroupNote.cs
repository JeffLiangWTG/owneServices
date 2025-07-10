using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusNomenclatureGroupNote")]
[Index("ZZL_ZZ5_NomenclatureGroup", Name = "IX_RefCusNomenclatureGroupNote_ZZL_ZZ5_NomenclatureGroup")]
public partial class RefCusNomenclatureGroupNote
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZZL_PK { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid ZZL_ZZ5_NomenclatureGroup { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZZL_ZZZ_NKDataGrouping { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZZL_ZX6_NKLanguage { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZZL_NoteType { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string ZZL_Note { get; set; }

    [ForeignKey("ZZL_ZZ5_NomenclatureGroup")]
    [InverseProperty("RefCusNomenclatureGroupNotes")]
    public virtual RefCusNomenclatureGroup ZZL_ZZ5_NomenclatureGroupNavigation { get; set; }
}
