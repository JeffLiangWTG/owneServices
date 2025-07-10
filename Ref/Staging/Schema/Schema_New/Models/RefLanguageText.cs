using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefLanguageText")]
public partial class RefLanguageText
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid RLT_PK { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid RLT_ParentId { get; set; }

    [Required]
    [Column(TypeName = "varchar(4)")]
    [StringLength(4)]
    [Unicode(false)]
    public string RLT_ParentTableCode { get; set; }

    [Required]
    [Column(TypeName = "varchar(7)")]
    [StringLength(7)]
    [Unicode(false)]
    public string RLT_Language { get; set; }

    [Required]
    [Column(TypeName = "varchar(50)")]
    [StringLength(50)]
    [Unicode(false)]
    public string RLT_ColumnName { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(1000)")]
    [StringLength(1000)]
    public string RLT_Text { get; set; }
}
