using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("SubscriptionNotificationQueue")]
public partial class SubscriptionNotificationQueue
{
    [Key]
    public Guid SNQ_PK { get; set; }

    [Required]
    [StringLength(9)]
    [Unicode(false)]
    public string SNQ_ClientId { get; set; }

    [Required]
    [StringLength(3)]
    [Unicode(false)]
    public string SNQ_Status { get; set; }

    [Required]
    [Column(TypeName = "xml")]
    public string SNQ_Message { get; set; }
}
