using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("DataChangeCapture")]
public partial class DataChangeCapture
{
    [Key]
    public Guid DCC_PK { get; set; }

    [Required]
    [StringLength(25)]
    [Unicode(false)]
    public string DCC_Column { get; set; }

    public Guid DCC_ParentPK { get; set; }

    [Required]
    [StringLength(3)]
    [Unicode(false)]
    public string DCC_ParentCode { get; set; }

    [Unicode(false)]
    public string DCC_OldValue { get; set; }

    [Required]
    [Unicode(false)]
    public string DCC_NewValue { get; set; }

    public DateTime DCC_EventTimeUTC { get; set; }
}
