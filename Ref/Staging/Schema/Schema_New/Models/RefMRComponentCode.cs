using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefMRComponentCode")]
public partial class RefMRComponentCode
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid RCC_PK { get; set; }

    [Required]
    [Column(TypeName = "bit")]
    public bool? RCC_IsActive { get; set; }

    [Required]
    [Column(TypeName = "varchar(10)")]
    [StringLength(10)]
    [Unicode(false)]
    public string RCC_Code { get; set; }

    [Required]
    [Column(TypeName = "varchar(100)")]
    [StringLength(100)]
    [Unicode(false)]
    public string RCC_Description { get; set; }

    [Required]
    [Column(TypeName = "varchar(15)")]
    [StringLength(15)]
    [Unicode(false)]
    public string RCC_Group { get; set; }

    [Column(TypeName = "bit")]
    public bool RCC_Machinery { get; set; }

    [Column(TypeName = "bit")]
    public bool RCC_Structural { get; set; }

    [Column(TypeName = "bit")]
    public bool RCC_TankCleaning { get; set; }

    [Column(TypeName = "bit")]
    public bool RCC_TankRepair { get; set; }
}
