using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusPreferenceLanguage")]
[Index("ZX9_ZZS_Preference", Name = "IX_RefCusPreferenceLanguage_ZX9_ZZS_Preference")]
public partial class RefCusPreferenceLanguage
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZX9_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZX9_ZX6_NKLanguage { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid ZX9_ZZS_Preference { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string ZX9_Description { get; set; }

    [ForeignKey("ZX9_ZZS_Preference")]
    [InverseProperty("RefCusPreferenceLanguages")]
    public virtual RefCusPreference ZX9_ZZS_PreferenceNavigation { get; set; }
}
