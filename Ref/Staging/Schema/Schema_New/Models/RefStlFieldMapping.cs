using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefStlFieldMapping")]
public partial class RefStlFieldMapping
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid SFM_PK { get; set; }

    [Required]
    [Column(TypeName = "char(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string SFM_FeatureCode { get; set; }

    [Column(TypeName = "varchar(255)")]
    [StringLength(255)]
    [Unicode(false)]
    public string SFM_BillableCount { get; set; }

    [Column(TypeName = "varchar(255)")]
    [StringLength(255)]
    [Unicode(false)]
    public string SFM_Reference1 { get; set; }

    [Column(TypeName = "varchar(255)")]
    [StringLength(255)]
    [Unicode(false)]
    public string SFM_Reference2 { get; set; }

    [Column(TypeName = "varchar(255)")]
    [StringLength(255)]
    [Unicode(false)]
    public string SFM_Reference3 { get; set; }

    [Column(TypeName = "varchar(255)")]
    [StringLength(255)]
    [Unicode(false)]
    public string SFM_Reference4 { get; set; }

    [Column(TypeName = "varchar(255)")]
    [StringLength(255)]
    [Unicode(false)]
    public string SFM_Reference5 { get; set; }

    [Column(TypeName = "varchar(255)")]
    [StringLength(255)]
    [Unicode(false)]
    public string SFM_Category { get; set; }

    [Column(TypeName = "varchar(255)")]
    [StringLength(255)]
    [Unicode(false)]
    public string SFM_PriceItemCode { get; set; }

    [Column(TypeName = "varchar(255)")]
    [StringLength(255)]
    [Unicode(false)]
    public string SFM_ServiceOccuredUTC { get; set; }

    [Column(TypeName = "varchar(255)")]
    [StringLength(255)]
    [Unicode(false)]
    public string SFM_ClientStaffCode { get; set; }
}
