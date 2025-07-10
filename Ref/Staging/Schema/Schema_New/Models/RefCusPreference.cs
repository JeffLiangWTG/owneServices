using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusPreference")]
public partial class RefCusPreference
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZZS_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(10)")]
    [StringLength(10)]
    [Unicode(false)]
    public string ZZS_Preference { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(500)")]
    [StringLength(500)]
    public string ZZS_Description { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZZS_ZZZ_NKDataGrouping { get; set; }

    [InverseProperty("ZX9_ZZS_PreferenceNavigation")]
    public virtual ICollection<RefCusPreferenceLanguage> RefCusPreferenceLanguages { get; set; } = new List<RefCusPreferenceLanguage>();
}
