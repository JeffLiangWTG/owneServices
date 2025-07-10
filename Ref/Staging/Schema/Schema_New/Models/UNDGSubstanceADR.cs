using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("UNDGSubstanceADR")]
public partial class UNDGSubstanceADR
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ADR_PK { get; set; }

    [Required]
    [Column(TypeName = "bit")]
    public bool? ADR_IsActive { get; set; }

    [Required]
    [Column(TypeName = "varchar(4)")]
    [StringLength(4)]
    [Unicode(false)]
    public string ADR_UNNO { get; set; }

    [Required]
    [Column(TypeName = "varchar(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string ADR_Variant { get; set; }

    [Required]
    [Column(TypeName = "varchar(260)")]
    [StringLength(260)]
    [Unicode(false)]
    public string ADR_PSN { get; set; }

    [Required]
    [Column(TypeName = "varchar(4)")]
    [StringLength(4)]
    [Unicode(false)]
    public string ADR_Class { get; set; }

    [Required]
    [Column(TypeName = "varchar(20)")]
    [StringLength(20)]
    [Unicode(false)]
    public string ADR_ClassificationCode { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ADR_PG { get; set; }

    [Required]
    [Column(TypeName = "varchar(20)")]
    [StringLength(20)]
    [Unicode(false)]
    public string ADR_Labels { get; set; }

    [Required]
    [Column(TypeName = "varchar(40)")]
    [StringLength(40)]
    [Unicode(false)]
    public string ADR_SpecialProvisions { get; set; }

    [Column(TypeName = "decimal(9, 3)")]
    public decimal ADR_LQMaxAmt { get; set; }

    [Required]
    [Column(TypeName = "varchar(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string ADR_LQMaxAmtUQ { get; set; }

    [Column(TypeName = "decimal(9, 3)")]
    public decimal ADR_LQ2MaxAmt { get; set; }

    [Required]
    [Column(TypeName = "varchar(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string ADR_LQ2MaxAmtUQ { get; set; }

    [Required]
    [Column(TypeName = "varchar(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string ADR_ExceptedQuantityCode { get; set; }

    [Required]
    [Column(TypeName = "varchar(50)")]
    [StringLength(50)]
    [Unicode(false)]
    public string ADR_PackIns { get; set; }

    [Required]
    [Column(TypeName = "varchar(50)")]
    [StringLength(50)]
    [Unicode(false)]
    public string ADR_PackProv { get; set; }

    [Required]
    [Column(TypeName = "varchar(50)")]
    [StringLength(50)]
    [Unicode(false)]
    public string ADR_MixedPackingProv { get; set; }

    [Required]
    [Column(TypeName = "varchar(80)")]
    [StringLength(80)]
    [Unicode(false)]
    public string ADR_BulkTankIns { get; set; }

    [Required]
    [Column(TypeName = "varchar(50)")]
    [StringLength(50)]
    [Unicode(false)]
    public string ADR_BulkTankSpecProv { get; set; }

    [Required]
    [Column(TypeName = "varchar(42)")]
    [StringLength(42)]
    [Unicode(false)]
    public string ADR_ADRTankCode { get; set; }

    [Required]
    [Column(TypeName = "varchar(80)")]
    [StringLength(80)]
    [Unicode(false)]
    public string ADR_ADRTankSpecProv { get; set; }

    [Required]
    [Column(TypeName = "varchar(26)")]
    [StringLength(26)]
    [Unicode(false)]
    public string ADR_TankVehicle { get; set; }

    [Required]
    [Column(TypeName = "varchar(15)")]
    [StringLength(15)]
    [Unicode(false)]
    public string ADR_TransportCategory { get; set; }

    [Required]
    [Column(TypeName = "varchar(18)")]
    [StringLength(18)]
    [Unicode(false)]
    public string ADR_PackingSpecialProv { get; set; }

    [Required]
    [Column(TypeName = "varchar(20)")]
    [StringLength(20)]
    [Unicode(false)]
    public string ADR_BulkSpecialProv { get; set; }

    [Required]
    [Column(TypeName = "varchar(48)")]
    [StringLength(48)]
    [Unicode(false)]
    public string ADR_LoadingSpecialProv { get; set; }

    [Required]
    [Column(TypeName = "varchar(36)")]
    [StringLength(36)]
    [Unicode(false)]
    public string ADR_OperationSpecialProv { get; set; }

    [Required]
    [Column(TypeName = "varchar(8)")]
    [StringLength(8)]
    [Unicode(false)]
    public string ADR_HazardIDNumber { get; set; }
}
