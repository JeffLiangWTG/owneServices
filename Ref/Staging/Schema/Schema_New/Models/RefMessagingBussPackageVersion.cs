using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefMessagingBussPackageVersion")]
[Index("ZMV_ZMP_PackageInfo", Name = "IX_RefMessagingBussPackageVersion_ZMV_ZMP_PackageInfo")]
public partial class RefMessagingBussPackageVersion
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZMV_PK { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid ZMV_ZMP_PackageInfo { get; set; }

    [Required]
    [Column(TypeName = "varchar(50)")]
    [StringLength(50)]
    [Unicode(false)]
    public string ZMV_Version { get; set; }

    [ForeignKey("ZMV_ZMP_PackageInfo")]
    [InverseProperty("RefMessagingBussPackageVersions")]
    public virtual RefMessagingBussPackageInfo ZMV_ZMP_PackageInfoNavigation { get; set; }
}
