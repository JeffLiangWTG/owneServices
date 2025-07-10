using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefMessagingBussCarrierInfo")]
[Index("ZMC_ZMP_PackageInfo", Name = "IX_RefMessagingBussCarrierInfo_ZMC_ZMP_PackageInfo")]
public partial class RefMessagingBussCarrierInfo
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZMC_PK { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid ZMC_ZMP_PackageInfo { get; set; }

    [Required]
    [Column(TypeName = "char(5)")]
    [StringLength(5)]
    [Unicode(false)]
    public string ZMC_CarrierCode { get; set; }

    [Required]
    [Column(TypeName = "varchar(200)")]
    [StringLength(200)]
    [Unicode(false)]
    public string ZMC_CarrierName { get; set; }

    [Column(TypeName = "varchar(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string ZMC_CountryCode { get; set; }

    [ForeignKey("ZMC_ZMP_PackageInfo")]
    [InverseProperty("RefMessagingBussCarrierInfos")]
    public virtual RefMessagingBussPackageInfo ZMC_ZMP_PackageInfoNavigation { get; set; }
}
