using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("UNDGSubstanceRID")]
public partial class UNDGSubstanceRID
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid RID_PK { get; set; }

    [Required]
    [Column(TypeName = "bit")]
    public bool? RID_IsActive { get; set; }

    [Required]
    [Column(TypeName = "varchar(4)")]
    [StringLength(4)]
    [Unicode(false)]
    public string RID_UNNO { get; set; }

    [Required]
    [Column(TypeName = "varchar(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string RID_Variant { get; set; }

    [Required]
    [Column(TypeName = "varchar(200)")]
    [StringLength(200)]
    [Unicode(false)]
    public string RID_PSN { get; set; }

    [Required]
    [Column(TypeName = "varchar(4)")]
    [StringLength(4)]
    [Unicode(false)]
    public string RID_Class { get; set; }

    [Required]
    [Column(TypeName = "varchar(20)")]
    [StringLength(20)]
    [Unicode(false)]
    public string RID_ClassificationCode { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string RID_PG { get; set; }

    [Required]
    [Column(TypeName = "varchar(20)")]
    [StringLength(20)]
    [Unicode(false)]
    public string RID_Labels { get; set; }

    [Required]
    [Column(TypeName = "varchar(46)")]
    [StringLength(46)]
    [Unicode(false)]
    public string RID_SpecialProvisions { get; set; }

    [Column(TypeName = "decimal(9, 3)")]
    public decimal RID_LQMaxAmt { get; set; }

    [Required]
    [Column(TypeName = "varchar(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string RID_LQMaxAmtUQ { get; set; }

    [Column(TypeName = "decimal(9, 3)")]
    public decimal RID_LQ2MaxAmt { get; set; }

    [Required]
    [Column(TypeName = "varchar(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string RID_LQ2MaxAmtUQ { get; set; }

    [Required]
    [Column(TypeName = "varchar(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string RID_ExceptedQuantityCode { get; set; }

    [Required]
    [Column(TypeName = "varchar(50)")]
    [StringLength(50)]
    [Unicode(false)]
    public string RID_PackIns { get; set; }

    [Required]
    [Column(TypeName = "varchar(46)")]
    [StringLength(46)]
    [Unicode(false)]
    public string RID_IBCIns { get; set; }

    [Required]
    [Column(TypeName = "varchar(48)")]
    [StringLength(48)]
    [Unicode(false)]
    public string RID_PackProv { get; set; }

    [Required]
    [Column(TypeName = "varchar(18)")]
    [StringLength(18)]
    [Unicode(false)]
    public string RID_MixedPackProv { get; set; }

    [Required]
    [Column(TypeName = "varchar(20)")]
    [StringLength(20)]
    [Unicode(false)]
    public string RID_BulkContainerTankIns { get; set; }

    [Required]
    [Column(TypeName = "varchar(34)")]
    [StringLength(34)]
    [Unicode(false)]
    public string RID_BulkContainerTankProv { get; set; }

    [Required]
    [Column(TypeName = "varchar(35)")]
    [StringLength(35)]
    [Unicode(false)]
    public string RID_TankCode { get; set; }

    [Required]
    [Column(TypeName = "varchar(60)")]
    [StringLength(60)]
    [Unicode(false)]
    public string RID_TankSpecProv { get; set; }

    [Required]
    [Column(TypeName = "varchar(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string RID_TransportCategory { get; set; }

    [Required]
    [Column(TypeName = "varchar(18)")]
    [StringLength(18)]
    [Unicode(false)]
    public string RID_CarriagePackagesSpecialProv { get; set; }

    [Required]
    [Column(TypeName = "varchar(38)")]
    [StringLength(38)]
    [Unicode(false)]
    public string RID_CarriageBulkSpecialProv { get; set; }

    [Required]
    [Column(TypeName = "varchar(40)")]
    [StringLength(40)]
    [Unicode(false)]
    public string RID_CarriageLoadingSpecialProv { get; set; }

    [Required]
    [Column(TypeName = "varchar(36)")]
    [StringLength(36)]
    [Unicode(false)]
    public string RID_ColisExpressCode { get; set; }

    [Required]
    [Column(TypeName = "varchar(8)")]
    [StringLength(8)]
    [Unicode(false)]
    public string RID_HazardIDNumber { get; set; }
}
