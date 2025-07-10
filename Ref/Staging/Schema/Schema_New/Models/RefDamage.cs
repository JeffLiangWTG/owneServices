using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefDamage")]
public partial class RefDamage
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid RFM_PK { get; set; }

    [Required]
    [Column(TypeName = "bit")]
    public bool? RFM_IsActive { get; set; }

    [Required]
    [Column(TypeName = "varchar(10)")]
    [StringLength(10)]
    [Unicode(false)]
    public string RFM_Code { get; set; }

    [Required]
    [Column(TypeName = "varchar(240)")]
    [StringLength(240)]
    [Unicode(false)]
    public string RFM_Description { get; set; }

    [Required]
    [Column(TypeName = "varchar(25)")]
    [StringLength(25)]
    [Unicode(false)]
    public string RFM_Group { get; set; }
}
