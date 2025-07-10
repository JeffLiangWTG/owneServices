using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.TW.Business
{
	public class CusInBondBill : Customs.Business.CusInBondBill
	{
		public CusInBondBill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Type MovementDetailType
		{
			get { return typeof(CusInBondMoveDetail); }
		}

		public new CusInBondHeader Header => (CusInBondHeader)base.Header;

		public new CusInBondBillValidation Validation => (CusInBondBillValidation)base.Validation;

		protected override Customs.Business.CusInBondBillValidation GetNewValidation()
		{
			if (IsImport)
			{
				return new ArrivalBillValidation(this);
			}
			else if (IsExport)
			{
				return new MovementBillValidation(this);
			}
			else
			{
				return new CusInBondBillValidation(this);
			}
		}

		protected override Customs.Business.CusInBondBillLookups GetNewLookups()
		{
			return new CusInBondBillLookups(this);
		}

		public ZBool IsAirForMasterBill
		{
			get
			{
				var result = false;
				if (Header != null)
				{
					if (IsExport)
					{
						result = (Header.MovementHeader?.BM_ExportLadenOn ?? ZString.Empty).IsEmpty;
					}
					else if (IsImport)
					{
						result = Header.IsTransportModeAir;
					}
				}
				return result;
			}
		}

		public new CusInBondBillLookups Lookups => (CusInBondBillLookups)base.Lookups;

		[ResourceStringData("Enterprise.Customs.TW.Business.CusInBondBill|B0_MasterBillNumber", Caption = "Master Bill")]
		public override ZString B0_MasterBillNumber
		{
			get => base.B0_MasterBillNumber;
			set
			{
				if (IsAirForMasterBill)
				{
					value = value.Replace("-", "").Replace(" ", "");
				}
				base.B0_MasterBillNumber = value;
			}
		}

		[ResourceStringData("Enterprise.Customs.TW.Business.CusInBondBill|B0_HouseBillNumber", Caption = "House Bill")]
		public override ZString B0_HouseBillNumber { get => base.B0_HouseBillNumber; set => base.B0_HouseBillNumber = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.CusInBondBill|B0_Weight", Caption = "Total Gross Weight")]
		public override ZDecimal B0_Weight
		{
			get => base.B0_Weight;
			set
			{
				var oldValue = B0_Weight;
				base.B0_Weight = value;
				if (!IsCopying && oldValue != B0_Weight)
				{
					Validation.ValidateB0_WeightUQ();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusInBondBillLookups.WeightUnits))]
		[ResourceStringData("Enterprise.Customs.TW.Business.CusInBondBill|B0_WeightUQ", Caption = "Weight UQ")]
		public override ZString B0_WeightUQ
		{
			get => base.B0_WeightUQ;
			set
			{
				var oldValue = B0_WeightUQ;
				base.B0_WeightUQ = value;
				if (!IsCopying && oldValue != B0_WeightUQ)
				{
					Validation.ValidateB0_Weight();
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.TW.Business.CusInBondBill|B0_ManifestQty", Caption = "Total Package Qty")]
		public override ZInt B0_ManifestQty
		{
			get => base.B0_ManifestQty;
			set
			{
				var oldValue = B0_ManifestQty;
				base.B0_ManifestQty = value;
				if (!IsCopying && oldValue != B0_ManifestQty)
				{
					Validation.ValidateB0_ManifestUQ();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusInBondBillLookups.ManifestUnits))]
		[ResourceStringData("Enterprise.Customs.TW.Business.CusInBondBill|B0_ManifestUQ", Caption = "Manifest UQ")]
		public override ZString B0_ManifestUQ
		{
			get => base.B0_ManifestUQ;
			set
			{
				var oldValue = B0_ManifestUQ;
				base.B0_ManifestUQ = value;
				if (!IsCopying && oldValue != B0_ManifestUQ)
				{
					Validation.ValidateB0_ManifestQty();
				}
			}
		}

		[MaxLength(4)]
		public override ZString B0_ReferenceID
		{
			get => base.B0_ReferenceID;
			set
			{
				base.B0_ReferenceID = value;
				if (IsImport)
				{
					Header?.MovementBill?.Validation?.ValidateB0_ReferenceID();
				}
				else if (IsExport)
				{
					Header?.ArrivalBill?.Validation?.ValidateB0_ReferenceID();
				}
			}
		}

		public ZString VoyageFlightNo
		{
			get
			{
				ZString result = ZString.Empty;
				if (IsImport)
				{
					result = Header?.BH_UniqueVoyageIdentifier ?? ZString.Empty;
				}
				else if (IsExport)
				{
					result = Header?.MovementHeader?.BM_ConveyanceNumber ?? ZString.Empty;
				}
				return result;
			}
		}

		ZBool IsImport => B0_ShipmentType == Constants.CusInBondBill.ShipmentType.Import;

		ZBool IsExport => B0_ShipmentType == Constants.CusInBondBill.ShipmentType.Export;
	}
}
