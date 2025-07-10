using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("AutoSchema")]
public partial class AutoSchema
{
    [Key]
    public Guid AS_PK { get; set; }

    [Required]
    [StringLength(200)]
    [Unicode(false)]
    public string AS_SchemaName { get; set; }

    [Required]
    [Column(TypeName = "xml")]
    public string AS_Schema { get; set; }

    [Required]
    [StringLength(3)]
    [Unicode(false)]
    public string AS_TableCode { get; set; }
}
