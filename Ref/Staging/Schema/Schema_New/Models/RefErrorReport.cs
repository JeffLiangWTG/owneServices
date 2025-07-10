using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefErrorReport")]
public partial class RefErrorReport
{
    [Key]
    public Guid RER_PK { get; set; }

    [Required]
    [Unicode(false)]
    public string RER_Exception { get; set; }

    [Required]
    [StringLength(3)]
    [Unicode(false)]
    public string RER_Status { get; set; }

    [Required]
    [StringLength(50)]
    [Unicode(false)]
    public string RER_JobName { get; set; }

    [Required]
    [StringLength(50)]
    [Unicode(false)]
    public string RER_Source { get; set; }

    public Guid? RER_SDA { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime RER_ExceptionTimeUtc { get; set; }

    [ForeignKey("RER_SDA")]
    [InverseProperty("RefErrorReports")]
    public virtual SourceData RER_SDANavigation { get; set; }
}
