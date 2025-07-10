using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefAirlineCommodityCode")]
public partial class RefAirlineCommodityCode
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid RAC_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string RAC_AirlineID { get; set; }

    [Required]
    [Column(TypeName = "varchar(20)")]
    [StringLength(20)]
    [Unicode(false)]
    public string RAC_Code { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string RAC_Description { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string RAC_SpecialHandlingCodes { get; set; }
}
