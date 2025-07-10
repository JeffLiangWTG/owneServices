using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCarrierCodeLanguage")]
[Index("ZCL_ZZ4_CarrierCode", Name = "IX_RefCarrierCodeLanguage_ZCL_ZZ4_CarrierCode")]
public partial class RefCarrierCodeLanguage
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZCL_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZCL_ZX6_NKLanguage { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid ZCL_ZZ4_CarrierCode { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string ZCL_Description { get; set; }

    [ForeignKey("ZCL_ZZ4_CarrierCode")]
    [InverseProperty("RefCarrierCodeLanguages")]
    public virtual RefCarrierCode ZCL_ZZ4_CarrierCodeNavigation { get; set; }
}
