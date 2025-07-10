using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusNomenclatureGroup")]
public partial class RefCusNomenclatureGroup
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZZ5_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZZ5_ZZ9_NKNomenclatureGroupType { get; set; }

    [Required]
    [Column(TypeName = "varchar(15)")]
    [StringLength(15)]
    [Unicode(false)]
    public string ZZ5_Value { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string ZZ5_Description { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime ZZ5_StartDate { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime ZZ5_EndDate { get; set; }

    [Required]
    [Column(TypeName = "varchar(100)")]
    [StringLength(100)]
    [Unicode(false)]
    public string ZZ5_CompositeKey { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZZ5_ZZZ_NKDataGrouping { get; set; }

    [InverseProperty("ZX1_ZZ5_NomenclatureNavigation")]
    public virtual ICollection<RefCusCondition> RefCusConditions { get; set; } = new List<RefCusCondition>();

    [InverseProperty("ZZL_ZZ5_NomenclatureGroupNavigation")]
    public virtual ICollection<RefCusNomenclatureGroupNote> RefCusNomenclatureGroupNotes { get; set; } = new List<RefCusNomenclatureGroupNote>();

    [InverseProperty("ZX8_ZZ5_NomenclatureGroupNavigation")]
    public virtual ICollection<RefCusNomenclatureLanguage> RefCusNomenclatureLanguages { get; set; } = new List<RefCusNomenclatureLanguage>();

    [InverseProperty("ZB1_ZZ5_NomenclatureNavigation")]
    public virtual ICollection<RefCusTariffBRCharacteristic> RefCusTariffBRCharacteristics { get; set; } = new List<RefCusTariffBRCharacteristic>();
}
