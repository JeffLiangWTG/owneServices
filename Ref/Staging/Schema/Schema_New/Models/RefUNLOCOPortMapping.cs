using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefUNLOCOPortMapping")]
public partial class RefUNLOCOPortMapping
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid RLM_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(5)")]
    [StringLength(5)]
    [Unicode(false)]
    public string RLM_RL_NKCode { get; set; }

    [Required]
    [Column(TypeName = "varchar(5)")]
    [StringLength(5)]
    [Unicode(false)]
    public string RLM_RL_NKCodeRelated { get; set; }
}
