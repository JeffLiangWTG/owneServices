using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusTariffBRCharacteristicValue")]
[Index("ZB2_ZB1_Characteristic", Name = "IX_RefCusTariffBRCharacteristicValue_ZB2_ZB1_Characteristic")]
public partial class RefCusTariffBRCharacteristicValue
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZB2_PK { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid ZB2_ZB1_Characteristic { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(100)")]
    [StringLength(100)]
    public string ZB2_Value { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(500)")]
    [StringLength(500)]
    public string ZB2_Description { get; set; }

    [ForeignKey("ZB2_ZB1_Characteristic")]
    [InverseProperty("RefCusTariffBRCharacteristicValues")]
    public virtual RefCusTariffBRCharacteristic ZB2_ZB1_CharacteristicNavigation { get; set; }
}
