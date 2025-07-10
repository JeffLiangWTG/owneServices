using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using YesNoList = Enterprise.Customs.Universal.CodeDescriptionPairLists.YesNoList;

namespace Enterprise.Customs.TR.Manifest.Business
{
	public class ManifestToOpen : CusSupportingInfo
	{
		public ManifestToOpen(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
		public new class Schema : AutoCusSupportingInfo.Schema
		{
			public const int ReferenceNumberMaxLength = 20;
			public const int SubTypeMaxLength = 1;
			public const string AtWarehouse = "AtWarehouse";
			public const string Procedure = "Procedure";
			public const string BillNo = "BillNo";
		}
		public const string ManifestToOpenType = "OTH";
		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_Type = ManifestToOpenType;
			CSI_SubType = SubTypeListForManifestToOpen.Codes.Billlevel;
			CSI_Status = YesNoList.Codes.No;
			CSI_Procedure = YesNoList.Codes.No;
		}
		protected override CusSupportingInfoValidation GetNewValidation() => new ManifestToOpenValidation(this);
		public new ManifestToOpenValidation Validation => (ManifestToOpenValidation)base.Validation;

		public new ManifestToOpenLookups Lookups => (ManifestToOpenLookups)base.Lookups;
		protected override CusSupportingInfoLookups GetNewLookups() => new ManifestToOpenLookups(this);

		[MaxLength(Schema.SubTypeMaxLength)]
		[ResourceStringData("TRManifestToOpen.CSI_SubType", Caption = "Opening Style")]
		public override ZString CSI_SubType
		{
			get => base.CSI_SubType;
			set
			{
				var oldValue = CSI_SubType;
				base.CSI_SubType = value;
				if (oldValue != CSI_SubType)
				{
					if (BillNoReadOnly)
					{
						BillNo = ZString.Empty;
					}
					if (ReadOnlyForBilllinelevel)
					{
						CSI_LineNo = ZShort.Zero;
						CSI_Quantity = ZDecimal.Zero;
						CSI_Quantity2 = ZDecimal.Zero;
						CSI_CustomsOffice = ZString.Empty;
					}
				}
			}
		}

		[MaxLength(Schema.ReferenceNumberMaxLength)]
		[ResourceStringData("TRManifestToOpen.CSI_ReferenceNumber2", Caption = "Registration No")]
		public override ZString CSI_ReferenceNumber2
		{
			get
			{
				return base.CSI_ReferenceNumber2;
			}
			set
			{
				base.CSI_ReferenceNumber2 = value;
				CheckAndSetDescriptionFromOtherRecords();
			}
		}

		[ResourceStringData("TRManifestToOpen.AtWarehouse", Caption = "At Warehouse")]
		public ZBool AtWarehouse
		{
			get { return this.CSI_Status == YesNoList.Codes.Yes; }
			set
			{
				this.CSI_Status = value ? YesNoList.Codes.Yes : YesNoList.Codes.No;
				AtWarehouseInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo AtWarehouseInfo => GetZPropertyInfo(Schema.AtWarehouse);

		[ResourceStringData("TRManifestToOpen.Procedure", Caption = "Re-open?")]
		public ZBool Procedure
		{
			get { return this.CSI_Procedure == YesNoList.Codes.Yes; }
			set
			{
				this.CSI_Procedure = value ? YesNoList.Codes.Yes : YesNoList.Codes.No;
				ProcedureInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo ProcedureInfo => GetZPropertyInfo(Schema.Procedure);

		void UpdateDescriptionForCollection()
		{
			var header = (AsycudaManifestHeader)Parent;
			var collection = header.ManifestsToOpenList.Cast<ManifestToOpen>().Where(x => x.CSI_ReferenceNumber2 == CSI_ReferenceNumber2 && x.PK != PK).ToArray();
			foreach (var item in collection)
			{
				item.CSI_Description = CSI_Description;
			}
		}

		void CheckAndSetDescriptionFromOtherRecords()
		{
			var header = (AsycudaManifestHeader)Parent;
			var previousRecord = header.ManifestsToOpenList.Cast<ManifestToOpen>().FirstOrDefault(x => x.CSI_ReferenceNumber2 == CSI_ReferenceNumber2 && !x.CSI_Description.IsEmpty && x.PK != PK);
			this.CSI_Description = previousRecord?.CSI_Description ?? this.CSI_Description;
		}

		[MaxLength(100)]
		[ResourceStringData("TRManifestToOpen.ManifestToOpenDescription", Caption = "Description")]
		[ReadOnlyMember(nameof(IsDescriptionReadOnly))]
		public override ZString CSI_Description
		{
			get
			{
				return base.CSI_Description;
			}
			set
			{
				var oldValue = base.CSI_Description;
				if (value != oldValue)
				{
					base.CSI_Description = value;
					if (IsFirstRecordForDescription)
					{
						UpdateDescriptionForCollection();
					}
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateCSI_Description();
				}
			}
		}

		[MaxLength(35)]
		[ResourceStringData("TRManifestToOpen.BillNo", Caption = "Bill No")]
		[ReadOnlyMember(nameof(BillNoReadOnly))]
		public ZString BillNo
		{
			get
			{
				return CSI_ReferenceNumber;
			}
			set
			{
				CheckMaximumLength(BillNoInfo, value);
				base.CSI_ReferenceNumber = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateBillNo();
				}
				BillNoInfo.RefreshBinding();
			}
		}
		bool BillNoReadOnly => CSI_SubType != SubTypeListForManifestToOpen.Codes.Billlevel && CSI_SubType != SubTypeListForManifestToOpen.Codes.Billlinelevel;
		public ZPropertyInfo BillNoInfo => GetZPropertyInfo(Schema.BillNo);

		[MaxLength(5)]
		[ResourceStringData("TRManifestToOpen.CSI_LineNo", Caption = "Bill Line No")]
		[ReadOnlyMember(nameof(ReadOnlyForBilllinelevel))]
		public override ZInt CSI_LineNo { get => base.CSI_LineNo; set => base.CSI_LineNo = value; }
		bool ReadOnlyForBilllinelevel => CSI_SubType != SubTypeListForManifestToOpen.Codes.Billlinelevel;

		[ResourceStringData("TRManifestToOpen.CSI_Quantity", Caption = "Total No. of Packs")]
		[DecimalPlaces(2)]
		[ReadOnlyMember(nameof(ReadOnlyForBilllinelevel))]
		public override ZDecimal CSI_Quantity { get => base.CSI_Quantity; set => base.CSI_Quantity = value; }

		[ResourceStringData("TRManifestToOpen.CSI_Quantity2", Caption = "Deduction No. of Packs")]
		[DecimalPlaces(2)]
		[ReadOnlyMember(nameof(ReadOnlyForBilllinelevel))]
		public override ZDecimal CSI_Quantity2 { get => base.CSI_Quantity2; set => base.CSI_Quantity2 = value; }

		[List(nameof(Lookups) + "." + nameof(ManifestToOpenLookups.TRWarehouseList))]
		[ResourceStringData("TRManifestToOpen.CSI_CustomsOffice", Caption = "Warehouse Code")]
		[ReadOnlyMember(nameof(ReadOnlyForBilllinelevel))]
		public override ZString CSI_CustomsOffice { get => base.CSI_CustomsOffice; set => base.CSI_CustomsOffice = value; }

		public ZBool IsFirstRecordForDescription => CheckIfFirstRecordForDescription();

		bool CheckIfFirstRecordForDescription()
		{
			var firstItem = (Parent as AsycudaManifestHeader)?.ManifestsToOpenList.Cast<ManifestToOpen>().First(x => x.CSI_ReferenceNumber2 == CSI_ReferenceNumber2);
			return firstItem?.PK == PK;
		}

		public ZBool IsDescriptionReadOnly => !IsFirstRecordForDescription;

		protected override ZString HumanReadableNameCore => ResString.GetMultilingualString("D1843395-4359-458F-B6FB-460C5C092C37", "Manifest to Open");
	}
}
