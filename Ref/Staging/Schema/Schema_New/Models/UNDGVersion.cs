using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("UNDGVersion")]
public partial class UNDGVersion
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid DV_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(20)")]
    [StringLength(20)]
    [Unicode(false)]
    public string DV_Name { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string DV_Standard { get; set; }

    [Required]
    [Column(TypeName = "bit")]
    public bool? DV_IsActive { get; set; }
}
