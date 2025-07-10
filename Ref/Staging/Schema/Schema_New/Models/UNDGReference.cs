using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("UNDGReference")]
[Index("DR_DG", Name = "IX_UNDGReference_DR_DG")]
public partial class UNDGReference
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid DR_PK { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid DR_DG { get; set; }

    [Required]
    [Column(TypeName = "varchar(5)")]
    [StringLength(5)]
    [Unicode(false)]
    public string DR_Type { get; set; }

    [Required]
    [Column(TypeName = "varchar(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string DR_RN_NKCountry { get; set; }

    [Required]
    [Column(TypeName = "varchar(6)")]
    [StringLength(6)]
    [Unicode(false)]
    public string DR_Code { get; set; }

    [Required]
    [Column(TypeName = "varchar(100)")]
    [StringLength(100)]
    [Unicode(false)]
    public string DR_Description { get; set; }

    [Column(TypeName = "bit")]
    public bool DR_HasFlashPointLower { get; set; }

    [Column(TypeName = "decimal(8, 1)")]
    public decimal DR_FlashPointLower { get; set; }

    [Column(TypeName = "bit")]
    public bool DR_HasFlashPointUpper { get; set; }

    [Column(TypeName = "decimal(8, 1)")]
    public decimal DR_FlashPointUpper { get; set; }

    [ForeignKey("DR_DG")]
    [InverseProperty("UNDGReferences")]
    public virtual UNDGSubstance DR_DGNavigation { get; set; }
}
