using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefUnitSection")]
public partial class RefUnitSection
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid RUS_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(10)")]
    [StringLength(10)]
    [Unicode(false)]
    public string RUS_Code { get; set; }

    [Required]
    [Column(TypeName = "varchar(50)")]
    [StringLength(50)]
    [Unicode(false)]
    public string RUS_Description { get; set; }

    [Required]
    [Column(TypeName = "varchar(10)")]
    [StringLength(10)]
    [Unicode(false)]
    public string RUS_Group { get; set; }
}
