using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefAirlineProductCodeCommodityCodePivot")]
[Index("RPC_RAR", Name = "IX_RefAirlineProductCodeCommodityCodePivot_RPC_RAR")]
public partial class RefAirlineProductCodeCommodityCodePivot
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid RPC_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string RPC_AirlineID { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid RPC_RAR { get; set; }

    [Required]
    [Column(TypeName = "varchar(20)")]
    [StringLength(20)]
    [Unicode(false)]
    public string RPC_RAC_NKCode { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string RPC_RAC_NKAirlineID { get; set; }

    [ForeignKey("RPC_RAR")]
    [InverseProperty("RefAirlineProductCodeCommodityCodePivots")]
    public virtual RefAirlineProductCode RPC_RARNavigation { get; set; }
}
