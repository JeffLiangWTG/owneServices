using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using YesNoList = Enterprise.Customs.Universal.CodeDescriptionPairLists.YesNoList;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class NctsManifestsToOpen : CusSupportingInfo
	{
		public NctsManifestsToOpen(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : AutoCusSupportingInfo.Schema
		{
			public const string AtWarehouse = "AtWarehouse";
			public const string IsPartial = "IsPartial";
			public const string IsOtherProcedure = "IsOtherProcedure";
		}

		protected override CusSupportingInfoValidation GetNewValidation() => new NctsManifestsToOpenValidation(this);
		public new NctsManifestsToOpenValidation Validation => (NctsManifestsToOpenValidation)base.Validation;
		public new NctsManifestsToOpenLookups Lookups => (NctsManifestsToOpenLookups)base.Lookups;
		protected override CusSupportingInfoLookups GetNewLookups() => new NctsManifestsToOpenLookups(this);

		[ResourceStringData("NctsManifestsToOpen|AtWarehouse", Caption = "At Warehouse")]
		public ZBool AtWarehouse
		{
			get { return this.CSI_Status == YesNoList.Codes.Yes; }
			set
			{
				var oldValue = AtWarehouse;
				this.CSI_Status = value ? YesNoList.Codes.Yes : YesNoList.Codes.No;
				if (oldValue != AtWarehouse && WarehouseCodeReadOnly)
				{
					this.CSI_CustomsOffice = "";
				}
				AtWarehouseInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo AtWarehouseInfo => GetZPropertyInfo(Schema.AtWarehouse);
		public bool WarehouseCodeReadOnly => !AtWarehouse;
		[List(nameof(Lookups) + "." + nameof(NctsManifestsToOpenLookups.TRWarehouseList))]
		[ResourceStringData("NctsManifestsToOpen|CSI_CustomsOffice", Caption = "Warehouse Code")]
		[ReadOnlyMember(nameof(WarehouseCodeReadOnly))]
		public override ZString CSI_CustomsOffice { get => base.CSI_CustomsOffice; set => base.CSI_CustomsOffice = value; }

		[ResourceStringData("NctsManifestsToOpen|CSI_ReferenceNumber", Caption = "Bill No")]
		public override ZString CSI_ReferenceNumber { get => base.CSI_ReferenceNumber; set => base.CSI_ReferenceNumber = value; }

		[ResourceStringData("NctsManifestsToOpen|IsPartial", Caption = "Partial?")]
		public ZBool IsPartial
		{
			get
			{ return this.CSI_SubType == YesNoList.Codes.Yes; }
			set
			{
				var oldValue = IsPartial;
				this.CSI_SubType = value ? YesNoList.Codes.Yes : YesNoList.Codes.No;

				if (oldValue != IsPartial && !IsCopying)
				{
					if (!BillLineNoAndQuantityReadOnly)
					{
						CSI_Quantity3 = ZDecimal.Zero;
						CSI_LineNo = ZShort.Zero;
					}
				}
				IsPartialInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo IsPartialInfo => GetZPropertyInfo(Schema.IsPartial);

		public bool BillLineNoAndQuantityReadOnly => !IsPartial;
		[MaxLength(5)]
		[ReadOnlyMember(nameof(BillLineNoAndQuantityReadOnly))]
		[ResourceStringData("NctsManifestsToOpen|CSI_LineNo", Caption = "Bill Line No")]
		public override ZInt CSI_LineNo { get => base.CSI_LineNo; set => base.CSI_LineNo = value; }

		[ResourceStringData("NctsManifestsToOpen|CSI_Quantity3", Caption = "Box Quantity")]
		[DecimalPlaces(0)]
		[ReadOnlyMember(nameof(BillLineNoAndQuantityReadOnly))]
		public override ZDecimal CSI_Quantity3 { get => base.CSI_Quantity3; set => base.CSI_Quantity3 = value; }

		[ResourceStringData("NctsManifestsToOpen|CSI_ReferenceNumber2", Caption = "Declaration No")]
		[MaxLength(20)]
		public override ZString CSI_ReferenceNumber2 { get => base.CSI_ReferenceNumber2; set => base.CSI_ReferenceNumber2 = value; }

		[ResourceStringData("NctsManifestsToOpen|IsOtherProcedure", Caption = "Other Procedure?")]
		public ZBool IsOtherProcedure
		{
			get
			{
				return this.CSI_IssuerType == YesNoList.Codes.Yes;
			}
			set
			{
				this.CSI_IssuerType = value ? YesNoList.Codes.Yes : YesNoList.Codes.No;
				IsOtherProcedureInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo IsOtherProcedureInfo => GetZPropertyInfo(Schema.IsOtherProcedure);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_Status = YesNoList.Codes.No;
			CSI_SubType = YesNoList.Codes.No;
			CSI_IssuerType = YesNoList.Codes.No;
		}
	}
}
