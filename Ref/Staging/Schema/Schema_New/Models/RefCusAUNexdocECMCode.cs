using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusAUNexdocECMCode")]
public partial class RefCusAUNexdocECMCode
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZY5_PK { get; set; }

    [Required]
    [Column(TypeName = "char(1)")]
    [StringLength(1)]
    [Unicode(false)]
    public string ZY5_CommodityCode { get; set; }

    [Required]
    [Column(TypeName = "varchar(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string ZY5_PreservationCode { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZY5_ProductTypeCode { get; set; }

    [Required]
    [Column(TypeName = "varchar(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string ZY5_PackTypeCode { get; set; }

    [Required]
    [Column(TypeName = "varchar(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string ZY5_SupplementaryCode { get; set; }
}
