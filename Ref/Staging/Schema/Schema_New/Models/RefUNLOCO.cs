using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefUNLOCO")]
public partial class RefUNLOCO
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid RL_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(5)")]
    [StringLength(5)]
    [Unicode(false)]
    public string RL_Code { get; set; }

    [Required]
    [Column(TypeName = "bit")]
    public bool? RL_IsActive { get; set; }

    [Required]
    [Column(TypeName = "varchar(35)")]
    [StringLength(35)]
    [Unicode(false)]
    public string RL_PortName { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(35)")]
    [StringLength(35)]
    public string RL_NameWithDiacriticals { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string RL_IATA { get; set; }

    [Required]
    [Column(TypeName = "varchar(12)")]
    [StringLength(12)]
    [Unicode(false)]
    public string RL_CoOrdinates { get; set; }

    [Column(TypeName = "bit")]
    public bool RL_HasAirport { get; set; }

    [Column(TypeName = "bit")]
    public bool RL_HasSeaport { get; set; }

    [Column(TypeName = "bit")]
    public bool RL_HasRail { get; set; }

    [Column(TypeName = "bit")]
    public bool RL_HasRoad { get; set; }

    [Column(TypeName = "bit")]
    public bool RL_HasPost { get; set; }

    [Column(TypeName = "bit")]
    public bool RL_HasCustomsLodge { get; set; }

    [Column(TypeName = "bit")]
    public bool RL_HasUnload { get; set; }

    [Column(TypeName = "bit")]
    public bool RL_HasStore { get; set; }

    [Column(TypeName = "bit")]
    public bool RL_HasTerminal { get; set; }

    [Column(TypeName = "bit")]
    public bool RL_HasDischarge { get; set; }

    [Column(TypeName = "bit")]
    public bool RL_HasOutport { get; set; }

    [Column(TypeName = "bit")]
    public bool RL_HasBorderCrossing { get; set; }

    [Column(TypeName = "varchar(35)")]
    [StringLength(35)]
    [Unicode(false)]
    public string RL_R3_NKTimeZoneSetName { get; set; }

    [Required]
    [Column(TypeName = "varchar(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string RL_RN_NKCountryCode { get; set; }

    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string RL_RW_NKCode { get; set; }

    [Column(TypeName = "char(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string RL_RW_RN_NKCountryCode { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string RL_IATARegionCode { get; set; }

    [Column(TypeName = "geography")]
    public Geometry RL_GeoLocation { get; set; }

    [Column(TypeName = "bit")]
    public bool RL_UserOverride { get; set; }
}
