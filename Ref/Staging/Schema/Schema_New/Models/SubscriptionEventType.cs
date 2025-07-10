using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("SubscriptionEventType")]
public partial class SubscriptionEventType
{
    [Key]
    public Guid SST_PK { get; set; }

    [Required]
    [StringLength(5)]
    [Unicode(false)]
    public string SST_EventType { get; set; }
}
