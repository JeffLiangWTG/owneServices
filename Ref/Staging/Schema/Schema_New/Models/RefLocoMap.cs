using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefLocoMap")]
public partial class RefLocoMap
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid RY_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(10)")]
    [StringLength(10)]
    [Unicode(false)]
    public string RY_LocalPortCode { get; set; }

    [Required]
    [Column(TypeName = "varchar(5)")]
    [StringLength(5)]
    [Unicode(false)]
    public string RY_RL_NKLocoPort { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string RY_SystemUsage { get; set; }

    [Column(TypeName = "char(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string RY_RN_NKCountryCode { get; set; }
}
