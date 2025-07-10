using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefUNLOCORelatedPort")]
public partial class RefUNLOCORelatedPort
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid RLR_PK { get; set; }

    [Column(TypeName = "smallint")]
    public short RLR_GroupNumber { get; set; }

    [Required]
    [Column(TypeName = "varchar(5)")]
    [StringLength(5)]
    [Unicode(false)]
    public string RLR_RL_NKRelatedPort { get; set; }
}
