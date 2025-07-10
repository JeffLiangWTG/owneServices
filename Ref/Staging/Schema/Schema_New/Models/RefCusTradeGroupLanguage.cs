using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusTradeGroupLanguage")]
[Index("ZXD_ZZA_TradeGroup", Name = "IX_RefCusTradeGroupLanguage_ZXD_ZZA_TradeGroup")]
public partial class RefCusTradeGroupLanguage
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZXD_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZXD_ZX6_NKLanguage { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid ZXD_ZZA_TradeGroup { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string ZXD_Description { get; set; }

    [ForeignKey("ZXD_ZZA_TradeGroup")]
    [InverseProperty("RefCusTradeGroupLanguages")]
    public virtual RefCusTradeGroup ZXD_ZZA_TradeGroupNavigation { get; set; }
}
