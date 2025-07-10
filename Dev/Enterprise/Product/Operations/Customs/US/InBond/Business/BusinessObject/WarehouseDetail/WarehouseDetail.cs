using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.InBond.Business
{
	public class WarehouseDetail : AutoWarehouseDetail
	{
		public WarehouseDetail(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoWarehouseDetail.Schema
		{
			public const string WarehouseBondedQuantityBalance = "WarehouseBondedQuantityBalance";
		}

		#region WarehouseBondedQuantityBalance

		[DecimalPlaces(0)]
		[ResourceStringData("Enterprise.Customs.US.InBond.Business.WarehouseDetail|WarehouseBondedQuantityBalance", Caption = "Balance Quantity", MediumCaption = "Balance Qty", ShortCaption = "Bal. Qty")]
		public ZDecimal WarehouseBondedQuantityBalance
		{
			get { return US_WarehouseBondedQuantity - US_WarehouseWithdrawQuantity; }
		}

		public ZPropertyInfo WarehouseBondedQuantityBalanceInfo
		{
			get { return GetZPropertyInfo(Schema.WarehouseBondedQuantityBalance); }
		}

		#endregion

		[DecimalPlaces(0)]
		[ResourceStringData("Enterprise.Customs.US.InBond.Business.WarehouseDetail|US_WarehouseBondedQuantity", Caption = "Bonded Quantity", MediumCaption = "Bonded Qty", ShortCaption = "Bonded")]
		public override ZDecimal US_WarehouseBondedQuantity { get => base.US_WarehouseBondedQuantity; set => base.US_WarehouseBondedQuantity = value; }

		[ResourceStringData("Enterprise.Customs.US.InBond.Business.WarehouseDetail|US_WarehouseWithdrawQuantity", Caption = "Withdraw Quantity", MediumCaption = "Withdraw Qty", ShortCaption = "Withdraw")]
		[DecimalPlaces(0)]
		public override ZDecimal US_WarehouseWithdrawQuantity { get => base.US_WarehouseWithdrawQuantity; set => base.US_WarehouseWithdrawQuantity = value; }

		[ResourceStringData("Enterprise.Customs.US.InBond.Business.WarehouseDetail|US_WarehouseNumber", Caption = "Entry Number", MediumCaption = "Entry No.")]
		public override ZString US_WarehouseNumber { get => base.US_WarehouseNumber; set => base.US_WarehouseNumber = value; }

		[BusinessObjectTestExclude]
		public override ZString B7_ParentTableCode
		{
			get { return base.B7_ParentTableCode; }
			set
			{
				if (!value.IsEmpty && value != CusInBondMoveDetailSchema.Constants.Prefix)
				{
					throw new NotSupportedException("Setting WarehouseDetail.B7_ParentTableCode is not supported.");
				}
				base.B7_ParentTableCode = value;
			}
		}

		public new CusInBondMoveDetail Parent
		{
			get
			{
				if (B7_Type != CusAddInfoTypeAttribute.Codes.USWarehouseDetail)
				{
					fParent = null;
				}
				else if ((fParent == null) || (fParent.PK != B7_ParentID))
				{
					fParent = base.Factory.Load<CusInBondMoveDetail>(B7_ParentID);
				}
				return fParent;
			}
		}

		#region Implementation
		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			base.B7_ParentTableCode = CusAddInfoSchema.Constants.Prefix;
			B7_Type = CusAddInfoTypeAttribute.Codes.USWarehouseDetail;
		}

		CusInBondMoveDetail fParent;
		#endregion
	}
}
