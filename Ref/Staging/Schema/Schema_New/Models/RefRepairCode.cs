using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefRepairCode")]
public partial class RefRepairCode
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid RRC_PK { get; set; }

    [Required]
    [Column(TypeName = "bit")]
    public bool? RRC_IsActive { get; set; }

    [Required]
    [Column(TypeName = "varchar(10)")]
    [StringLength(10)]
    [Unicode(false)]
    public string RRC_Code { get; set; }

    [Required]
    [Column(TypeName = "varchar(100)")]
    [StringLength(100)]
    [Unicode(false)]
    public string RRC_Description { get; set; }

    [Required]
    [Column(TypeName = "varchar(15)")]
    [StringLength(15)]
    [Unicode(false)]
    public string RRC_Group { get; set; }

    [Required]
    [Column(TypeName = "char(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string RRC_ServiceType { get; set; }
}
