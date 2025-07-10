using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.Customs.TR.NCTS.Business
{
	[SystemDefinedValues]
	public class NctsWarehouseToOpen : CusSupportingInfo
	{
		public NctsWarehouseToOpen(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : AutoCusSupportingInfo.Schema
		{
			public const string Incoterm = nameof(Incoterm);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			base.CSI_Code = NCTSMessageProviderConstants.PreviousDocuments.WarehouseCode;
			base.CSI_Type = CusSupportingInfoTypeList.Codes.PRE;
		}

		public new NctsWarehouseToOpenLookups Lookups => (NctsWarehouseToOpenLookups)base.Lookups;
		protected override CusSupportingInfoLookups GetNewLookups() => new NctsWarehouseToOpenLookups(this);

		public new NctsWarehouseToOpenValidation Validation => (NctsWarehouseToOpenValidation)base.Validation;
		protected override CusSupportingInfoValidation GetNewValidation() => new NctsWarehouseToOpenValidation(this);

		#region Properties

		[MaxLength(20)]
		[ResourceStringData("NctsWarehouseToOpen|CSI_ReferenceNumber", Caption = "Declaration No")]
		public override ZString CSI_ReferenceNumber { get => base.CSI_ReferenceNumber; set => base.CSI_ReferenceNumber = value; }

		[MaxLength(10)]
		[ResourceStringData("NctsWarehouseToOpen|LineNo", Caption = "Declaration Line No")]
		public override ZInt CSI_LineNo { get => base.CSI_LineNo; set => base.CSI_LineNo = value; }

		[MaxLength(10)]
		[ResourceStringData("NctsWarehouseToOpen|NctsLineNo", Caption = "NCTS Line No")]
		public override ZInt CSI_ItemNumber { get => base.CSI_ItemNumber; set => base.CSI_ItemNumber = value; }

		[DecimalPlaces(2)]
		[ResourceStringData("NctsWarehouseToOpen|Amount", Caption = "Amount")]
		public override ZDecimal CSI_Value { get => base.CSI_Value; set => base.CSI_Value = value; }

		[MaxLength(3)]
		[ResourceStringData("NctsWarehouseToOpen|CSI_RX_NKCurrency", Caption = "Currency")]
		[List(nameof(Lookups) + "." + nameof(NctsWarehouseToOpenLookups.Currencies))]
		public override ZString CSI_RX_NKCurrency { get => base.CSI_RX_NKCurrency; set => base.CSI_RX_NKCurrency = value; }

		[ResourceStringData("NctsWarehouseToOpen|CSI_Quantity", Caption = "Packages")]
		[DecimalPlaces(2)]
		public override ZDecimal CSI_Quantity { get => base.CSI_Quantity; set => base.CSI_Quantity = value; }

		[MaxLength(4)]
		[ResourceStringData("NctsWarehouseToOpen|CSI_UnitOfQuantity", Caption = "Packages UQ")]
		[List(nameof(Lookups) + "." + nameof(NctsWarehouseToOpenLookups.PackagesTypeList))]
		public override ZString CSI_UnitOfQuantity
		{
			get { return base.CSI_UnitOfQuantity; }
			set
			{
				value = value.ToUpper();
				base.CSI_UnitOfQuantity = value;
			}
		}

		[MaxLength(3)]
		[ResourceStringData("NctsWarehouseToOpen|Incoterm", Caption = "Incoterm")]
		[List(nameof(Lookups) + "." + nameof(NctsWarehouseToOpenLookups.IncotermList))]
		public ZString Incoterm
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.Incoterm);
			set
			{
				var oldValue = Incoterm;
				if (oldValue != value)
				{
					CheckMaximumLength(IncotermInfo, value);
					this.SetSystemDefinedValue(Schema.Incoterm, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateIncoterm();   
					}
					IncotermInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo IncotermInfo => GetZPropertyInfo(Schema.Incoterm);

		[MaxLength(2)]
		[ResourceStringData("NctsWarehouseToOpen|CSI_SubType", Caption = "Payment Type")]
		public override ZString CSI_SubType { get => base.CSI_SubType; set => base.CSI_SubType = value; }

		[MaxLength(2)]
		[ResourceStringData("NctsWarehouseToOpen|CSI_Procedure", Caption = "Nature of Business")]
		[List(nameof(Lookups) + "." + nameof(NctsWarehouseToOpenLookups.NatureOfBusinessList))]
		public override ZString CSI_Procedure { get => base.CSI_Procedure; set => base.CSI_Procedure = value; }

		[MaxLength(2)]
		[ResourceStringData("NctsWarehouseToOpen|CSI_RN_NKCountryCode", Caption = "Trade Country")]
		public override ZString CSI_RN_NKCountryCode { get => base.CSI_RN_NKCountryCode; set => base.CSI_RN_NKCountryCode = value; }

		[MaxLength(100)]
		[ResourceStringData("NctsWarehouseToOpen|CSI_Description", Caption = "Explanation")]
		public override ZString CSI_Description
		{
			get => base.CSI_Description;
			set => base.CSI_Description = value;
		}
		#endregion
	}
}
