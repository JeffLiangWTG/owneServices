using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusTariffBRCharacteristicAttribute")]
[Index("ZB3_ZB1_Characteristic", Name = "IX_RefCusTariffBRCharacteristicAttribute_ZB3_ZB1_Characteristic")]
public partial class RefCusTariffBRCharacteristicAttribute
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZB3_PK { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid ZB3_ZB1_Characteristic { get; set; }

    [Required]
    [Column(TypeName = "varchar(35)")]
    [StringLength(35)]
    [Unicode(false)]
    public string ZB3_Name { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(10)")]
    [StringLength(10)]
    public string ZB3_Code { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string ZB3_Value { get; set; }

    [ForeignKey("ZB3_ZB1_Characteristic")]
    [InverseProperty("RefCusTariffBRCharacteristicAttributes")]
    public virtual RefCusTariffBRCharacteristic ZB3_ZB1_CharacteristicNavigation { get; set; }
}
