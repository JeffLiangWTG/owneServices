using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TW.Business
{
	public class CusEntryInstructionDocument : CusCodeData
	{
		public CusEntryInstructionDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[MaxLength(35)]
		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryInstructionDocument|DocumentNumber", Caption = "Attached Document No.")]
		public override ZString CY_Data { get => base.CY_Data; set => base.CY_Data = value; }

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(CusEntryInstruction));

		protected override CusCodeDataValidation GetNewValidation()
		{
			return new CusEntryInstructionDocumentValidation(this);
		}

		public new CusEntryInstructionDocumentValidation Validation => (CusEntryInstructionDocumentValidation)base.Validation;

		public override bool CY_DataAllowWesternEuropeanCharactersOnly => false;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.AttachedDocumentNumber;
		}
	}
}
