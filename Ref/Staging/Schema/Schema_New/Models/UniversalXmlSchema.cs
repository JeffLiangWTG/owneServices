using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("UniversalXmlSchema")]
[Index("XSD_SchemaName", "XSD_SchemaVersion", Name = "IX_XSD_SchemaName_XSD_SchemaVersion", IsUnique = true)]
public partial class UniversalXmlSchema
{
    [Key]
    public Guid XSD_PK { get; set; }

    [Required]
    [StringLength(255)]
    [Unicode(false)]
    public string XSD_SchemaName { get; set; }

    public int XSD_SchemaVersion { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime XSD_TimeStamp { get; set; }

    [Required]
    [StringLength(32)]
    [Unicode(false)]
    public string XSD_SchemaMD5HashCode { get; set; }

    [Required]
    [Column(TypeName = "xml")]
    public string XSD_SchemaContent { get; set; }
}
