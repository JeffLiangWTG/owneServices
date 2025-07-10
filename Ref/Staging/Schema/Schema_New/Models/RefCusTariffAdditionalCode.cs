using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusTariffAdditionalCode")]
[Index("ZY2_ZZ1_Tariff", Name = "IX_RefCusTariffAdditionalCode_ZY2_ZZ1_Tariff")]
[Index("ZY2_ZZW_NationalCode", Name = "IX_RefCusTariffAdditionalCode_ZY2_ZZW_NationalCode")]
public partial class RefCusTariffAdditionalCode
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZY2_PK { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid? ZY2_ZZ1_Tariff { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid? ZY2_ZZW_NationalCode { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(15)")]
    [StringLength(15)]
    public string ZY2_AdditionalCode { get; set; }

    [Column(TypeName = "nvarchar(200)")]
    [StringLength(200)]
    public string ZY2_Description { get; set; }

    [Required]
    [Column(TypeName = "char(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZY2_ZY3_NKCategory { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(15)")]
    [StringLength(15)]
    public string ZY2_ParentAdditionalCode { get; set; }

    [Required]
    [Column(TypeName = "char(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZY2_ZY3_NKParentCategory { get; set; }

    [Column(TypeName = "bit")]
    public bool? ZY2_IsMandatory { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZY2_ZZZ_NKDataGrouping { get; set; }

    [InverseProperty("ZZT_ZY2_AdditionalCodeNavigation")]
    public virtual ICollection<RefCusApplicability> RefCusApplicabilities { get; set; } = new List<RefCusApplicability>();

    [InverseProperty("ZY4_ZY2_TariffAdditionalCodeNavigation")]
    public virtual ICollection<RefCusTariffAdditionalCodeLanguage> RefCusTariffAdditionalCodeLanguages { get; set; } = new List<RefCusTariffAdditionalCodeLanguage>();

    [ForeignKey("ZY2_ZZ1_Tariff")]
    [InverseProperty("RefCusTariffAdditionalCodes")]
    public virtual RefCusTariff ZY2_ZZ1_TariffNavigation { get; set; }

    [ForeignKey("ZY2_ZZW_NationalCode")]
    [InverseProperty("RefCusTariffAdditionalCodes")]
    public virtual RefCusTariffNationalCode ZY2_ZZW_NationalCodeNavigation { get; set; }
}
