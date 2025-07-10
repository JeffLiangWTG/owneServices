using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("StmNote")]
public partial class StmNote
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ST_PK { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid? ST_ParentId { get; set; }

    [Required]
    [Column(TypeName = "varchar(4)")]
    [StringLength(4)]
    [Unicode(false)]
    public string ST_ParentTableCode { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(50)")]
    [StringLength(50)]
    public string ST_Description { get; set; }

    [Required]
    [Column(TypeName = "bit")]
    public bool? ST_ForceRead { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string ST_NoteText { get; set; }

    [Required]
    [Column(TypeName = "char(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ST_NoteType { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ST_NoteContext { get; set; }
}
