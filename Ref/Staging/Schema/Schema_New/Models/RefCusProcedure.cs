using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusProcedure")]
public partial class RefCusProcedure
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZZ6_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZZ6_Category { get; set; }

    [Required]
    [Column(TypeName = "varchar(5)")]
    [StringLength(5)]
    [Unicode(false)]
    public string ZZ6_ProcedureCode { get; set; }

    [Required]
    [Column(TypeName = "varchar(5)")]
    [StringLength(5)]
    [Unicode(false)]
    public string ZZ6_PreviousProcedureCode { get; set; }

    [Required]
    [Column(TypeName = "varchar(50)")]
    [StringLength(50)]
    [Unicode(false)]
    public string ZZ6_Concession { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string ZZ6_Description { get; set; }

    [Required]
    [Column(TypeName = "varchar(3)")]
    [StringLength(3)]
    [Unicode(false)]
    public string ZZ6_ZZZ_NKDataGrouping { get; set; }

    [Required]
    [Column(TypeName = "varchar(50)")]
    [StringLength(50)]
    [Unicode(false)]
    public string ZZ6_ShipmentType { get; set; }

    [Required]
    [Column(TypeName = "bit")]
    public bool? ZZ6_CalculateDuty { get; set; }

    [Required]
    [Column(TypeName = "varchar(200)")]
    [StringLength(200)]
    [Unicode(false)]
    public string ZZ6_Group { get; set; }

    [Column(TypeName = "bit")]
    public bool ZZ6_LandedCost { get; set; }

    [Required]
    [Column(TypeName = "char(1)")]
    [StringLength(1)]
    [Unicode(false)]
    public string ZZ6_IntoWarehouse { get; set; }

    [Required]
    [Column(TypeName = "char(1)")]
    [StringLength(1)]
    [Unicode(false)]
    public string ZZ6_OutOfWarehouse { get; set; }

    [Required]
    [Column(TypeName = "char(1)")]
    [StringLength(1)]
    [Unicode(false)]
    public string ZZ6_IntoInwardProcessing { get; set; }

    [Required]
    [Column(TypeName = "char(1)")]
    [StringLength(1)]
    [Unicode(false)]
    public string ZZ6_OutOfInwardProcessing { get; set; }

    [Required]
    [Column(TypeName = "char(1)")]
    [StringLength(1)]
    [Unicode(false)]
    public string ZZ6_IntoOutwardProcessing { get; set; }

    [Required]
    [Column(TypeName = "char(1)")]
    [StringLength(1)]
    [Unicode(false)]
    public string ZZ6_OutofOutwardProcessing { get; set; }

    [Required]
    [Column(TypeName = "char(1)")]
    [StringLength(1)]
    [Unicode(false)]
    public string ZZ6_IntoTemporaryImport { get; set; }

    [Required]
    [Column(TypeName = "char(1)")]
    [StringLength(1)]
    [Unicode(false)]
    public string ZZ6_OutOfTemporaryImport { get; set; }

    [Required]
    [Column(TypeName = "char(1)")]
    [StringLength(1)]
    [Unicode(false)]
    public string ZZ6_IntoTemporaryExport { get; set; }

    [Required]
    [Column(TypeName = "char(1)")]
    [StringLength(1)]
    [Unicode(false)]
    public string ZZ6_OutOfTemporaryExport { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime ZZ6_StartDate { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime ZZ6_EndDate { get; set; }

    [Required]
    [Column(TypeName = "bit")]
    public bool? ZZ6_CalculateVAT { get; set; }

    [Required]
    [Column(TypeName = "char(1)")]
    [StringLength(1)]
    [Unicode(false)]
    public string ZZ6_IsGuaranteeConsumed { get; set; }

    [Required]
    [Column(TypeName = "char(1)")]
    [StringLength(1)]
    [Unicode(false)]
    public string ZZ6_IsGuaranteeReleased { get; set; }

    [Required]
    [Column(TypeName = "char(1)")]
    [StringLength(1)]
    [Unicode(false)]
    public string ZZ6_IsTransit { get; set; }

    [Required]
    [Column(TypeName = "char(1)")]
    [StringLength(1)]
    [Unicode(false)]
    public string ZZ6_IntoVATWarehouse { get; set; }

    [Required]
    [Column(TypeName = "char(1)")]
    [StringLength(1)]
    [Unicode(false)]
    public string ZZ6_OutOfVATWarehouse { get; set; }

    [InverseProperty("ZXB_ZZ6_ProcedureCodeNavigation")]
    public virtual ICollection<RefCusProcedureAttribute> RefCusProcedureAttributes { get; set; } = new List<RefCusProcedureAttribute>();

    [InverseProperty("ZXV_ZZ6_ProcedureNavigation")]
    public virtual ICollection<RefCusProcedureLanguage> RefCusProcedureLanguages { get; set; } = new List<RefCusProcedureLanguage>();
}
