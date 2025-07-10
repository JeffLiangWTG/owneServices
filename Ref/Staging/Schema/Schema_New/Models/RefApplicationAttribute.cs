using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefApplicationAttribute")]
[Index("RAA_ConfigFilePath", "RAA_AttributeName", Name = "IX_RefApplicationAttribute_RAA_ConfigFilePath_RAA_AttributeName", IsUnique = true)]
public partial class RefApplicationAttribute
{
    [Key]
    public Guid RAA_PK { get; set; }

    [Required]
    [StringLength(200)]
    [Unicode(false)]
    public string RAA_ConfigFilePath { get; set; }

    [Required]
    [StringLength(100)]
    public string RAA_AttributeName { get; set; }

    [Required]
    [StringLength(200)]
    public string RAA_Value { get; set; }

    [Required]
    [StringLength(10)]
    [Unicode(false)]
    public string RAA_RAT_NKType { get; set; }

    public byte[] RAA_Content { get; set; }

    [Required]
    [StringLength(150)]
    public string RAA_JobGroup { get; set; }

    public virtual RefApplicationAttributeType RAA_RAT_NKTypeNavigation { get; set; }
}
