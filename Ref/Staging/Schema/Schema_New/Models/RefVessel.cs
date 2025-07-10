using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefVessel")]
public partial class RefVessel
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid RV_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(50)")]
    [StringLength(50)]
    [Unicode(false)]
    public string RV_Code { get; set; }

    [Required]
    [Column(TypeName = "bit")]
    public bool? RV_IsActive { get; set; }

    [Required]
    [Column(TypeName = "char(7)")]
    [StringLength(7)]
    [Unicode(false)]
    public string RV_LloydsNumber { get; set; }

    [Required]
    [Column(TypeName = "varchar(9)")]
    [StringLength(9)]
    [Unicode(false)]
    public string RV_MalaysiaVesselId { get; set; }

    [Required]
    [Column(TypeName = "varchar(10)")]
    [StringLength(10)]
    [Unicode(false)]
    public string RV_RadioCallSign { get; set; }

    [Column(TypeName = "int")]
    public int RV_NetRegisterTon { get; set; }

    [Required]
    [Column(TypeName = "char(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string RV_VesselType { get; set; }

    [Column(TypeName = "smallint")]
    public short RV_YearOfConstruction { get; set; }

    [Required]
    [Column(TypeName = "varchar(10)")]
    [StringLength(10)]
    [Unicode(false)]
    public string RV_CustomAttrib1 { get; set; }

    [Required]
    [Column(TypeName = "varchar(10)")]
    [StringLength(10)]
    [Unicode(false)]
    public string RV_CustomAttrib2 { get; set; }

    [Required]
    [Column(TypeName = "varchar(10)")]
    [StringLength(10)]
    [Unicode(false)]
    public string RV_CustomAttrib3 { get; set; }

    [Column(TypeName = "bit")]
    public bool RV_CustomFlag1 { get; set; }

    [Column(TypeName = "decimal(9, 3)")]
    public decimal RV_CustomDecimal1 { get; set; }

    [Required]
    [Column(TypeName = "varchar(4)")]
    [StringLength(4)]
    [Unicode(false)]
    public string RV_CarrierCode { get; set; }

    [Required]
    [Column(TypeName = "char(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string RV_ScreeningStatus { get; set; }

    [Required]
    [Column(TypeName = "varchar(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string RV_RN_NKCountryOfReg { get; set; }

    [Required]
    [Column(TypeName = "char(9)")]
    [StringLength(9)]
    [Unicode(false)]
    public string RV_MaritimeMobileServiceIdentity { get; set; }

    [Required]
    [Column(TypeName = "char(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string RV_StatusCode { get; set; }

    [Required]
    [Column(TypeName = "char(7)")]
    [StringLength(7)]
    [Unicode(false)]
    public string RV_StatCode5 { get; set; }

    [Column(TypeName = "bit")]
    public bool RV_IsGearless { get; set; }

    [Column(TypeName = "decimal(7, 3)")]
    public decimal RV_Length { get; set; }

    [Column(TypeName = "decimal(7, 3)")]
    public decimal RV_Breadth { get; set; }

    [Column(TypeName = "decimal(7, 3)")]
    public decimal RV_Draught { get; set; }

    [Column(TypeName = "decimal(10, 3)")]
    public decimal RV_Deadweight { get; set; }

    [Column(TypeName = "decimal(10, 3)")]
    public decimal RV_GrossTonnage { get; set; }

    [Column(TypeName = "decimal(10, 3)")]
    public decimal RV_GrainCapacity { get; set; }

    [Column(TypeName = "decimal(10, 3)")]
    public decimal RV_LiquidCapacity { get; set; }

    [Column(TypeName = "decimal(8, 3)")]
    public decimal RV_RoroLanesLength { get; set; }

    [Column(TypeName = "decimal(8, 3)")]
    public decimal RV_RoroLanesWidth { get; set; }

    [Column(TypeName = "decimal(6, 3)")]
    public decimal RV_RoroLanesClearHeight { get; set; }

    [Column(TypeName = "smallint")]
    public short RV_RoroLanesNumber { get; set; }

    [Column(TypeName = "tinyint")]
    public byte RV_RoroRampsNumber { get; set; }

    [Column(TypeName = "int")]
    public int RV_TEU { get; set; }

    [Column(TypeName = "int")]
    public int RV_CarsNumber { get; set; }

    [Column(TypeName = "smallint")]
    public short RV_ReeferPointsNumber { get; set; }

    [Column(TypeName = "smallint")]
    public short RV_TanksNumber { get; set; }
}
