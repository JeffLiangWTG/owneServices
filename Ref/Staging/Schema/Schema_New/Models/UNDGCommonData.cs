using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

public partial class UNDGCommonData
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid DC_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string DC_Language { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string DC_Type { get; set; }

    [Required]
    [Column(TypeName = "varchar(6)")]
    [StringLength(6)]
    [Unicode(false)]
    public string DC_Index { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string DC_Descriptor { get; set; }
}
