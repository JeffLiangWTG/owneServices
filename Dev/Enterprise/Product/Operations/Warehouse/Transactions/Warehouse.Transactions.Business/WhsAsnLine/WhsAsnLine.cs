using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	[ActionFieldFollow(false)]
	[DeferTriggerAndRunBeforeCommit(WhsValidationHelper.TG_WhsAsnLine_EnsureSerialNumberQuantityMatchWithLine, WhsValidationHelper.WhsCheckSerialNumberQuantityMatchWithAsnLine, WhsAsnLineSchema.Constants.PK, typeof(IWhsCheckSerialNumberQuantityMatchWithAsnLine_DeferTriggerStrategy))]
	public class WhsAsnLine : AutoWhsAsnLine, IWhsAsnLine, ISerialNumberParent
	{
		#region Constructors

		public WhsAsnLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#endregion

		#region Properties

		// calculated

		#region CommodityCode

		public ZString CommodityCode
		{
			get
			{
				var part = SupplierPart;
				return part != null ? part.OP_RH_NKCommodityCode : ZString.Empty;
			}
		}

		public ZPropertyInfo CommodityCodeInfo
		{
			get { return GetZPropertyInfo(nameof(CommodityCode)); }
		}

		#endregion

		#region WI_OP_Desc

		public ZString WN_OP_Desc
		{
			get
			{
				var part = SupplierPart;
				return part != null ? part.OP_Desc : ZString.Empty;
			}
		}

		public ZPropertyInfo WN_OP_DescInfo
		{
			get { return GetZPropertyInfo(nameof(WN_OP_Desc)); }
		}

		#endregion

		// persistent

		#region WN_LineNo

		[ReadOnly(true)]
		public override ZInt WN_LineNo
		{
			get { return base.WN_LineNo; }
			set { base.WN_LineNo = value; }
		}

		#endregion

		#region WN_QuantityUQ

		[ReadOnly(true)]
		[List("Lookups.ProductUQList")]
		public override ZString WN_QuantityUQ
		{
			get { return base.WN_QuantityUQ; }
			set { base.WN_QuantityUQ = value; }
		}

		#endregion

		#region WN_OP

		[ReadOnly(true)]
		[List("Lookups.SupplierParts")]
		public override ZGuid WN_OP
		{
			get { return base.WN_OP; }
			set { base.WN_OP = value; }
		}

		#endregion

		#region WN_Quantity

		[ReadOnly(true)]
		public override ZDecimal WN_Quantity
		{
			get { return base.WN_Quantity; }
			set { base.WN_Quantity = value; }
		}

		#endregion

		#region WN_SubLineNo

		[ReadOnly(true)]
		public override ZInt WN_SubLineNo
		{
			get { return base.WN_SubLineNo; }
			set { base.WN_SubLineNo = value; }
		}

		#endregion

		#region WN_PalletId

		[ReadOnly(true)]
		public override ZString WN_PalletId
		{
			get { return base.WN_PalletId; }
			set { base.WN_PalletId = value; }
		}

		#endregion

		#region Attributes

		#region WN_ExpiryDate

		[ReadOnly(true)]
		public override ZDate WN_ExpiryDate
		{
			get { return base.WN_ExpiryDate; }
			set { base.WN_ExpiryDate = value; }
		}

		#endregion

		#region WN_PackingDate

		[ReadOnly(true)]
		public override ZDate WN_PackingDate
		{
			get { return base.WN_PackingDate; }
			set { base.WN_PackingDate = value; }
		}

		#endregion

		#region WN_PartAttrib1

		[ReadOnly(true)]
		public override ZString WN_PartAttrib1
		{
			get { return base.WN_PartAttrib1; }
			set { base.WN_PartAttrib1 = value; }
		}

		#endregion

		#region WN_PartAttrib2

		[ReadOnly(true)]
		public override ZString WN_PartAttrib2
		{
			get { return base.WN_PartAttrib2; }
			set { base.WN_PartAttrib2 = value; }
		}

		#endregion

		#region WN_PartAttrib3

		[ReadOnly(true)]
		public override ZString WN_PartAttrib3
		{
			get { return base.WN_PartAttrib3; }
			set { base.WN_PartAttrib3 = value; }
		}

		#endregion

		#region WN_SerialNumber

		[ReadOnly(true)]
		public override ZString WN_SerialNumber
		{
			get => base.WN_SerialNumber;
			set => base.WN_SerialNumber = value;
		}

		#endregion

		#endregion

		#endregion

		#region Related Entities

		#region Docket

		public WhsReceive Docket
		{
			get { return Factory.Load<WhsReceive>(WN_WD); }
		}

		#endregion

		#region SerialNumbers

		[ChildEditable]
		public WhsSerialNumberPivotCollection SerialNumbers
		{
			get
			{
				if (serialNumbers == null)
				{
					serialNumbers = new WhsSerialNumberPivotCollection(this);
					RegisterEditableChildObject(serialNumbers);
				}
				return serialNumbers;
			}
		}

		WhsSerialNumberPivotCollection serialNumbers;

		#region ISerialNumberParent

		public ZGuid ClientPK => Docket.WD_OH_Client;

		ZGuid ISerialNumberParent.ProductPK => WN_OP;

		bool ISerialNumberParent.SerialNumberReadOnly => true;

		public bool IsSerialNumberUsed => WhsProduct.GetWhsProduct(SupplierPart)?.IsSerialNumberUsed(Docket.Client) ?? false;

		bool ISerialNumberParent.IsAllowedToCreateOriginalSerialNumberRecord => false;

		bool ISerialNumberParent.IsSerialNumberAlreadyInUse(WhsSerialNumberPivot pivot) => false; // Implemente in next WIs

		#endregion

		#endregion

		#endregion

		#region SupportsNotes

		public override bool SupportsNotes
		{
			get { return false; }
		}

		#endregion

		#region UsedAttributesCount

		public int UsedAttributesAndPalletIdCount
		{
			get
			{
				int result = 0;
				if (!WN_PartAttrib1.IsEmpty)
				{
					result++;
				}
				if (!WN_PartAttrib2.IsEmpty)
				{
					result++;
				}
				if (!WN_PartAttrib3.IsEmpty)
				{
					result++;
				}
				if (!WN_SerialNumber.IsEmpty)
				{
					result++;
				}
				if (!WN_PackingDate.IsEmpty)
				{
					result++;
				}
				if (!WN_ExpiryDate.IsEmpty)
				{
					result++;
				}
				if (!WN_PalletId.IsEmpty)
				{
					result++;
				}
				return result;
			}
		}

		#endregion

		#region Lookups

		protected override WhsAsnLineLookups GetNewLookups()
		{
			return new WhsAsnLineLookups(this);
		}

		#endregion

		#region Validation

		protected override WhsAsnLineValidation GetNewValidation()
		{
			return new WhsAsnLineValidation(this);
		}

		#endregion
	}
}
