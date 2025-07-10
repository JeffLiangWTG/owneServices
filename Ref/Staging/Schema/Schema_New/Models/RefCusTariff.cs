using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusTariff")]
public partial class RefCusTariff
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZZ1_PK { get; set; }

    [Column(TypeName = "varchar(5)")]
    [StringLength(5)]
    [Unicode(false)]
    public string ZZ1_ZZI_NKTariffType { get; set; }

    [Required]
    [Column(TypeName = "varchar(35)")]
    [StringLength(35)]
    [Unicode(false)]
    public string ZZ1_TariffCode { get; set; }

    [Column(TypeName = "smallint")]
    public short ZZ1_IAMUnique { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(4000)")]
    [StringLength(4000)]
    public string ZZ1_Description { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime ZZ1_StartDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime ZZ1_EndDate { get; set; }

    [Required]
    [Column(TypeName = "varchar(4)")]
    [StringLength(4)]
    [Unicode(false)]
    public string ZZ1_ZZF_NKTaxOrFeeCode { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZZ1_ZZZ_NKDataGrouping { get; set; }

    [Required]
    [Column(TypeName = "varchar(100)")]
    [StringLength(100)]
    [Unicode(false)]
    public string ZZ1_CompositeKeyOnZZ5 { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZZ1_ZZI_ZZZ_NKDataGrouping { get; set; }

    [InverseProperty("ZX1_ZZ1_TariffNavigation")]
    public virtual ICollection<RefCusCondition> RefCusConditions { get; set; } = new List<RefCusCondition>();

    [InverseProperty("ZZ2_ZZ1_TariffNavigation")]
    public virtual ICollection<RefCusRate> RefCusRates { get; set; } = new List<RefCusRate>();

    [InverseProperty("ZY2_ZZ1_TariffNavigation")]
    public virtual ICollection<RefCusTariffAdditionalCode> RefCusTariffAdditionalCodes { get; set; } = new List<RefCusTariffAdditionalCode>();

    [InverseProperty("ZZ3_ZZ1_TariffNavigation")]
    public virtual ICollection<RefCusTariffAttribute> RefCusTariffAttributes { get; set; } = new List<RefCusTariffAttribute>();

    [InverseProperty("ZB1_ZZ1_TariffNavigation")]
    public virtual ICollection<RefCusTariffBRCharacteristic> RefCusTariffBRCharacteristics { get; set; } = new List<RefCusTariffBRCharacteristic>();

    [InverseProperty("ZX7_ZZ1_TariffNavigation")]
    public virtual ICollection<RefCusTariffLanguage> RefCusTariffLanguages { get; set; } = new List<RefCusTariffLanguage>();

    [InverseProperty("ZZW_ZZ1_TariffNavigation")]
    public virtual ICollection<RefCusTariffNationalCode> RefCusTariffNationalCodes { get; set; } = new List<RefCusTariffNationalCode>();

    [InverseProperty("ZZH_ZZ1_TariffNavigation")]
    public virtual ICollection<RefCusTariffRelationship> RefCusTariffRelationships { get; set; } = new List<RefCusTariffRelationship>();

    [InverseProperty("ZZ8_ZZ1_TariffNavigation")]
    public virtual ICollection<RefCusTariffUOM> RefCusTariffUOMs { get; set; } = new List<RefCusTariffUOM>();

    [InverseProperty("ZX5_ZZ1_TariffNavigation")]
    public virtual ICollection<RefCusVATApplicability> RefCusVATApplicabilities { get; set; } = new List<RefCusVATApplicability>();
}
