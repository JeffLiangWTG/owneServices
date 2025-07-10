using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusCondition")]
[Index("ZX1_ZZ1_Tariff", Name = "IX_RefCusCondition_ZX1_ZZ1_Tariff")]
[Index("ZX1_ZZ5_Nomenclature", Name = "IX_RefCusCondition_ZX1_ZZ5_Nomenclature")]
public partial class RefCusCondition
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZX1_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(6)")]
    [StringLength(6)]
    [Unicode(false)]
    public string ZX1_ZX2_NKConditionType { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZX1_ZX2_ZZZ_NKDataGrouping { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid? ZX1_ZZ1_Tariff { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid? ZX1_ZZ5_Nomenclature { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime ZX1_StartDate { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime ZX1_EndDate { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(4000)")]
    [StringLength(4000)]
    public string ZX1_Source { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(4000)")]
    [StringLength(4000)]
    public string ZX1_Comment { get; set; }

    [Column(TypeName = "bit")]
    public bool ZX1_IsImport { get; set; }

    [Column(TypeName = "bit")]
    public bool ZX1_IsExport { get; set; }

    [Column(TypeName = "bit")]
    public bool ZX1_ConditionValueTrueMeansStop { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZX1_ZZZ_NKDataGrouping { get; set; }

    [Column(TypeName = "varchar(10)")]
    [StringLength(10)]
    [Unicode(false)]
    public string ZX1_ZZS_NKPreference { get; set; }

    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZX1_ZZS_ZZZ_NKDataGrouping { get; set; }

    [Column(TypeName = "tinyint")]
    public byte ZX1_LogicalANDWithinGroup { get; set; }

    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZX1_ZY7_NKConditionCode { get; set; }

    [Required]
    [Column(TypeName = "varchar(4000)")]
    [StringLength(4000)]
    [Unicode(false)]
    public string ZX1_AdditionalComment { get; set; }

    [Column(TypeName = "char(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZX1_Severity { get; set; }

    [InverseProperty("ZZT_ZX1_ConditionsNavigation")]
    public virtual ICollection<RefCusApplicability> RefCusApplicabilities { get; set; } = new List<RefCusApplicability>();

    [InverseProperty("ZXJ_ZX1_ConditionNavigation")]
    public virtual ICollection<RefCusConditionLanguage> RefCusConditionLanguages { get; set; } = new List<RefCusConditionLanguage>();

    [InverseProperty("ZX3_ZX1_ConditionNavigation")]
    public virtual ICollection<RefCusConditionValue> RefCusConditionValues { get; set; } = new List<RefCusConditionValue>();

    [ForeignKey("ZX1_ZZ1_Tariff")]
    [InverseProperty("RefCusConditions")]
    public virtual RefCusTariff ZX1_ZZ1_TariffNavigation { get; set; }

    [ForeignKey("ZX1_ZZ5_Nomenclature")]
    [InverseProperty("RefCusConditions")]
    public virtual RefCusNomenclatureGroup ZX1_ZZ5_NomenclatureNavigation { get; set; }
}
