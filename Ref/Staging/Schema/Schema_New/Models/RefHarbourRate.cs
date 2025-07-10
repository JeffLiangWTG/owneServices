using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefHarbourRate")]
public partial class RefHarbourRate
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZXF_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZXF_Type { get; set; }

    [Required]
    [Column(TypeName = "varchar(5)")]
    [StringLength(5)]
    [Unicode(false)]
    public string ZXF_Port { get; set; }

    [Required]
    [Column(TypeName = "varchar(4)")]
    [StringLength(4)]
    [Unicode(false)]
    public string ZXF_Mode { get; set; }

    [Required]
    [Column(TypeName = "varchar(4)")]
    [StringLength(4)]
    [Unicode(false)]
    public string ZXF_Commodity { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZXF_PortTaxType { get; set; }

    [Column(TypeName = "date")]
    public DateTime ZXF_StartDate { get; set; }

    [Column(TypeName = "date")]
    public DateTime ZXF_EndDate { get; set; }

    [Required]
    [Column(TypeName = "varchar(300)")]
    [StringLength(300)]
    [Unicode(false)]
    public string ZXF_RateFormula { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZXF_ZZZ_NKDataGrouping { get; set; }
}
