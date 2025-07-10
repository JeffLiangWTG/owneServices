using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusTradeGroup")]
public partial class RefCusTradeGroup
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZZA_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(35)")]
    [StringLength(35)]
    [Unicode(false)]
    public string ZZA_TradeGroup { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(4000)")]
    [StringLength(4000)]
    public string ZZA_Description { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime ZZA_StartDate { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime ZZA_EndDate { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZZA_ZZZ_NKDataGrouping { get; set; }

    [InverseProperty("ZZB_ZZA_TradeGroupNavigation")]
    public virtual ICollection<RefCusTradeGroupCountry> RefCusTradeGroupCountries { get; set; } = new List<RefCusTradeGroupCountry>();

    [InverseProperty("ZXD_ZZA_TradeGroupNavigation")]
    public virtual ICollection<RefCusTradeGroupLanguage> RefCusTradeGroupLanguages { get; set; } = new List<RefCusTradeGroupLanguage>();
}
