using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefFacilityLocalCode")]
public partial class RefFacilityLocalCode
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid RFL_PK { get; set; }

    [Required]
    [Column(TypeName = "char(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string RFL_Usage { get; set; }

    [Required]
    [Column(TypeName = "char(11)")]
    [StringLength(11)]
    [Unicode(false)]
    public string RFL_RFT_NKFacilityCode { get; set; }

    [Required]
    [Column(TypeName = "varchar(20)")]
    [StringLength(20)]
    [Unicode(false)]
    public string RFL_Code { get; set; }

    [Required]
    [Column(TypeName = "char(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string RFL_RN_NKCountryCode { get; set; }
}
