using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("UNDGSubstanceCFR")]
public partial class UNDGSubstanceCFR
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid CFR_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(4)")]
    [StringLength(4)]
    [Unicode(false)]
    public string CFR_UNNO { get; set; }

    [Required]
    [Column(TypeName = "varchar(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string CFR_Variant { get; set; }

    [Required]
    [Column(TypeName = "varchar(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string CFR_Prefix { get; set; }

    [Required]
    [Column(TypeName = "varchar(4)")]
    [StringLength(4)]
    [Unicode(false)]
    public string CFR_CVL { get; set; }

    [Required]
    [Column(TypeName = "varchar(205)")]
    [StringLength(205)]
    [Unicode(false)]
    public string CFR_PSN { get; set; }

    [Required]
    [Column(TypeName = "varchar(150)")]
    [StringLength(150)]
    [Unicode(false)]
    public string CFR_Variation { get; set; }

    [Required]
    [Column(TypeName = "varchar(4)")]
    [StringLength(4)]
    [Unicode(false)]
    public string CFR_PrimaryClass { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string CFR_SecondaryClass { get; set; }

    [Required]
    [Column(TypeName = "char(1)")]
    [StringLength(1)]
    [Unicode(false)]
    public string CFR_TertiaryClass { get; set; }

    [Required]
    [Column(TypeName = "char(1)")]
    [StringLength(1)]
    [Unicode(false)]
    public string CFR_MarinePollutant { get; set; }

    [Required]
    [Column(TypeName = "varchar(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string CFR_ExceptedQuantity { get; set; }

    [Column(TypeName = "bit")]
    public bool CFR_LimitedQuantityPermitted { get; set; }

    [Column(TypeName = "decimal(9, 3)")]
    public decimal CFR_ReportableQuantity { get; set; }

    [Required]
    [Column(TypeName = "varchar(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string CFR_ReportableQuantityUnit { get; set; }

    [Required]
    [Column(TypeName = "varchar(8)")]
    [StringLength(8)]
    [Unicode(false)]
    public string CFR_GeneralStowage { get; set; }

    [Required]
    [Column(TypeName = "varchar(8)")]
    [StringLength(8)]
    [Unicode(false)]
    public string CFR_PassengerStowage { get; set; }

    [Required]
    [Column(TypeName = "varchar(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string CFR_StowageCategory { get; set; }

    [Required]
    [Column(TypeName = "varchar(36)")]
    [StringLength(36)]
    [Unicode(false)]
    public string CFR_StowageCodes { get; set; }

    [Required]
    [Column(TypeName = "varchar(36)")]
    [StringLength(36)]
    [Unicode(false)]
    public string CFR_StowageIMDGCodes { get; set; }

    [Required]
    [Column(TypeName = "varchar(12)")]
    [StringLength(12)]
    [Unicode(false)]
    public string CFR_BulkPackingInstructions { get; set; }

    [Required]
    [Column(TypeName = "varchar(30)")]
    [StringLength(30)]
    [Unicode(false)]
    public string CFR_BulkPackingProvisions { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string CFR_IBCInstructions { get; set; }

    [Required]
    [Column(TypeName = "varchar(12)")]
    [StringLength(12)]
    [Unicode(false)]
    public string CFR_IBCProvisions { get; set; }

    [Required]
    [Column(TypeName = "varchar(12)")]
    [StringLength(12)]
    [Unicode(false)]
    public string CFR_PackingExceptions { get; set; }

    [Required]
    [Column(TypeName = "varchar(13)")]
    [StringLength(13)]
    [Unicode(false)]
    public string CFR_PackingInstructions { get; set; }

    [Required]
    [Column(TypeName = "varchar(16)")]
    [StringLength(16)]
    [Unicode(false)]
    public string CFR_PackingProvisions { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string CFR_PackingGroup { get; set; }

    [Required]
    [Column(TypeName = "varchar(24)")]
    [StringLength(24)]
    [Unicode(false)]
    public string CFR_SpecialProvisions { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string CFR_TankInstructions { get; set; }

    [Required]
    [Column(TypeName = "varchar(30)")]
    [StringLength(30)]
    [Unicode(false)]
    public string CFR_TankProvisions { get; set; }

    [Required]
    [Column(TypeName = "char(1)")]
    [StringLength(1)]
    [Unicode(false)]
    public string CFR_PoisonInhalationHazard { get; set; }

    [Required]
    [Column(TypeName = "char(1)")]
    [StringLength(1)]
    [Unicode(false)]
    public string CFR_State { get; set; }

    [Column(TypeName = "bit")]
    public bool CFR_IsFixedPSN { get; set; }

    [Column(TypeName = "bit")]
    public bool CFR_AppliesForAirTransport { get; set; }

    [Column(TypeName = "bit")]
    public bool CFR_AppliesForDomesticTransport { get; set; }

    [Column(TypeName = "bit")]
    public bool CFR_AppliesForInternationalTransport { get; set; }

    [Column(TypeName = "bit")]
    public bool CFR_AppliesForVesselTransport { get; set; }

    [Column(TypeName = "bit")]
    public bool CFR_RequiresTechnicalNameInParenthesis { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string CFR_EmergencyResponseGuide { get; set; }

    [Required]
    [Column(TypeName = "char(1)")]
    [StringLength(1)]
    [Unicode(false)]
    public string CFR_TechnicalName { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string CFR_TreatAs { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string CFR_PAXAirRailLimitType { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string CFR_CargoAirRailLimitType { get; set; }

    [Column(TypeName = "decimal(9, 3)")]
    public decimal CFR_PAXAirRailLimit { get; set; }

    [Required]
    [Column(TypeName = "varchar(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string CFR_PAXAirRailLimitUnit { get; set; }

    [Column(TypeName = "decimal(9, 3)")]
    public decimal CFR_CargoAirRailLimit { get; set; }

    [Required]
    [Column(TypeName = "varchar(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string CFR_CargoAirRailLimitUnit { get; set; }

    [Column(TypeName = "decimal(9, 3)")]
    public decimal CFR_SecondaryPAXAirRailLimit { get; set; }

    [Required]
    [Column(TypeName = "varchar(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string CFR_SecondaryPAXAirRailLimitUnit { get; set; }

    [Column(TypeName = "decimal(9, 3)")]
    public decimal CFR_SecondaryCargoAirRailLimit { get; set; }

    [Required]
    [Column(TypeName = "varchar(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string CFR_SecondaryCargoAirRailLimitUnit { get; set; }

    [Column(TypeName = "decimal(9, 3)")]
    public decimal CFR_LQMaxAmt { get; set; }

    [Required]
    [Column(TypeName = "varchar(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string CFR_LQMaxAmtUQ { get; set; }

    [Required]
    [Column(TypeName = "bit")]
    public bool? CFR_IsActive { get; set; }
}
