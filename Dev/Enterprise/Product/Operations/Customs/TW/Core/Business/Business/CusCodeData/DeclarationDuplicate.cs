using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Business
{
	public class DeclarationDuplicate : CusCodeData
	{
		public DeclarationDuplicate(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : CusCodeData.Schema
		{
			public const string Copy = "Copy";
		}

		[ResourceStringData("DeclarationDuplicate|Copy", Caption = "Copy", FullDescription = "The number of the declaration copies requested.")]
		public ZInt Copy { get => ZInt.ParseSafe(CY_Data, ZInt.Zero); set => CY_Data = value.ToString(); }

		public ZWrappedPropertyInfo CopyInfo => GetWrappedZPropertyInfo(Schema.Copy, x => CY_DataInfo);

		[ResourceStringData("DeclarationDuplicate|CY_Data", Caption = "Copy")]
		public override ZString CY_Data { get => base.CY_Data; set => base.CY_Data = value; }

		[ResourceStringData("DeclarationDuplicate|CY_Code", Caption = "Type", FullDescription = "The type of the declaration document copy requested.")]
		public override ZString CY_Code { get => base.CY_Code; set => base.CY_Code = value; }

		[ResourceStringData("DeclarationDuplicate|Description", Caption = "Description")]
		public override ZString Description => base.Description;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_ParentTableCode = CusEntryInstructionSchema.Constants.Prefix;
			CY_Type = CusCodeDataTypeList.Codes.DeclarationDuplicate;
			Copy = 1;
		}

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(CusEntryInstruction));

		protected override CusCodeDataValidation GetNewValidation()
		{
			return new DeclarationDuplicateValidation(this);
		}

		public new DeclarationDuplicateLookups Lookups => (DeclarationDuplicateLookups)base.Lookups;

		protected override CusCodeDataLookups GetNewLookups()
		{
			return new DeclarationDuplicateLookups(this);
		}

		public new DeclarationDuplicateValidation Validation => (DeclarationDuplicateValidation)base.Validation;

		public new CusEntryInstruction Parent => (CusEntryInstruction)base.Parent;
	}
}
