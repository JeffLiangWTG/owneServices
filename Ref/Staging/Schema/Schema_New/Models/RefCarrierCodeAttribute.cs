using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCarrierCodeAttribute")]
[Index("ZZG_ZZ4_CarrierCode", Name = "IX_RefCarrierCodeAttribute_ZZG_ZZ4_CarrierCode")]
public partial class RefCarrierCodeAttribute
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZZG_PK { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid ZZG_ZZ4_CarrierCode { get; set; }

    [Required]
    [Column(TypeName = "varchar(32)")]
    [StringLength(32)]
    [Unicode(false)]
    public string ZZG_Name { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(100)")]
    [StringLength(100)]
    public string ZZG_Value { get; set; }

    [ForeignKey("ZZG_ZZ4_CarrierCode")]
    [InverseProperty("RefCarrierCodeAttributes")]
    public virtual RefCarrierCode ZZG_ZZ4_CarrierCodeNavigation { get; set; }
}
