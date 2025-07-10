using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefShippingLineMessagingRequirement")]
[Index("RSR_RSL_ShippingLine", Name = "IX_RefShippingLineMessagingRequirement_RSR_RSL_ShippingLine")]
public partial class RefShippingLineMessagingRequirement
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid RSR_PK { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid RSR_RSL_ShippingLine { get; set; }

    [Required]
    [Column(TypeName = "char(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string RSR_RST_NKType { get; set; }

    [Column(TypeName = "bit")]
    public bool RSR_IsBookingRequest { get; set; }

    [Column(TypeName = "bit")]
    public bool RSR_IsShippingOrder { get; set; }

    [Column(TypeName = "bit")]
    public bool RSR_IsShippingInstruction { get; set; }

    [Column(TypeName = "bit")]
    public bool RSR_IsEManifest { get; set; }

    [Column(TypeName = "bit")]
    public bool RSR_IsVerifiedGrossContainerWeight { get; set; }

    [ForeignKey("RSR_RSL_ShippingLine")]
    [InverseProperty("RefShippingLineMessagingRequirements")]
    public virtual RefShippingLine RSR_RSL_ShippingLineNavigation { get; set; }
}
