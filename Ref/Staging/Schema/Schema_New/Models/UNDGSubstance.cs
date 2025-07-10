using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("UNDGSubstance")]
public partial class UNDGSubstance
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid DG_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(4)")]
    [StringLength(4)]
    [Unicode(false)]
    public string DG_UNNO { get; set; }

    [Required]
    [Column(TypeName = "varchar(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string DG_Variant { get; set; }

    [Required]
    [Column(TypeName = "varchar(150)")]
    [StringLength(150)]
    [Unicode(false)]
    public string DG_Variation { get; set; }

    [Required]
    [Column(TypeName = "varchar(4)")]
    [StringLength(4)]
    [Unicode(false)]
    public string DG_Class { get; set; }

    [Required]
    [Column(TypeName = "varchar(20)")]
    [StringLength(20)]
    [Unicode(false)]
    public string DG_SubLabel1 { get; set; }

    [Required]
    [Column(TypeName = "varchar(20)")]
    [StringLength(20)]
    [Unicode(false)]
    public string DG_SubLabel2 { get; set; }

    [Required]
    [Column(TypeName = "varchar(200)")]
    [StringLength(200)]
    [Unicode(false)]
    public string DG_PSN { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string DG_PG { get; set; }

    [Required]
    [Column(TypeName = "varchar(8)")]
    [StringLength(8)]
    [Unicode(false)]
    public string DG_EMS { get; set; }

    [Required]
    [Column(TypeName = "varchar(1)")]
    [StringLength(1)]
    [Unicode(false)]
    public string DG_MP { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(30)")]
    [StringLength(30)]
    public string DG_FlashPoint { get; set; }

    [Column(TypeName = "decimal(9, 3)")]
    public decimal DG_LQMaxAmt { get; set; }

    [Required]
    [Column(TypeName = "varchar(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string DG_LQMaxAmtUQ { get; set; }

    [Required]
    [Column(TypeName = "varchar(6)")]
    [StringLength(6)]
    [Unicode(false)]
    public string DG_LQSpecProvIndex { get; set; }

    [Required]
    [Column(TypeName = "varchar(1)")]
    [StringLength(1)]
    [Unicode(false)]
    public string DG_TechName { get; set; }

    [Required]
    [Column(TypeName = "varchar(5)")]
    [StringLength(5)]
    [Unicode(false)]
    public string DG_TreatAs { get; set; }

    [Required]
    [Column(TypeName = "varchar(120)")]
    [StringLength(120)]
    [Unicode(false)]
    public string DG_DglPhrase { get; set; }

    [Required]
    [Column(TypeName = "varchar(50)")]
    [StringLength(50)]
    [Unicode(false)]
    public string DG_PackIns { get; set; }

    [Required]
    [Column(TypeName = "varchar(50)")]
    [StringLength(50)]
    [Unicode(false)]
    public string DG_PackProv { get; set; }

    [Required]
    [Column(TypeName = "varchar(50)")]
    [StringLength(50)]
    [Unicode(false)]
    public string DG_IBCIns { get; set; }

    [Required]
    [Column(TypeName = "varchar(50)")]
    [StringLength(50)]
    [Unicode(false)]
    public string DG_IBCProv { get; set; }

    [Required]
    [Column(TypeName = "varchar(50)")]
    [StringLength(50)]
    [Unicode(false)]
    public string DG_IMOTankIns { get; set; }

    [Required]
    [Column(TypeName = "varchar(50)")]
    [StringLength(50)]
    [Unicode(false)]
    public string DG_UNTankIns { get; set; }

    [Required]
    [Column(TypeName = "varchar(50)")]
    [StringLength(50)]
    [Unicode(false)]
    public string DG_TankProv { get; set; }

    [Required]
    [Column(TypeName = "varchar(5)")]
    [StringLength(5)]
    [Unicode(false)]
    public string DG_Markers { get; set; }

    [Required]
    [Column(TypeName = "varchar(8)")]
    [StringLength(8)]
    [Unicode(false)]
    public string DG_Pointers { get; set; }

    [Required]
    [Column(TypeName = "varchar(17)")]
    [StringLength(17)]
    [Unicode(false)]
    public string DG_EXVector { get; set; }

    [Required]
    [Column(TypeName = "varchar(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string DG_StowCat { get; set; }

    [Required]
    [Column(TypeName = "varchar(52)")]
    [StringLength(52)]
    [Unicode(false)]
    public string DG_CodedStow { get; set; }

    [Required]
    [Column(TypeName = "varchar(1)")]
    [StringLength(1)]
    [Unicode(false)]
    public string DG_State { get; set; }

    [Required]
    [Column(TypeName = "varchar(26)")]
    [StringLength(26)]
    [Unicode(false)]
    public string DG_ExpLim { get; set; }

    [Required]
    [Column(TypeName = "varchar(1)")]
    [StringLength(1)]
    [Unicode(false)]
    public string DG_UlineEMS { get; set; }

    [Required]
    [Column(TypeName = "varchar(100)")]
    [StringLength(100)]
    [Unicode(false)]
    public string DG_UsrUSDOTShippingName { get; set; }

    [Required]
    [Column(TypeName = "varchar(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string DG_ExceptedQuantityCode { get; set; }

    [Required]
    [Column(TypeName = "bit")]
    public bool? DG_IsActive { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string DG_Standard { get; set; }

    [Column(TypeName = "bit")]
    public bool DG_IsNotOtherwiseSpecified { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string DG_LQMaxAmtType { get; set; }

    [Required]
    [Column(TypeName = "varchar(50)")]
    [StringLength(50)]
    [Unicode(false)]
    public string DG_PaxPackIns { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string DG_LQ2OrPaxMaxAmtType { get; set; }

    [Column(TypeName = "decimal(9, 3)")]
    public decimal DG_LQ2OrPaxMaxAmt { get; set; }

    [Required]
    [Column(TypeName = "varchar(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string DG_LQ2OrPaxMaxAmtUQ { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string DG_CargoPackAmtType { get; set; }

    [Required]
    [Column(TypeName = "varchar(50)")]
    [StringLength(50)]
    [Unicode(false)]
    public string DG_CargoPackIns { get; set; }

    [Column(TypeName = "decimal(9, 3)")]
    public decimal DG_CargoMaxAmt { get; set; }

    [Required]
    [Column(TypeName = "varchar(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string DG_CargoMaxAmtUQ { get; set; }

    [Required]
    [Column(TypeName = "varchar(4)")]
    [StringLength(4)]
    [Unicode(false)]
    public string DG_EmergencyResponseGuide { get; set; }

    [Required]
    [Column(TypeName = "varchar(100)")]
    [StringLength(100)]
    [Unicode(false)]
    public string DG_Hazards { get; set; }

    [Required]
    [Column(TypeName = "varchar(15)")]
    [StringLength(15)]
    [Unicode(false)]
    public string DG_SpecialHandlingCodes { get; set; }

    [Required]
    [Column(TypeName = "varchar(15)")]
    [StringLength(15)]
    [Unicode(false)]
    public string DG_UniqueRecordId { get; set; }

    [InverseProperty("DA_DGNavigation")]
    public virtual ICollection<UNDGAttribute> UNDGAttributes { get; set; } = new List<UNDGAttribute>();

    [InverseProperty("DR_DGNavigation")]
    public virtual ICollection<UNDGReference> UNDGReferences { get; set; } = new List<UNDGReference>();
}
