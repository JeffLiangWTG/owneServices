using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusProfileQuestionPathway")]
public partial class RefCusProfileQuestionPathway
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid XQP_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(35)")]
    [StringLength(35)]
    [Unicode(false)]
    public string XQP_XQ2_NKQuestionParent { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime XQP_XQ2_NKQuestionStartDateParent { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string XQP_XQ2_ZZZ_NKDataGroupingParent { get; set; }

    [Required]
    [Column(TypeName = "varchar(35)")]
    [StringLength(35)]
    [Unicode(false)]
    public string XQP_XQ2_NKQuestionChild { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime XQP_XQ2_NKQuestionStartDateChild { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string XQP_XQ2_ZZZ_NKDataGroupingChild { get; set; }

    [Required]
    [Column(TypeName = "varchar(10)")]
    [StringLength(10)]
    [Unicode(false)]
    public string XQP_XQ2_XXX_NKProfileType { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string XQP_XQ2_XXX_ZZZ_NKDataGrouping { get; set; }

    [Required]
    [Column(TypeName = "varchar(5)")]
    [StringLength(5)]
    [Unicode(false)]
    public string XQP_XQ2_XXX_ZZI_NKTariffType { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string XQP_XQ2_XXX_ZZI_ZZZ_NKDataGrouping { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(200)")]
    [StringLength(200)]
    public string XQP_Description { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime XQP_StartDate { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime XQP_EndDate { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(500)")]
    [StringLength(500)]
    public string XQP_ConditionToProceedFormula { get; set; }

    [Column(TypeName = "bit")]
    public bool XQP_AllowMultipleAnswers { get; set; }

    [Column(TypeName = "bit")]
    public bool XQP_IsAnswerMandatory { get; set; }
}
