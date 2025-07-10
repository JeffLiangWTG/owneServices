using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("UNDGCountryReference")]
public partial class UNDGCountryReference
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid DCR_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(5)")]
    [StringLength(5)]
    [Unicode(false)]
    public string DCR_Type { get; set; }

    [Required]
    [Column(TypeName = "varchar(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string DCR_RN_NKCountry { get; set; }

    [Required]
    [Column(TypeName = "varchar(6)")]
    [StringLength(6)]
    [Unicode(false)]
    public string DCR_Code { get; set; }

    [Required]
    [Column(TypeName = "varchar(200)")]
    [StringLength(200)]
    [Unicode(false)]
    public string DCR_Description { get; set; }

    [Column(TypeName = "bit")]
    public bool DCR_HasFlashPointLower { get; set; }

    [Column(TypeName = "decimal(8, 1)")]
    public decimal DCR_FlashPointLowerCentigrade { get; set; }

    [Column(TypeName = "bit")]
    public bool DCR_HasFlashPointUpper { get; set; }

    [Column(TypeName = "decimal(8, 1)")]
    public decimal DCR_FlashPointUpperCentigrade { get; set; }

    [InverseProperty("DCP_DCRNavigation")]
    public virtual ICollection<UNDGCountryReferencePivot> UNDGCountryReferencePivots { get; set; } = new List<UNDGCountryReferencePivot>();
}
