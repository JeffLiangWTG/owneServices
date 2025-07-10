using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusProfileQuestionAnswerList")]
[Index("XQ4_XQ2_Question", "XQ4_Value", Name = "IX_RefCusProfileQuestionAnswerList_XQ4_XQ2_Question_XQ4_Value")]
public partial class RefCusProfileQuestionAnswerList
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid XQ4_PK { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid XQ4_XQ2_Question { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(100)")]
    [StringLength(100)]
    public string XQ4_Value { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(500)")]
    [StringLength(500)]
    public string XQ4_Description { get; set; }

    [InverseProperty("XAL_XQ4_QuestionAnswerNavigation")]
    public virtual ICollection<RefCusProfileQuestionAnswerListLanguage> RefCusProfileQuestionAnswerListLanguages { get; set; } = new List<RefCusProfileQuestionAnswerListLanguage>();

    [ForeignKey("XQ4_XQ2_Question")]
    [InverseProperty("RefCusProfileQuestionAnswerLists")]
    public virtual RefCusProfileQuestion XQ4_XQ2_QuestionNavigation { get; set; }
}
