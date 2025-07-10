using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusTaxOrFeeLanguage")]
[Index("ZXU_ZZF_TaxOrFee", Name = "IX_RefCusTaxOrFeeLanguage_ZXU_ZZF_TaxOrFee")]
public partial class RefCusTaxOrFeeLanguage
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZXU_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZXU_ZX6_NKLanguage { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid ZXU_ZZF_TaxOrFee { get; set; }

    [Column(TypeName = "nvarchar(500)")]
    [StringLength(500)]
    public string ZXU_Description { get; set; }

    [ForeignKey("ZXU_ZZF_TaxOrFee")]
    [InverseProperty("RefCusTaxOrFeeLanguages")]
    public virtual RefCusTaxOrFee ZXU_ZZF_TaxOrFeeNavigation { get; set; }
}
