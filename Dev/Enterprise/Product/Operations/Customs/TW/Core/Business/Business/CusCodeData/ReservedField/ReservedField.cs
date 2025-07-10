using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TW.Business
{
	public abstract class ReservedField : CusCodeData
	{
		protected ReservedField(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[MaxLength(1)]
		[ResourceStringData("Enterprise.Customs.TW.Business.ReservedField|CY_Code", Caption = "Code", FullDescription = "The type code of the reserved field.")]
		public override ZString CY_Code { get => base.CY_Code; set => base.CY_Code = value; }

		[MaxLength(35)]
		[ResourceStringData("Enterprise.Customs.TW.Business.ReservedField|CY_Data", Caption = "Value", FullDescription = "The fields reserved for future use.")]
		public override ZString CY_Data { get => base.CY_Data; set => base.CY_Data = value; }

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.ReservedField;
		}

		protected override CusCodeDataValidation GetNewValidation()
		{
			return new ReservedFieldValidation(this);
		}

		public override bool CY_DataAllowWesternEuropeanCharactersOnly => false;
	}

	public interface IReservedFieldSupporter
	{
		IEnumerable<ReservedField> GetReservedFields();
	}
}
