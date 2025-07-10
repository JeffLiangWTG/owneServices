using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Index("Name", "Type", Name = "IX_ProcessData_Name_Type", IsUnique = true)]
public partial class ProcessData
{
    [Key]
    public Guid ID { get; set; }

    [Required]
    [StringLength(200)]
    [Unicode(false)]
    public string Name { get; set; }

    [Required]
    [StringLength(200)]
    [Unicode(false)]
    public string Type { get; set; }

    [Unicode(false)]
    public string Data { get; set; }
}
