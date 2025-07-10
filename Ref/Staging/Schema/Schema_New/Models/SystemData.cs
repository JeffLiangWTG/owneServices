using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

public partial class SystemData
{
    [Key]
    public Guid SD_PK { get; set; }

    [Required]
    [StringLength(50)]
    [Unicode(false)]
    public string SD_Name { get; set; }

    [Required]
    [Unicode(false)]
    public string SD_Value { get; set; }
}
