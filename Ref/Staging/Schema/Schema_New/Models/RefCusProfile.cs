using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusProfile")]
public partial class RefCusProfile
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid XX0_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(10)")]
    [StringLength(10)]
    [Unicode(false)]
    public string XX0_XXX_NKProfileType { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string XX0_XXX_ZZZ_NKDataGrouping { get; set; }

    [Required]
    [Column(TypeName = "varchar(5)")]
    [StringLength(5)]
    [Unicode(false)]
    public string XX0_XXX_ZZI_NKTariffType { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string XX0_XXX_ZZI_ZZZ_NKDataGrouping { get; set; }

    [Required]
    [Column(TypeName = "varchar(35)")]
    [StringLength(35)]
    [Unicode(false)]
    public string XX0_TariffCode { get; set; }

    [Required]
    [Column(TypeName = "varchar(35)")]
    [StringLength(35)]
    [Unicode(false)]
    public string XX0_QuestionCode { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime XX0_StartDate { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime XX0_EndDate { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string XX0_ZZZ_NKDataGrouping { get; set; }

    [Column(TypeName = "bit")]
    public bool XX0_AllowMultipleAnswers { get; set; }

    [Column(TypeName = "bit")]
    public bool XX0_IsAnswerMandatory { get; set; }

    [InverseProperty("XXY_XX0_ProfileNavigation")]
    public virtual ICollection<RefCusProfileAttribute> RefCusProfileAttributes { get; set; } = new List<RefCusProfileAttribute>();
}
