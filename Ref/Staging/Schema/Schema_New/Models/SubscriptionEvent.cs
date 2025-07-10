using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("SubscriptionEvent")]
public partial class SubscriptionEvent
{
    [Key]
    public Guid SSV_PK { get; set; }

    [Required]
    [StringLength(5)]
    [Unicode(false)]
    public string SSV_SST_NKEventType { get; set; }

    [Required]
    [StringLength(3)]
    [Unicode(false)]
    public string SSV_DataSetCode { get; set; }

    [Required]
    [StringLength(2)]
    [Unicode(false)]
    public string SSV_Country { get; set; }

    [Required]
    [StringLength(3)]
    [Unicode(false)]
    public string SSV_ClientGroupCountry { get; set; }

    public bool SSV_IsActive { get; set; }

    public DateTime? SSV_LastUpdatedTime { get; set; }
}
