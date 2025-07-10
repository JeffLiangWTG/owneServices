using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusTariffType")]
public partial class RefCusTariffType
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZZI_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(5)")]
    [StringLength(5)]
    [Unicode(false)]
    public string ZZI_TariffType { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(100)")]
    [StringLength(100)]
    public string ZZI_Description { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZZI_ZZZ_NKDataGrouping { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid? ZZI_ZZR_RateType { get; set; }

    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZZI_ZZR_NKRateType { get; set; }

    [Column(TypeName = "bit")]
    public bool ZZI_HasFormulaSpecificQuestions { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZZI_ZZ9_NKNomenclatureGroupType { get; set; }

    [InverseProperty("ZXK_ZZI_TariffTypeNavigation")]
    public virtual ICollection<RefCusTariffTypeLanguage> RefCusTariffTypeLanguages { get; set; } = new List<RefCusTariffTypeLanguage>();

    [ForeignKey("ZZI_ZZR_RateType")]
    [InverseProperty("RefCusTariffTypes")]
    public virtual RefCusRateType ZZI_ZZR_RateTypeNavigation { get; set; }
}
