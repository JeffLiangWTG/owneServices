using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusNomenclatureGroupType")]
public partial class RefCusNomenclatureGroupType
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZZ9_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZZ9_GroupType { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string ZZ9_Description { get; set; }
}
