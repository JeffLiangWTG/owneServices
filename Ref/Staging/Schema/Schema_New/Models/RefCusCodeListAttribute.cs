using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusCodeListAttribute")]
[Index("ZZE_ZZD_CodeList", Name = "IX_RefCusCodeListAttribute_ZZE_ZZD_CodeList")]
public partial class RefCusCodeListAttribute
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZZE_PK { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid ZZE_ZZD_CodeList { get; set; }

    [Required]
    [Column(TypeName = "varchar(32)")]
    [StringLength(32)]
    [Unicode(false)]
    public string ZZE_ZXE_NKName { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(255)")]
    [StringLength(255)]
    public string ZZE_Value { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime? ZZE_StartDate { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime? ZZE_EndDate { get; set; }

    [InverseProperty("ZZU_ZZE_AttributeNavigation")]
    public virtual ICollection<RefCusCodeOrAttributeTransportMode> RefCusCodeOrAttributeTransportModes { get; set; } = new List<RefCusCodeOrAttributeTransportMode>();

    [ForeignKey("ZZE_ZZD_CodeList")]
    [InverseProperty("RefCusCodeListAttributes")]
    public virtual RefCusCodeList ZZE_ZZD_CodeListNavigation { get; set; }
}
