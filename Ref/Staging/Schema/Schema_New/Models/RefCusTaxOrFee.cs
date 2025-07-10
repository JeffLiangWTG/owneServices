using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusTaxOrFee")]
public partial class RefCusTaxOrFee
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZZF_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(4)")]
    [StringLength(4)]
    [Unicode(false)]
    public string ZZF_Code { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(500)")]
    [StringLength(500)]
    public string ZZF_Description { get; set; }

    [Column(TypeName = "decimal(19, 8)")]
    public decimal ZZF_Value { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime ZZF_StartDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime ZZF_EndDate { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZZF_ZZZ_NKDataGrouping { get; set; }

    [Column(TypeName = "decimal(19, 8)")]
    public decimal ZZF_Minimum { get; set; }

    [Column(TypeName = "decimal(19, 8)")]
    public decimal ZZF_Maximum { get; set; }

    [Column(TypeName = "decimal(19, 8)")]
    public decimal ZZF_Threshold { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZZF_ZX0_NKTaxOrFeeType { get; set; }

    [InverseProperty("ZXU_ZZF_TaxOrFeeNavigation")]
    public virtual ICollection<RefCusTaxOrFeeLanguage> RefCusTaxOrFeeLanguages { get; set; } = new List<RefCusTaxOrFeeLanguage>();
}
