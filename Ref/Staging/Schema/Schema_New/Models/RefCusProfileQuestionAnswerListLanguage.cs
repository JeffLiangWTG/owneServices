using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusProfileQuestionAnswerListLanguage")]
[Index("XAL_XQ4_QuestionAnswer", "XAL_ZX6_NKLanguage", Name = "IX_RefCusProfileQuestionAnswerListLanguage_XAL_XQ4_QuestionAnswer_XAL_ZX6_NKLanguage")]
public partial class RefCusProfileQuestionAnswerListLanguage
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid XAL_PK { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid XAL_XQ4_QuestionAnswer { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(500)")]
    [StringLength(500)]
    public string XAL_Description { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string XAL_ZX6_NKLanguage { get; set; }

    [ForeignKey("XAL_XQ4_QuestionAnswer")]
    [InverseProperty("RefCusProfileQuestionAnswerListLanguages")]
    public virtual RefCusProfileQuestionAnswerList XAL_XQ4_QuestionAnswerNavigation { get; set; }
}
