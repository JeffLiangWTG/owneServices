using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefMessagingBussPackageInfo")]
public partial class RefMessagingBussPackageInfo
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZMP_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(200)")]
    [StringLength(200)]
    [Unicode(false)]
    public string ZMP_PackageName { get; set; }

    [InverseProperty("ZMC_ZMP_PackageInfoNavigation")]
    public virtual ICollection<RefMessagingBussCarrierInfo> RefMessagingBussCarrierInfos { get; set; } = new List<RefMessagingBussCarrierInfo>();

    [InverseProperty("ZMV_ZMP_PackageInfoNavigation")]
    public virtual ICollection<RefMessagingBussPackageVersion> RefMessagingBussPackageVersions { get; set; } = new List<RefMessagingBussPackageVersion>();
}
