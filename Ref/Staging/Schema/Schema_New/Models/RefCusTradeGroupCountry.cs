using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusTradeGroupCountry")]
[Index("ZZB_ZZA_TradeGroup", Name = "IX_RefCusTradeGroupCountry_ZZB_ZZA_TradeGroup")]
public partial class RefCusTradeGroupCountry
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZZB_PK { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid ZZB_ZZA_TradeGroup { get; set; }

    [Required]
    [Column(TypeName = "char(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string ZZB_RN_NKTradeGroupCountryCode { get; set; }

    [Column(TypeName = "date")]
    public DateTime ZZB_StartDate { get; set; }

    [Column(TypeName = "date")]
    public DateTime ZZB_EndDate { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(200)")]
    [StringLength(200)]
    public string ZZB_Description { get; set; }

    [ForeignKey("ZZB_ZZA_TradeGroup")]
    [InverseProperty("RefCusTradeGroupCountries")]
    public virtual RefCusTradeGroup ZZB_ZZA_TradeGroupNavigation { get; set; }
}
