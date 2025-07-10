using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefVesselArrival")]
[Index("ZYA_ZZO_Vessel", Name = "IX_RefVesselArrival_ZYA_ZZO_Vessel")]
public partial class RefVesselArrival
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZYA_PK { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid ZYA_ZZO_Vessel { get; set; }

    [Required]
    [Column(TypeName = "varchar(10)")]
    [StringLength(10)]
    [Unicode(false)]
    public string ZYA_VoyageNumber { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime ZYA_ArrivalDate { get; set; }

    [Required]
    [Column(TypeName = "varchar(5)")]
    [StringLength(5)]
    [Unicode(false)]
    public string ZYA_ArrivalPort { get; set; }

    [ForeignKey("ZYA_ZZO_Vessel")]
    [InverseProperty("RefVesselArrivals")]
    public virtual RefVesselZZ ZYA_ZZO_VesselNavigation { get; set; }
}
