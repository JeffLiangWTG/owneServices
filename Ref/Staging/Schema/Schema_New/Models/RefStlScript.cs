using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefStlScript")]
public partial class RefStlScript
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid STL_PK { get; set; }

    [Required]
    [Column(TypeName = "char(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string STL_FeatureCode { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(50)")]
    [StringLength(50)]
    public string STL_RoleName { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(50)")]
    [StringLength(50)]
    public string STL_ModuleName { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(50)")]
    [StringLength(50)]
    public string STL_FunctionName { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(75)")]
    [StringLength(75)]
    public string STL_FeatureName { get; set; }

    [Required]
    [Column(TypeName = "char(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string STL_DataGranularity { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(1000)")]
    [StringLength(1000)]
    public string STL_CompanyCode { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(1000)")]
    [StringLength(1000)]
    public string STL_BranchCode { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(1000)")]
    [StringLength(1000)]
    public string STL_TransactionDateUtc { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(1000)")]
    [StringLength(1000)]
    public string STL_CreatingUserCode { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(1000)")]
    [StringLength(1000)]
    public string STL_GuidReference { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(1000)")]
    [StringLength(1000)]
    public string STL_BillingReference1 { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(1000)")]
    [StringLength(1000)]
    public string STL_BillingReference2 { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(1000)")]
    [StringLength(1000)]
    public string STL_BillingReference3 { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(1000)")]
    [StringLength(1000)]
    public string STL_BillingReference4 { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(1000)")]
    [StringLength(1000)]
    public string STL_AdditionalRefs { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(1000)")]
    [StringLength(1000)]
    public string STL_TransactionCount { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string STL_PreparationScript { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string STL_FromClause { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string STL_WhereClause { get; set; }

    [Column(TypeName = "bit")]
    public bool STL_WithOptionRecompile { get; set; }

    [Required]
    [Column(TypeName = "bit")]
    public bool? STL_UsedInBilling { get; set; }

    [Required]
    [Column(TypeName = "char(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string STL_ActiveOn { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(20)")]
    [StringLength(20)]
    public string STL_MinCW1Version { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(20)")]
    [StringLength(20)]
    public string STL_MaxCW1Version { get; set; }

    [Required]
    [Column(TypeName = "char(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string STL_DateType { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? STL_CollectionStartDateUtc { get; set; }
}
