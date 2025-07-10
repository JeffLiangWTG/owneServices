using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefEquipmentGrade")]
public partial class RefEquipmentGrade
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid REG_PK { get; set; }

    [Required]
    [Column(TypeName = "bit")]
    public bool? REG_IsActive { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string REG_Code { get; set; }

    [Required]
    [Column(TypeName = "varchar(50)")]
    [StringLength(50)]
    [Unicode(false)]
    public string REG_Description { get; set; }
}
