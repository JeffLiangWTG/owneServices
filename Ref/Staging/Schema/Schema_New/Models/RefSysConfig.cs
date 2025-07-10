using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefSysConfig")]
[Index("ZRC_ZRT_NKConfigCode", Name = "IX_RefSysConfig_ZRC_ZRT_NKConfigCode")]
public partial class RefSysConfig
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZRC_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(10)")]
    [StringLength(10)]
    [Unicode(false)]
    public string ZRC_ZRT_NKConfigCode { get; set; }

    [Column(TypeName = "decimal(19, 8)")]
    public decimal ZRC_DecimalValue { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(4000)")]
    [StringLength(4000)]
    public string ZRC_StringValue { get; set; }

    [Column(TypeName = "bit")]
    public bool ZRC_BitValue { get; set; }

    [Column(TypeName = "varbinary(max)")]
    public byte[] ZRC_BinaryValue { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime ZRC_StartDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ZRC_EndDate { get; set; }

    public virtual RefSysConfigType ZRC_ZRT_NKConfigCodeNavigation { get; set; }
}
