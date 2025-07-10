using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusProfileQuestionAttribute")]
[Index("XQ3_XQ2_Question", "XQ3_Name", Name = "IX_RefCusProfileQuestionAttribute_XQ3_XQ2_Question_XQ3_Name")]
public partial class RefCusProfileQuestionAttribute
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid XQ3_PK { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid XQ3_XQ2_Question { get; set; }

    [Required]
    [Column(TypeName = "varchar(35)")]
    [StringLength(35)]
    [Unicode(false)]
    public string XQ3_Name { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string XQ3_Value { get; set; }

    [ForeignKey("XQ3_XQ2_Question")]
    [InverseProperty("RefCusProfileQuestionAttributes")]
    public virtual RefCusProfileQuestion XQ3_XQ2_QuestionNavigation { get; set; }
}
