using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusProfileQuestionLanguage")]
[Index("XQL_XQ2_Question", "XQL_Name", "XQL_ZX6_NKLanguage", Name = "IX_RefCusProfileQuestionLanguage_XQL_XQ2_Question_XQL_Name_XQL_ZX6_NKLanguage")]
public partial class RefCusProfileQuestionLanguage
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid XQL_PK { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid XQL_XQ2_Question { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(500)")]
    [StringLength(500)]
    public string XQL_Name { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(1000)")]
    [StringLength(1000)]
    public string XQL_Text { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(2000)")]
    [StringLength(2000)]
    public string XQL_Note { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string XQL_ZX6_NKLanguage { get; set; }

    [ForeignKey("XQL_XQ2_Question")]
    [InverseProperty("RefCusProfileQuestionLanguages")]
    public virtual RefCusProfileQuestion XQL_XQ2_QuestionNavigation { get; set; }
}
