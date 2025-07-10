using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("NamedEntityClassification")]
[Index("NEC_Language", "NEC_Class", Name = "IX_NamedEntityClassification_NEC_Language_NEC_Class")]
public partial class NamedEntityClassification
{
    [Key]
    public Guid NEC_PK { get; set; }

    [Required]
    public string NEC_Name { get; set; }

    [Required]
    [StringLength(50)]
    [Unicode(false)]
    public string NEC_Class { get; set; }

    [Required]
    [StringLength(7)]
    [Unicode(false)]
    public string NEC_Language { get; set; }

    [Required]
    [StringLength(20)]
    [Unicode(false)]
    public string NEC_Code { get; set; }
}
