using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefAirline")]
public partial class RefAirline
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid RM_PK { get; set; }

    [Required]
    [Column(TypeName = "bit")]
    public bool? RM_IsActive { get; set; }

    [Required]
    [Column(TypeName = "varchar(40)")]
    [StringLength(40)]
    [Unicode(false)]
    public string RM_AirlineName1 { get; set; }

    [Required]
    [Column(TypeName = "varchar(40)")]
    [StringLength(40)]
    [Unicode(false)]
    public string RM_AirlineName2 { get; set; }

    [Required]
    [Column(TypeName = "varchar(4)")]
    [StringLength(4)]
    [Unicode(false)]
    public string RM_AccountingCode { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string RM_ThreeLetterCode { get; set; }

    [Required]
    [Column(TypeName = "varchar(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string RM_TwoCharacterCode { get; set; }

    [Column(TypeName = "bit")]
    public bool RM_DuplicateFlagIndicator { get; set; }

    [Required]
    [Column(TypeName = "varchar(40)")]
    [StringLength(40)]
    [Unicode(false)]
    public string RM_AddressLine1 { get; set; }

    [Required]
    [Column(TypeName = "varchar(40)")]
    [StringLength(40)]
    [Unicode(false)]
    public string RM_AddressLine2 { get; set; }

    [Required]
    [Column(TypeName = "varchar(25)")]
    [StringLength(25)]
    [Unicode(false)]
    public string RM_AirlineCity { get; set; }

    [Required]
    [Column(TypeName = "varchar(20)")]
    [StringLength(20)]
    [Unicode(false)]
    public string RM_AirlineState { get; set; }

    [Required]
    [Column(TypeName = "varchar(44)")]
    [StringLength(44)]
    [Unicode(false)]
    public string RM_AirlineCountry { get; set; }

    [Required]
    [Column(TypeName = "varchar(10)")]
    [StringLength(10)]
    [Unicode(false)]
    public string RM_AirlinePostalCode { get; set; }

    [Required]
    [Column(TypeName = "varchar(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string RM_RN_NKAirlineCountry { get; set; }

    [Required]
    [Column(TypeName = "varchar(8)")]
    [StringLength(8)]
    [Unicode(false)]
    public string RM_ReservationsDeptTeletype { get; set; }

    [Required]
    [Column(TypeName = "varchar(20)")]
    [StringLength(20)]
    [Unicode(false)]
    public string RM_ReservationsContactName { get; set; }

    [Required]
    [Column(TypeName = "varchar(20)")]
    [StringLength(20)]
    [Unicode(false)]
    public string RM_ReservationsContactTitle { get; set; }

    [Required]
    [Column(TypeName = "varchar(8)")]
    [StringLength(8)]
    [Unicode(false)]
    public string RM_ReservationsContactTeletype { get; set; }

    [Required]
    [Column(TypeName = "varchar(8)")]
    [StringLength(8)]
    [Unicode(false)]
    public string RM_EmergencyTeletype { get; set; }

    [Required]
    [Column(TypeName = "varchar(20)")]
    [StringLength(20)]
    [Unicode(false)]
    public string RM_EmergencyContactName { get; set; }

    [Required]
    [Column(TypeName = "varchar(20)")]
    [StringLength(20)]
    [Unicode(false)]
    public string RM_EmergencyContactTitle { get; set; }

    [Column(TypeName = "bit")]
    public bool RM_MembershipFlagSITA { get; set; }

    [Column(TypeName = "bit")]
    public bool RM_MembershipFlagARINC { get; set; }

    [Column(TypeName = "bit")]
    public bool RM_MembershipFlagIATA { get; set; }

    [Column(TypeName = "bit")]
    public bool RM_MembershipFlagATA { get; set; }

    [Required]
    [Column(TypeName = "varchar(1)")]
    [StringLength(1)]
    [Unicode(false)]
    public string RM_TypeOfOperationsCode { get; set; }

    [Required]
    [Column(TypeName = "varchar(1)")]
    [StringLength(1)]
    [Unicode(false)]
    public string RM_AccountingSecondaryFlag { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string RM_AirlinePrefix { get; set; }

    [Required]
    [Column(TypeName = "varchar(1)")]
    [StringLength(1)]
    [Unicode(false)]
    public string RM_AirlinePrefixSecondaryFlag { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string RM_EagleAddedAirlinePrefixOrAccountingCode { get; set; }

    [Required]
    [Column(TypeName = "varchar(35)")]
    [StringLength(35)]
    [Unicode(false)]
    public string RM_LabelShortName { get; set; }

    [Column(TypeName = "bit")]
    public bool RM_IsCASSControlled { get; set; }

    [Required]
    [Column(TypeName = "varchar(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string RM_ContactNameOCIIdentifier { get; set; }

    [Required]
    [Column(TypeName = "varchar(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string RM_ContactPhoneOCIIdentifier { get; set; }

    [Column(TypeName = "varbinary(max)")]
    public byte[] RM_AirlineLogo { get; set; }
}
