using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefShippingLine")]
public partial class RefShippingLine
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid RSL_PK { get; set; }

    [Required]
    [Column(TypeName = "bit")]
    public bool? RSL_IsActive { get; set; }

    [Column(TypeName = "bit")]
    public bool RSL_IsNVO { get; set; }

    [Column(TypeName = "bit")]
    public bool RSL_BookingRequestAvailable { get; set; }

    [Column(TypeName = "bit")]
    public bool RSL_ShippingInstructionAvailable { get; set; }

    [Column(TypeName = "bit")]
    public bool RSL_VerifiedGrossContainerWeightAvailable { get; set; }

    [Column(TypeName = "bit")]
    public bool RSL_ShippingOrderAvailable { get; set; }

    [Column(TypeName = "bit")]
    public bool RSL_EManifestAvailable { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(75)")]
    [StringLength(75)]
    public string RSL_CarrierName { get; set; }

    [Required]
    [Column(TypeName = "varchar(4)")]
    [StringLength(4)]
    [Unicode(false)]
    public string RSL_StandardCarrierAlphaCode { get; set; }

    [Required]
    [Column(TypeName = "varchar(4)")]
    [StringLength(4)]
    [Unicode(false)]
    public string RSL_CargoWiseOneCode { get; set; }

    [Column(TypeName = "bit")]
    public bool RSL_OceanCarrierMessagingAvailable { get; set; }

    [Column(TypeName = "bit")]
    public bool RSL_GlobalSailingScheduleAvailable { get; set; }

    [Column(TypeName = "bit")]
    public bool RSL_ContainerAutomationAvailable { get; set; }

    [Column(TypeName = "bit")]
    public bool RSL_CargoSphereRatesAvailable { get; set; }

    [Column(TypeName = "bit")]
    public bool RSL_IsShippingLine { get; set; }

    [Column(TypeName = "bit")]
    public bool RSL_InvoiceAvailable { get; set; }

    [Column(TypeName = "bit")]
    public bool RSL_IsCW1User { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(250)")]
    [StringLength(250)]
    public string RSL_EHubIds { get; set; }

    [Column(TypeName = "varbinary(max)")]
    public byte[] RSL_ShippingLineLogo { get; set; }

    [InverseProperty("RSE_RSL_ShippingLineNavigation")]
    public virtual ICollection<RefShippingLineEBLProvider> RefShippingLineEBLProviders { get; set; } = new List<RefShippingLineEBLProvider>();

    [InverseProperty("RSR_RSL_ShippingLineNavigation")]
    public virtual ICollection<RefShippingLineMessagingRequirement> RefShippingLineMessagingRequirements { get; set; } = new List<RefShippingLineMessagingRequirement>();
}
