using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefShippingLineMessagingRequirementType")]
public partial class RefShippingLineMessagingRequirementType
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid RST_PK { get; set; }

    [Required]
    [Column(TypeName = "char(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string RST_Code { get; set; }

    [Required]
    [Column(TypeName = "varchar(256)")]
    [StringLength(256)]
    [Unicode(false)]
    public string RST_Description { get; set; }
}
