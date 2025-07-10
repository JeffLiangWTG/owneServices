using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("UNDGSubstanceJTT")]
public partial class UNDGSubstanceJTT
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid JTT_PK { get; set; }

    [Required]
    [Column(TypeName = "bit")]
    public bool? JTT_IsActive { get; set; }

    [Required]
    [Column(TypeName = "varchar(4)")]
    [StringLength(4)]
    [Unicode(false)]
    public string JTT_UNNO { get; set; }

    [Required]
    [Column(TypeName = "varchar(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string JTT_Variant { get; set; }

    [Required]
    [Column(TypeName = "varchar(300)")]
    [StringLength(300)]
    [Unicode(false)]
    public string JTT_PSN { get; set; }

    [Required]
    [Column(TypeName = "varchar(4)")]
    [StringLength(4)]
    [Unicode(false)]
    public string JTT_Class { get; set; }

    [Required]
    [Column(TypeName = "varchar(20)")]
    [StringLength(20)]
    [Unicode(false)]
    public string JTT_ClassificationCode { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string JTT_PG { get; set; }

    [Required]
    [Column(TypeName = "varchar(20)")]
    [StringLength(20)]
    [Unicode(false)]
    public string JTT_Labels { get; set; }

    [Required]
    [Column(TypeName = "varchar(40)")]
    [StringLength(40)]
    [Unicode(false)]
    public string JTT_SpecialProvisions { get; set; }

    [Column(TypeName = "decimal(9, 3)")]
    public decimal JTT_LQMaxAmt { get; set; }

    [Required]
    [Column(TypeName = "varchar(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string JTT_LQMaxAmtUQ { get; set; }

    [Column(TypeName = "decimal(9, 3)")]
    public decimal JTT_LQ2MaxAmt { get; set; }

    [Required]
    [Column(TypeName = "varchar(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string JTT_LQ2MaxAmtUQ { get; set; }

    [Required]
    [Column(TypeName = "varchar(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string JTT_ExceptedQuantityCode { get; set; }

    [Required]
    [Column(TypeName = "varchar(50)")]
    [StringLength(50)]
    [Unicode(false)]
    public string JTT_PackIns { get; set; }

    [Required]
    [Column(TypeName = "varchar(50)")]
    [StringLength(50)]
    [Unicode(false)]
    public string JTT_PackProv { get; set; }

    [Required]
    [Column(TypeName = "varchar(50)")]
    [StringLength(50)]
    [Unicode(false)]
    public string JTT_MixedPackingProv { get; set; }

    [Required]
    [Column(TypeName = "varchar(80)")]
    [StringLength(80)]
    [Unicode(false)]
    public string JTT_BulkTankIns { get; set; }

    [Required]
    [Column(TypeName = "varchar(50)")]
    [StringLength(50)]
    [Unicode(false)]
    public string JTT_BulkTankSpecProv { get; set; }

    [Required]
    [Column(TypeName = "varchar(42)")]
    [StringLength(42)]
    [Unicode(false)]
    public string JTT_TankCode { get; set; }

    [Required]
    [Column(TypeName = "varchar(62)")]
    [StringLength(62)]
    [Unicode(false)]
    public string JTT_TankSpecProv { get; set; }

    [Required]
    [Column(TypeName = "varchar(26)")]
    [StringLength(26)]
    [Unicode(false)]
    public string JTT_TankVehicle { get; set; }

    [Required]
    [Column(TypeName = "varchar(15)")]
    [StringLength(15)]
    [Unicode(false)]
    public string JTT_TransportCategory { get; set; }

    [Required]
    [Column(TypeName = "varchar(18)")]
    [StringLength(18)]
    [Unicode(false)]
    public string JTT_PackingSpecialProv { get; set; }

    [Required]
    [Column(TypeName = "varchar(20)")]
    [StringLength(20)]
    [Unicode(false)]
    public string JTT_BulkSpecialProv { get; set; }

    [Required]
    [Column(TypeName = "varchar(48)")]
    [StringLength(48)]
    [Unicode(false)]
    public string JTT_LoadingSpecialProv { get; set; }

    [Required]
    [Column(TypeName = "varchar(36)")]
    [StringLength(36)]
    [Unicode(false)]
    public string JTT_OperationSpecialProv { get; set; }

    [Required]
    [Column(TypeName = "varchar(8)")]
    [StringLength(8)]
    [Unicode(false)]
    public string JTT_HazardIDNumber { get; set; }
}
