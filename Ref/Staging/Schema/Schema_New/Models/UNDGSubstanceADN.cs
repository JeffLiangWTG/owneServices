using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("UNDGSubstanceADN")]
public partial class UNDGSubstanceADN
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ADN_PK { get; set; }

    [Required]
    [Column(TypeName = "bit")]
    public bool? ADN_IsActive { get; set; }

    [Required]
    [Column(TypeName = "varchar(4)")]
    [StringLength(4)]
    [Unicode(false)]
    public string ADN_UNNO { get; set; }

    [Required]
    [Column(TypeName = "varchar(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string ADN_Variant { get; set; }

    [Required]
    [Column(TypeName = "varchar(200)")]
    [StringLength(200)]
    [Unicode(false)]
    public string ADN_PSN { get; set; }

    [Required]
    [Column(TypeName = "varchar(4)")]
    [StringLength(4)]
    [Unicode(false)]
    public string ADN_Class { get; set; }

    [Required]
    [Column(TypeName = "varchar(20)")]
    [StringLength(20)]
    [Unicode(false)]
    public string ADN_ClassificationCode { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ADN_PG { get; set; }

    [Required]
    [Column(TypeName = "varchar(20)")]
    [StringLength(20)]
    [Unicode(false)]
    public string ADN_Labels { get; set; }

    [Required]
    [Column(TypeName = "varchar(40)")]
    [StringLength(40)]
    [Unicode(false)]
    public string ADN_SpecialProvisions { get; set; }

    [Required]
    [Column(TypeName = "varchar(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string ADN_ExceptedQuantityCode { get; set; }

    [Column(TypeName = "decimal(9, 3)")]
    public decimal ADN_LQMaxAmt { get; set; }

    [Required]
    [Column(TypeName = "varchar(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string ADN_LQMaxAmtUQ { get; set; }

    [Column(TypeName = "decimal(9, 3)")]
    public decimal ADN_LQ2MaxAmt { get; set; }

    [Required]
    [Column(TypeName = "varchar(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string ADN_LQ2MaxAmtUQ { get; set; }

    [Column(TypeName = "bit")]
    public bool ADN_CarriagePermittedPacks { get; set; }

    [Column(TypeName = "bit")]
    public bool ADN_CarriagePermittedBulk { get; set; }

    [Column(TypeName = "bit")]
    public bool ADN_CarriagePermittedTanks { get; set; }

    [Required]
    [Column(TypeName = "varchar(35)")]
    [StringLength(35)]
    [Unicode(false)]
    public string ADN_CarriagePermittedDetails { get; set; }

    [Column(TypeName = "bit")]
    public bool ADN_EquipPPE { get; set; }

    [Column(TypeName = "bit")]
    public bool ADN_EquipEscapeDevice { get; set; }

    [Column(TypeName = "bit")]
    public bool ADN_EquipGasDetector { get; set; }

    [Column(TypeName = "bit")]
    public bool ADN_EquipToximeter { get; set; }

    [Column(TypeName = "bit")]
    public bool ADN_EquipBreathingApparatus { get; set; }

    [Required]
    [Column(TypeName = "varchar(40)")]
    [StringLength(40)]
    [Unicode(false)]
    public string ADN_EquipmentDetails { get; set; }

    [Required]
    [Column(TypeName = "varchar(40)")]
    [StringLength(40)]
    [Unicode(false)]
    public string ADN_Ventilation { get; set; }

    [Required]
    [Column(TypeName = "varchar(20)")]
    [StringLength(20)]
    [Unicode(false)]
    public string ADN_LoadingSpecialProv { get; set; }

    [Required]
    [Column(TypeName = "varchar(150)")]
    [StringLength(150)]
    [Unicode(false)]
    public string ADN_LoadingSpecialProvNote { get; set; }

    [Required]
    [Column(TypeName = "varchar(20)")]
    [StringLength(20)]
    [Unicode(false)]
    public string ADN_UnloadingSpecialProv { get; set; }

    [Required]
    [Column(TypeName = "varchar(150)")]
    [StringLength(150)]
    [Unicode(false)]
    public string ADN_UnloadingSpecialProvNote { get; set; }

    [Required]
    [Column(TypeName = "varchar(20)")]
    [StringLength(20)]
    [Unicode(false)]
    public string ADN_OperationSpecialProv { get; set; }

    [Required]
    [Column(TypeName = "varchar(150)")]
    [StringLength(150)]
    [Unicode(false)]
    public string ADN_OperationSpecialProvNote { get; set; }

    [Column(TypeName = "tinyint")]
    public byte ADN_BlueCones { get; set; }
}
