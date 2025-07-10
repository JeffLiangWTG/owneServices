using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

public partial class RefCusQuota
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZXQ_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(6)")]
    [StringLength(6)]
    [Unicode(false)]
    public string ZXQ_OrderNumber { get; set; }

    [Column(TypeName = "decimal(19, 5)")]
    public decimal ZXQ_InitialAmount { get; set; }

    [Required]
    [Column(TypeName = "varchar(6)")]
    [StringLength(6)]
    [Unicode(false)]
    public string ZXQ_UnitOfMeasure { get; set; }

    [Column(TypeName = "decimal(19, 5)")]
    public decimal ZXQ_Balance { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime ZXQ_StartDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime ZXQ_EndDate { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZXQ_ZZZ_NKDataGrouping { get; set; }
}
