using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusCodeList")]
public partial class RefCusCodeList
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZZD_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(5)")]
    [StringLength(5)]
    [Unicode(false)]
    public string ZZD_ZZK_NKCodeType { get; set; }

    [Required]
    [Column(TypeName = "varchar(35)")]
    [StringLength(35)]
    [Unicode(false)]
    public string ZZD_Code { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(2000)")]
    [StringLength(2000)]
    public string ZZD_Description { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime ZZD_StartDate { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime ZZD_EndDate { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZZD_ZZZ_NKDataGrouping { get; set; }

    [InverseProperty("ZZE_ZZD_CodeListNavigation")]
    public virtual ICollection<RefCusCodeListAttribute> RefCusCodeListAttributes { get; set; } = new List<RefCusCodeListAttribute>();

    [InverseProperty("ZXA_ZZD_CodeListNavigation")]
    public virtual ICollection<RefCusCodeListLanguage> RefCusCodeListLanguages { get; set; } = new List<RefCusCodeListLanguage>();

    [InverseProperty("ZZU_ZZD_CodeListNavigation")]
    public virtual ICollection<RefCusCodeOrAttributeTransportMode> RefCusCodeOrAttributeTransportModes { get; set; } = new List<RefCusCodeOrAttributeTransportMode>();
}
