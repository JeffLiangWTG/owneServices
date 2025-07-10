using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefApplicationAttributeType")]
[Index("RAT_Type", Name = "IX_RefApplicationAttributeType_RAT_Type", IsUnique = true)]
public partial class RefApplicationAttributeType
{
    [Key]
    public Guid RAT_PK { get; set; }

    [Required]
    [StringLength(10)]
    [Unicode(false)]
    public string RAT_Type { get; set; }

    [Required]
    [StringLength(200)]
    public string RAT_Description { get; set; }

    public virtual ICollection<RefApplicationAttribute> RefApplicationAttributes { get; set; } = new List<RefApplicationAttribute>();
}
