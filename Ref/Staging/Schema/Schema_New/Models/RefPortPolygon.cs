using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefPortPolygon")]
public partial class RefPortPolygon
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid RPP_PK { get; set; }

    [Column(TypeName = "int")]
    public int RPP_PortId { get; set; }

    [Column(TypeName = "geography")]
    public Geometry RPP_SerializedPolygon { get; set; }
}
