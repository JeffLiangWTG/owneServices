using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefComplianceList")]
public partial class RefComplianceList
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid RCL_PK { get; set; }

    [Required]
    [Column(TypeName = "bit")]
    public bool? RCL_IsActive { get; set; }

    [Required]
    [Column(TypeName = "varchar(30)")]
    [StringLength(30)]
    [Unicode(false)]
    public string RCL_ListCode { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(120)")]
    [StringLength(120)]
    public string RCL_ListName { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string RCL_ListDescription { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(120)")]
    [StringLength(120)]
    public string RCL_ListPublisher { get; set; }

    [Required]
    [Column(TypeName = "varchar(30)")]
    [StringLength(30)]
    [Unicode(false)]
    public string RCL_ListType { get; set; }

    [Required]
    [Column(TypeName = "varchar(30)")]
    [StringLength(30)]
    [Unicode(false)]
    public string RCL_PublisherJurisdiction { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string RCL_PublisherDescription { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(254)")]
    [StringLength(254)]
    public string RCL_MainSourceURL { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(254)")]
    [StringLength(254)]
    public string RCL_SecondarySourceURL { get; set; }

    [Column(TypeName = "date")]
    public DateTime RCL_LastUpdatedDate { get; set; }

    [Column(TypeName = "date")]
    public DateTime? RCL_IntegrationDate { get; set; }
}
