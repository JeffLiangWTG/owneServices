using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.Customs.TW.Business
{
	[SystemDefinedValues]
	[DependentBusinessObject(typeof(CusInBondMoveDetail), "CusInBondMoveLineItemCollection")]
	public class CusInBondMoveLineItem : Customs.Business.CusInBondMoveLineItem
	{
		public CusInBondMoveLineItem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : Customs.Business.AutoCusInBondMoveLineItem.Schema
		{
			public const string TWIsCoPackaged = "TW_IsCoPackaged";
		}

		[ResourceStringData("Enterprise.Customs.TW.Business.CusInBondMoveLineItem|BI_Description", Caption = "Goods Description")]
		[MaxLength(256)]
		public override ZString BI_Description { get => base.BI_Description; set => base.BI_Description = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.CusInBondMoveLineItem|TW_IsCoPackaged", Caption = "Co-Package?")]
		public ZBool TW_IsCoPackaged
		{
			get => this.GetSystemDefinedValue<ZBool>(Schema.TWIsCoPackaged);
			set
			{
				var oldValue = TW_IsCoPackaged;
				this.SetSystemDefinedValue(Schema.TWIsCoPackaged, value);
				TW_IsCoPackagedInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo TW_IsCoPackagedInfo => GetZPropertyInfo(Schema.TWIsCoPackaged);

		[ResourceStringData("Enterprise.Customs.TW.Business.CusInBondMoveLineItem|BI_MarksAndNumbers", Caption = "Marks and Numbers")]
		public override ZString BI_MarksAndNumbers { get => base.BI_MarksAndNumbers; set => base.BI_MarksAndNumbers = value; }

		[DecimalPlaces(4)]
		[ResourceStringData("Enterprise.Customs.TW.Business.CusInBondMoveLineItem|BI_Quantity", Caption = "Quantity")]
		public override ZDecimal BI_Quantity
		{
			get => base.BI_Quantity;
			set
			{
				var oldValue = BI_Quantity;
				base.BI_Quantity = value;
				if (!IsCopying && oldValue != BI_Quantity)
				{
					Validation.ValidateBI_QuantityUQ();
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.TW.Business.CusInBondMoveLineItem|BI_QuantityUQ", Caption = "Quantity Unit")]
		[List(nameof(Lookups) + "." + nameof(CusInBondMoveLineItemLookups.PackageTypeList))]
		public override ZString BI_QuantityUQ
		{
			get => base.BI_QuantityUQ;
			set
			{
				var oldValue = BI_QuantityUQ;
				base.BI_QuantityUQ = value;
				if (!IsCopying && oldValue != BI_QuantityUQ)
				{
					Validation.ValidateBI_Quantity();
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.TW.Business.CusInBondMoveLineItem|BI_PackagingDescription", Caption = "Package Description")]
		public override ZString BI_PackagingDescription { get => base.BI_PackagingDescription; set => base.BI_PackagingDescription = value; }

		[RelatedBusinessObject("MoveDetail")]
		public override ZGuid BI_B9
		{
			get => base.BI_B9;
			set
			{
				if (value.IsEmpty && !IsCopying)
				{
					bI_B9CachedOnRelationshipResetByCore = base.BI_B9;
				}
				base.BI_B9 = value;
			}
		}

		public CusInBondMoveDetail MoveDetail
		{
			get
			{
				if (!IsDeleted && (fMoveDetail == null || fMoveDetail.PK != BI_B9))
				{
					var reference = BI_B9.IsEmpty ? bI_B9CachedOnRelationshipResetByCore : BI_B9;
					fMoveDetail = Factory.Load<CusInBondMoveDetail>(reference);
				}
				return fMoveDetail != null && !fMoveDetail.IsDeleted ? fMoveDetail : null;
			}
		}

		CusInBondMoveDetail fMoveDetail;

		ZGuid bI_B9CachedOnRelationshipResetByCore;

		public new CusInBondMoveLineItemLookups Lookups => (CusInBondMoveLineItemLookups)base.Lookups;

		public new CusInBondMoveLineItemValidation Validation => (CusInBondMoveLineItemValidation)base.Validation;

		protected override Customs.Business.CusInBondMoveLineItemLookups GetNewLookups()
		{
			return new CusInBondMoveLineItemLookups(this);
		}

		protected override Customs.Business.CusInBondMoveLineItemValidation GetNewValidation()
		{
			return new CusInBondMoveLineItemValidation(this);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			BI_QuantityUQ = Core.Constants.PkgUnit.Piece;
		}
	}
}
