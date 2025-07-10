using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefAirlineProductCode")]
public partial class RefAirlineProductCode
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid RAR_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string RAR_AirlineID { get; set; }

    [Required]
    [Column(TypeName = "varchar(20)")]
    [StringLength(20)]
    [Unicode(false)]
    public string RAR_Code { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string RAR_Description { get; set; }

    [InverseProperty("RPC_RARNavigation")]
    public virtual ICollection<RefAirlineProductCodeCommodityCodePivot> RefAirlineProductCodeCommodityCodePivots { get; set; } = new List<RefAirlineProductCodeCommodityCodePivot>();
}
