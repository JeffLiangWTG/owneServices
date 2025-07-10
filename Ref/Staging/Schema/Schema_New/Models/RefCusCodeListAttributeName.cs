using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusCodeListAttributeName")]
public partial class RefCusCodeListAttributeName
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZXE_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(32)")]
    [StringLength(32)]
    [Unicode(false)]
    public string ZXE_Name { get; set; }

    [Required]
    [Column(TypeName = "varchar(500)")]
    [StringLength(500)]
    [Unicode(false)]
    public string ZXE_Description { get; set; }

    [Required]
    [Column(TypeName = "varchar(5)")]
    [StringLength(5)]
    [Unicode(false)]
    public string ZXE_ZZK_NKCodeType { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZXE_ZZZ_NKDataGrouping { get; set; }

    [Column(TypeName = "bit")]
    public bool ZXE_IsMandatory { get; set; }

    [Column(TypeName = "bit")]
    public bool ZXE_AllowDuplicates { get; set; }

    [Column(TypeName = "bit")]
    public bool ZXE_IsValueMandatory { get; set; }

    [Column(TypeName = "varchar(5)")]
    [StringLength(5)]
    [Unicode(false)]
    public string ZXE_ZZK_NKCodeTypeForValueList { get; set; }

    [Required]
    [Column(TypeName = "varchar(7)")]
    [StringLength(7)]
    [Unicode(false)]
    public string ZXE_ValueDataType { get; set; }

    [Column(TypeName = "smallint")]
    public short ZXE_MinLengthOrValue { get; set; }

    [Column(TypeName = "decimal(19, 5)")]
    public decimal ZXE_MaxLengthOrValue { get; set; }

    [Column(TypeName = "tinyint")]
    public byte ZXE_DecimalPlaces { get; set; }

    [Required]
    [Column(TypeName = "varchar(35)")]
    [StringLength(35)]
    [Unicode(false)]
    public string ZXE_ColumnCaption { get; set; }

    [Column(TypeName = "bit")]
    public bool ZXE_IsDateRangeUsed { get; set; }

    [InverseProperty("ZXH_ZXE_CodeListAttributeNameNavigation")]
    public virtual ICollection<RefCusCodeListAttributeNameLanguage> RefCusCodeListAttributeNameLanguages { get; set; } = new List<RefCusCodeListAttributeNameLanguage>();
}
