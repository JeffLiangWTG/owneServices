using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("UNDGCountryReferencePivot")]
public partial class UNDGCountryReferencePivot
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid DCP_PK { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid DCP_DCR { get; set; }

    [Required]
    [Column(TypeName = "varchar(4)")]
    [StringLength(4)]
    [Unicode(false)]
    public string DCP_UNNO { get; set; }

    [Required]
    [Column(TypeName = "varchar(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string DCP_Variant { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string DCP_Standard { get; set; }

    [ForeignKey("DCP_DCR")]
    [InverseProperty("UNDGCountryReferencePivots")]
    public virtual UNDGCountryReference DCP_DCRNavigation { get; set; }
}
