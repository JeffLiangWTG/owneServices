using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusProfileQuestion")]
public partial class RefCusProfileQuestion
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid XQ2_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(10)")]
    [StringLength(10)]
    [Unicode(false)]
    public string XQ2_XXX_NKProfileType { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string XQ2_XXX_ZZZ_NKDataGrouping { get; set; }

    [Required]
    [Column(TypeName = "varchar(5)")]
    [StringLength(5)]
    [Unicode(false)]
    public string XQ2_XXX_ZZI_NKTariffType { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string XQ2_XXX_ZZI_ZZZ_NKDataGrouping { get; set; }

    [Required]
    [Column(TypeName = "varchar(35)")]
    [StringLength(35)]
    [Unicode(false)]
    public string XQ2_QuestionCode { get; set; }

    [Required]
    [Column(TypeName = "varchar(35)")]
    [StringLength(35)]
    [Unicode(false)]
    public string XQ2_AnswerDataType { get; set; }

    [Column(TypeName = "smallint")]
    public short XQ2_AnswerMaxLength { get; set; }

    [Column(TypeName = "smallint")]
    public short XQ2_AnswerDecimalPlaces { get; set; }

    [Required]
    [Column(TypeName = "varchar(50)")]
    [StringLength(50)]
    [Unicode(false)]
    public string XQ2_AnswerMask { get; set; }

    [Column(TypeName = "bit")]
    public bool XQ2_AllowMultipleAnswers { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(200)")]
    [StringLength(200)]
    public string XQ2_Name { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(1000)")]
    [StringLength(1000)]
    public string XQ2_Text { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(2000)")]
    [StringLength(2000)]
    public string XQ2_Note { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime XQ2_StartDate { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime XQ2_EndDate { get; set; }

    [Column(TypeName = "bit")]
    public bool XQ2_IsAnswerMandatory { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string XQ2_ZZZ_NKDataGrouping { get; set; }

    [InverseProperty("XQ4_XQ2_QuestionNavigation")]
    public virtual ICollection<RefCusProfileQuestionAnswerList> RefCusProfileQuestionAnswerLists { get; set; } = new List<RefCusProfileQuestionAnswerList>();

    [InverseProperty("XQ3_XQ2_QuestionNavigation")]
    public virtual ICollection<RefCusProfileQuestionAttribute> RefCusProfileQuestionAttributes { get; set; } = new List<RefCusProfileQuestionAttribute>();

    [InverseProperty("XQL_XQ2_QuestionNavigation")]
    public virtual ICollection<RefCusProfileQuestionLanguage> RefCusProfileQuestionLanguages { get; set; } = new List<RefCusProfileQuestionLanguage>();
}
