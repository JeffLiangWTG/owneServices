using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

public partial class RefCountryStates
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid RW_PK { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(35)")]
    [StringLength(35)]
    public string RW_Description { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(35)")]
    [StringLength(35)]
    public string RW_RegionName { get; set; }

    [Column(TypeName = "bit")]
    public bool RW_IsActive { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string RW_Code { get; set; }

    [Required]
    [Column(TypeName = "char(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string RW_RN_NKCountryCode { get; set; }
}
