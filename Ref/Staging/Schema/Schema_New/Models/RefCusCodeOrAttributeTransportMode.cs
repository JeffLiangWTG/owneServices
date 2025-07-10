using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusCodeOrAttributeTransportMode")]
[Index("ZZU_ZZD_CodeList", Name = "IX_RefCusCodeOrAttributeTransportMode_ZZU_ZZD_CodeList")]
[Index("ZZU_ZZE_Attribute", Name = "IX_RefCusCodeOrAttributeTransportMode_ZZU_ZZE_Attribute")]
public partial class RefCusCodeOrAttributeTransportMode
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZZU_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZZU_TransportMode { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid? ZZU_ZZD_CodeList { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid? ZZU_ZZE_Attribute { get; set; }

    [ForeignKey("ZZU_ZZD_CodeList")]
    [InverseProperty("RefCusCodeOrAttributeTransportModes")]
    public virtual RefCusCodeList ZZU_ZZD_CodeListNavigation { get; set; }

    [ForeignKey("ZZU_ZZE_Attribute")]
    [InverseProperty("RefCusCodeOrAttributeTransportModes")]
    public virtual RefCusCodeListAttribute ZZU_ZZE_AttributeNavigation { get; set; }
}
