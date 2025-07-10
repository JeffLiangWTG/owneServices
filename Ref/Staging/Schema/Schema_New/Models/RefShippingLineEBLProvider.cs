using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefShippingLineEBLProvider")]
public partial class RefShippingLineEBLProvider
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid RSE_PK { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid RSE_RSL_ShippingLine { get; set; }

    [Required]
    [Column(TypeName = "varchar(255)")]
    [StringLength(255)]
    [Unicode(false)]
    public string RSE_Name { get; set; }

    [Column(TypeName = "bit")]
    public bool RSE_IsAvailable { get; set; }

    [Column(TypeName = "bit")]
    public bool RSE_IsDefault { get; set; }

    [ForeignKey("RSE_RSL_ShippingLine")]
    [InverseProperty("RefShippingLineEBLProviders")]
    public virtual RefShippingLine RSE_RSL_ShippingLineNavigation { get; set; }
}
