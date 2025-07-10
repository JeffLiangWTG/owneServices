using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class NctsExportToOpen : CusSupportingInfo
	{
		public NctsExportToOpen(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : AutoCusSupportingInfo.Schema
		{
			public const string IsWrapper = "IsWrapper";
			public const string IsPartial = "IsPartial";
		}

		public new NctsDepartureCargoDesc Parent => (NctsDepartureCargoDesc)base.Parent;

		protected override CusSupportingInfoValidation GetNewValidation() => new NctsExportToOpenValidation(this);
		public new NctsExportToOpenValidation Validation => (NctsExportToOpenValidation)base.Validation;
		public new NctsExportToOpenLookups Lookups => (NctsExportToOpenLookups)base.Lookups;
		protected override CusSupportingInfoLookups GetNewLookups() => new NctsExportToOpenLookups(this);

		[MaxLength(4)]
		[ResourceStringData("NctsExportToOpen|CSI_Procedure", Caption = "Declaration Type")]
		[List(nameof(Lookups) + "." + nameof(NctsExportToOpenLookups.DeclarationTypeList))]
		public override ZString CSI_Procedure
		{
			get => base.CSI_Procedure;
			set => base.CSI_Procedure = value;
		}

		[ResourceStringData("NctsManifestsToOpen|IsPartial", Caption = "Partial?")]
		public ZBool IsPartial
		{
			get
			{ return this.CSI_SubType == YesNoList.Codes.Yes; }
			set
			{
				var oldValue = IsPartial;
				this.CSI_SubType = value ? YesNoList.Codes.Yes : YesNoList.Codes.No;
				IsPartialInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo IsPartialInfo => GetZPropertyInfo(nameof(IsPartial));

		[ResourceStringData("NctsExportToOpen|CSI_ReferenceNumber", Caption = "Declaration No")]
		[MaxLength(20)]
		public override ZString CSI_ReferenceNumber { get => base.CSI_ReferenceNumber; set => base.CSI_ReferenceNumber = value; }

		[ResourceStringData("NctsExportToOpen|CSI_ItemNumber", Caption = "Declaration Line No")]
		[MaxLength(20)]
		public override ZInt CSI_ItemNumber { get => base.CSI_ItemNumber; set => base.CSI_ItemNumber = value; }

		[ResourceStringData("NctsExportToOpen|IsOtherProcedure", Caption = "Wrapper?")]
		public ZBool IsWrapper
		{
			get
			{
				return this.CSI_IssuerType == YesNoList.Codes.Yes;
			}
			set
			{
				this.CSI_IssuerType = value ? YesNoList.Codes.Yes : YesNoList.Codes.No;
				IsWrapperInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo IsWrapperInfo => GetZPropertyInfo(nameof(IsWrapper));

		[ResourceStringData("NctsExportToOpen|CSI_ReferenceNumber2", Caption = "Consignor ID", FullDescription = "If the sender of the export declaration you have defined is different from the sender you have defined in the shipment, enter the sender identification number in this field.")]
		[MaxLength(20)]
		public override ZString CSI_ReferenceNumber2 { get => base.CSI_ReferenceNumber2; set => base.CSI_ReferenceNumber2 = value; }

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_SubType = YesNoList.Codes.No;
			CSI_IssuerType = YesNoList.Codes.No;
		}
	}
}
