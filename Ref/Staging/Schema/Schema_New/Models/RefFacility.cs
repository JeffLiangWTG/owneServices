using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefFacility")]
public partial class RefFacility
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid RFT_PK { get; set; }

    [Required]
    [Column(TypeName = "bit")]
    public bool? RFT_IsActive { get; set; }

    [Required]
    [Column(TypeName = "char(11)")]
    [StringLength(11)]
    [Unicode(false)]
    public string RFT_Code { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(128)")]
    [StringLength(128)]
    public string RFT_Name { get; set; }

    [Required]
    [Column(TypeName = "char(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string RFT_FacilityType { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(50)")]
    [StringLength(50)]
    public string RFT_Address1 { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(50)")]
    [StringLength(50)]
    public string RFT_Address2 { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(50)")]
    [StringLength(50)]
    public string RFT_City { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(25)")]
    [StringLength(25)]
    public string RFT_State { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(10)")]
    [StringLength(10)]
    public string RFT_PostCode { get; set; }

    [Required]
    [Column(TypeName = "varchar(5)")]
    [StringLength(5)]
    [Unicode(false)]
    public string RFT_RL_NKLocationCode { get; set; }

    [Required]
    [Column(TypeName = "char(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string RFT_RN_NKCountryCode { get; set; }

    [Required]
    [Column(TypeName = "varchar(12)")]
    [StringLength(12)]
    [Unicode(false)]
    public string RFT_SMDGCode { get; set; }

    [Required]
    [Column(TypeName = "varchar(9)")]
    [StringLength(9)]
    [Unicode(false)]
    public string RFT_BICCode { get; set; }

    [Required]
    [Column(TypeName = "varchar(22)")]
    [StringLength(22)]
    [Unicode(false)]
    public string RFT_IHSGlobalPortId { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string RFT_IATACode { get; set; }

    [Column(TypeName = "bit")]
    public bool RFT_IsSea { get; set; }

    [Column(TypeName = "bit")]
    public bool RFT_IsAir { get; set; }

    [Column(TypeName = "bit")]
    public bool RFT_IsRail { get; set; }

    [Column(TypeName = "bit")]
    public bool RFT_IsRoad { get; set; }

    [Column(TypeName = "bit")]
    public bool RFT_IsInlandWaterway { get; set; }

    [Column(TypeName = "bit")]
    public bool RFT_ContainerAutomationAvailable { get; set; }

    [Column(TypeName = "bit")]
    public bool RFT_RequiresCredentials { get; set; }

    [Column(TypeName = "geography")]
    public Geometry RFT_GeoLocation { get; set; }
}
