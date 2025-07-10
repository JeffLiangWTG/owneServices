using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefClient")]
public partial class RefClient
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid RCT_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(255)")]
    [StringLength(255)]
    [Unicode(false)]
    public string RCT_ClientID { get; set; }

    [Required]
    [Column(TypeName = "varbinary(max)")]
    public byte[] RCT_Certificate { get; set; }

    [Column(TypeName = "varbinary(max)")]
    public byte[] RCT_LegacyCertificate { get; set; }

    [Required]
    [Column(TypeName = "varbinary(max)")]
    public byte[] RCT_Signature { get; set; }
}
