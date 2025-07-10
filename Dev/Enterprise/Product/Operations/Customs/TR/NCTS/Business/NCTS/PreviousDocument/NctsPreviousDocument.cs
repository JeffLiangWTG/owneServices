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
	public class NctsPreviousDocument : EU.NCTS.Business.NctsPreviousDocument
	{
		public NctsPreviousDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : EU.NCTS.Business.NctsPreviousDocument.Schema
		{
			public const string Incoterm = nameof(Incoterm);
		}

		public new INctsPreviousDocumentLookups Lookups => (INctsPreviousDocumentLookups)base.Lookups;

		public new INctsPreviousDocumentValidation Validation => (INctsPreviousDocumentValidation)base.Validation;

		protected override CusSupportingInfoLookups GetNewPhase4Lookups() => new NctsPreviousDocumentLookups(this);

		protected override CusSupportingInfoLookups GetNewPhase5Lookups() => new NctsPreviousDocumentPhase5Lookups(this);

		protected override CusSupportingInfoValidation GetNewPhase5Validation() => new NctsPreviousDocumentPhase5Validation(this);

		protected override CusSupportingInfoValidation GetNewPhase4Validation() => new NctsPreviousDocumentValidation(this);

		public new NctsDepartureCargoDesc Parent => (NctsDepartureCargoDesc)base.Parent;

		#region Properties

		[ResourceStringData("TRPreviousDocument|CSI_Code", Caption = "Type")]
		[List(nameof(Lookups) + "." + nameof(INctsPreviousDocumentLookups.PreviousDocumentsCodeList))]
		public override ZString CSI_Code { get => base.CSI_Code; set => base.CSI_Code = value; }

		[MaxLength(20)]
		[ResourceStringData("TRPreviousDocument|CSI_ReferenceNumber", Caption = "Declaration No")]
		public override ZString CSI_ReferenceNumber { get => base.CSI_ReferenceNumber; set => base.CSI_ReferenceNumber = value; }

		[MaxLength(5)]
		[ResourceStringData("TRPreviousDocument|LineNo", Caption = "Declaration Line No")]
		public override ZInt CSI_LineNo { get => base.CSI_LineNo; set => base.CSI_LineNo = value; }

		[DecimalPlaces(2)]
		[ResourceStringData("TRPreviousDocument|Amount", Caption = "Amount")]
		public override ZDecimal CSI_Value { get => base.CSI_Value; set => base.CSI_Value = value; }

		[MaxLength(3)]
		[ResourceStringData("TRPreviousDocument|CSI_RX_NKCurrency", Caption = "Currency")]
		[List(nameof(Lookups) + "." + nameof(INctsPreviousDocumentLookups.Currencies))]
		public override ZString CSI_RX_NKCurrency { get => base.CSI_RX_NKCurrency; set => base.CSI_RX_NKCurrency = value; }

		[ResourceStringData("TRPreviousDocument|CSI_Quantity", Caption = "Packages")]
		[DecimalPlaces(2)]
		public override ZDecimal CSI_Quantity { get => base.CSI_Quantity; set => base.CSI_Quantity = value; }

		[ResourceStringData("TRPreviousDocument|CSI_Quantity2", Caption = "Gross Weight")]
		[DecimalPlaces(3)]
		public override ZDecimal CSI_Quantity2 { get => base.CSI_Quantity2; set => base.CSI_Quantity2 = value; }

		[MaxLength(4)]
		[ResourceStringData("TRPreviousDocument|CSI_UnitOfQuantity2", Caption = "Gross Weight UQ")]
		[List(nameof(Lookups) + "." + nameof(INctsPreviousDocumentLookups.WeightUQList))]
		public override ZString CSI_UnitOfQuantity2
		{
			get { return base.CSI_UnitOfQuantity2; }
			set
			{
				value = value.ToUpper();
				base.CSI_UnitOfQuantity2 = value;
			}
		}

		[ResourceStringData("TRPreviousDocument|CSI_Quantity3", Caption = "Net Weight")]
		[DecimalPlaces(3)]
		public override ZDecimal CSI_Quantity3 { get => base.CSI_Quantity3; set => base.CSI_Quantity3 = value; }

		[MaxLength(4)]
		[ResourceStringData("TRPreviousDocument|CSI_UnitOfQuantity3", Caption = "Net Weight UQ")]
		[List(nameof(Lookups) + "." + nameof(INctsPreviousDocumentLookups.WeightUQList))]
		public override ZString CSI_UnitOfQuantity3
		{
			get { return base.CSI_UnitOfQuantity3; }
			set
			{
				value = value.ToUpper();
				base.CSI_UnitOfQuantity3 = value;
			}
		}

		[MaxLength(3)]
		[ResourceStringData("TRPreviousDocument|Incoterm", Caption = "Incoterm")]
		[List(nameof(Lookups) + "." + nameof(INctsPreviousDocumentLookups.IncotermList))]
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
		[ResourceStringData("TRPreviousDocument|CSI_SubType", Caption = "Payment Type")]
		public override ZString CSI_SubType { get => base.CSI_SubType; set => base.CSI_SubType = value; }

		[MaxLength(2)]
		[ResourceStringData("TRPreviousDocument|CSI_Procedure", Caption = "Nature of Business")]
		[List(nameof(Lookups) + "." + nameof(INctsPreviousDocumentLookups.NatureOfBusinessList))]
		public override ZString CSI_Procedure
		{
			get => base.CSI_Procedure;
			set => base.CSI_Procedure = value;
		}

		[MaxLength(2)]
		[ResourceStringData("TRPreviousDocument|CSI_RN_NKCountryCode", Caption = "Trade Country")]
		public override ZString CSI_RN_NKCountryCode { get => base.CSI_RN_NKCountryCode; set => base.CSI_RN_NKCountryCode = value; }

		[MaxLength(100)]
		[ResourceStringData("TRPreviousDocument|CSI_Description", Caption = "Explanation")]
		public override ZString CSI_Description
		{
			get => base.CSI_Description;
			set => base.CSI_Description = value;
		}
		#endregion
	}
}
