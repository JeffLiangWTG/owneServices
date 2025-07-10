using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefComplianceCommodityAlert")]
public partial class RefComplianceCommodityAlert
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid RCR_PK { get; set; }

    [Required]
    [Column(TypeName = "bit")]
    public bool? RCR_IsActive { get; set; }

    [Required]
    [Column(TypeName = "varchar(200)")]
    [StringLength(200)]
    [Unicode(false)]
    public string RCR_AlertCode { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string RCR_AlertDescription { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(200)")]
    [StringLength(200)]
    public string RCR_AlertName { get; set; }

    [Required]
    [Column(TypeName = "char(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string RCR_AlertType { get; set; }

    [Required]
    [Column(TypeName = "char(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string RCR_CountryRegion { get; set; }

    [Column(TypeName = "smallint")]
    public short RCR_PublishYear { get; set; }

    [Required]
    [Column(TypeName = "varchar(2048)")]
    [StringLength(2048)]
    [Unicode(false)]
    public string RCR_SourceURL { get; set; }

    [Required]
    [Column(TypeName = "char(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string RCR_TradeDirection { get; set; }
}
