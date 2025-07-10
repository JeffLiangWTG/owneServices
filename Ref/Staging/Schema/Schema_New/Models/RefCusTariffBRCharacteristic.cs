using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

[Table("RefCusTariffBRCharacteristic")]
public partial class RefCusTariffBRCharacteristic
{
    [Key]
    [Column(TypeName = "uniqueidentifier")]
    public Guid ZB1_PK { get; set; }

    [Required]
    [Column(TypeName = "varchar(5)")]
    [StringLength(5)]
    [Unicode(false)]
    public string ZB1_CharacteristicType { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid? ZB1_ZZ1_Tariff { get; set; }

    [Column(TypeName = "uniqueidentifier")]
    public Guid? ZB1_ZZ5_Nomenclature { get; set; }

    [Required]
    [Column(TypeName = "varchar(10)")]
    [StringLength(10)]
    [Unicode(false)]
    public string ZB1_Style { get; set; }

    [Column(TypeName = "smallint")]
    public short ZB1_MaxLength { get; set; }

    [Column(TypeName = "smallint")]
    public short ZB1_DecimalPlaces { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(35)")]
    [StringLength(35)]
    public string ZB1_Code { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(1000)")]
    [StringLength(1000)]
    public string ZB1_Text { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime ZB1_StartDate { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime ZB1_EndDate { get; set; }

    [Column(TypeName = "bit")]
    public bool ZB1_IsImport { get; set; }

    [Column(TypeName = "bit")]
    public bool ZB1_IsExport { get; set; }

    [Column(TypeName = "bit")]
    public bool ZB1_IsMandatory { get; set; }

    [Column(TypeName = "bit")]
    public bool ZB1_IsConditioningAttribute { get; set; }

    [InverseProperty("ZB3_ZB1_CharacteristicNavigation")]
    public virtual ICollection<RefCusTariffBRCharacteristicAttribute> RefCusTariffBRCharacteristicAttributes { get; set; } = new List<RefCusTariffBRCharacteristicAttribute>();

    [InverseProperty("ZB2_ZB1_CharacteristicNavigation")]
    public virtual ICollection<RefCusTariffBRCharacteristicValue> RefCusTariffBRCharacteristicValues { get; set; } = new List<RefCusTariffBRCharacteristicValue>();

    [ForeignKey("ZB1_ZZ1_Tariff")]
    [InverseProperty("RefCusTariffBRCharacteristics")]
    public virtual RefCusTariff ZB1_ZZ1_TariffNavigation { get; set; }

    [ForeignKey("ZB1_ZZ5_Nomenclature")]
    [InverseProperty("RefCusTariffBRCharacteristics")]
    public virtual RefCusNomenclatureGroup ZB1_ZZ5_NomenclatureNavigation { get; set; }
}
