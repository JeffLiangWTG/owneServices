using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TR.Manifest.Business
{
	public class RelatedDeclarationForExport : CusSupportingInfo
	{
		public RelatedDeclarationForExport(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoCusSupportingInfo.Schema
		{
			public const int ReferenceNumberMaxLength = 20;
			public const int SubTypeMaxLength = 3;
		}

		public const string RelatedDeclarationForExportType = "BIL";

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_Type = RelatedDeclarationForExportType;
			CSI_SubType = SubTypeList.Codes.No;
		}

		protected override CusSupportingInfoValidation GetNewValidation() => new RelatedDeclarationForExportValidation(this);
		public new RelatedDeclarationForExportValidation Validation => (RelatedDeclarationForExportValidation)base.Validation;

		public new RelatedDeclarationForExportLookups Lookups => (RelatedDeclarationForExportLookups)base.Lookups;
		protected override CusSupportingInfoLookups GetNewLookups() => new RelatedDeclarationForExportLookups(this);

		[MaxLength(Schema.ReferenceNumberMaxLength)]
		public override ZString CSI_ReferenceNumber { get => base.CSI_ReferenceNumber; set => base.CSI_ReferenceNumber = value; }

		[MaxLength(Schema.SubTypeMaxLength)]
		[ResourceStringData("TRRelatedDeclarationForExport.CSI_SubType", Caption = "Partial?")]
		public override ZString CSI_SubType { get => base.CSI_SubType; set => base.CSI_SubType = value; }

		[ResourceStringData("TRRelatedDeclarationForExport.CSI_Quantity", Caption = "Box Quantity")]
		public override ZDecimal CSI_Quantity { get => base.CSI_Quantity; set => base.CSI_Quantity = value; }

		[ResourceStringData("TRRelatedDeclarationForExport.CSI_Quantity2", Caption = "Gross Weight")]
		public override ZDecimal CSI_Quantity2 { get => base.CSI_Quantity2; set => base.CSI_Quantity2 = value; }
	}
}
