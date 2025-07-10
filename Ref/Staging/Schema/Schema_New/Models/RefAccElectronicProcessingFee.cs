using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefAccElectronicProcessingFee")]
public partial class RefAccElectronicProcessingFee
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid EPF_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string EPF_SystemCode { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string EPF_Category { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string EPF_Code { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(250)")]
    [StringLength(250)]
    public string EPF_Description { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string EPF_Currency { get; set; }

    [Column(TypeName = "decimal(18, 6)")]
    public decimal EPF_Price { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime EPF_ValidFrom { get; set; }

    [Required]
    [Column(TypeName = "char(2)")]
    [StringLength(2)]
    [Unicode(false)]
    public string EPF_CountryCode { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string EPF_JobDirection { get; set; }
}
