using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Tracking.Business
{
	public class TrackingWhsOrderSummaryLine : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Constructors

		public TrackingWhsOrderSummaryLine(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public TrackingWhsOrderSummaryLine(TrackingWhsOrderLine line)
			: base(line.Factory)
		{
			productPK = line.WhsOrderLine.WE_OP;
			packsQuantity = line.WhsOrderLine.WE_PackQuantity;
			packsUQ = line.WhsOrderLine.WE_F3_NKPackType;
			orderedQuantity = line.WhsOrderLine.WE_TransactionQuantity;
			uQ = line.WhsOrderLine.ProductUQ;
			reservedQuantity = line.WhsOrderLine.WE_CrossDockQuantity;
		}

		#endregion

		#region Schema

		public abstract class Schema
		{
			public const string ProductPK = "ProductPK";
			public const string ProductCode = "ProductCode";
			public const string ProductDescription = "ProductDescription";
			public const string PacksQuantity = "PacksQuantity";
			public const string PacksUQ = "PacksUQ";
			public const string OrderedQuantity = "OrderedQuantity";
			public const string UQ = "UQ";
			public const string ReservedQuantity = "ReservedQuantity";
		}

		#endregion

		#region Properties

		#region ProductPK

		public ZPropertyInfo ProductPKInfo
		{
			get { return GetZPropertyInfo(Schema.ProductPK); }
		}

		public ZGuid ProductPK
		{
			get
			{
				return productPK;
			}
			set
			{
				productPK = value;
				ProductPKInfo.RefreshBinding();
			}
		}

		ZGuid productPK;

		#endregion

		#region ProductCode

		public ZPropertyInfo ProductCodeInfo
		{
			get { return GetZPropertyInfo(Schema.ProductCode); }
		}

		public ZString ProductCode
		{
			get
			{
				return (SupplierPart == null ? ZString.Empty : SupplierPart.OP_PartNum);
			}
		}

		#endregion

		#region ProductDescription

		public ZPropertyInfo ProductDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.ProductDescription); }
		}

		public ZString ProductDescription
		{
			get
			{
				return (SupplierPart == null ? ZString.Empty : SupplierPart.OP_Desc);
			}
		}

		#endregion

		#region PacksQuantity

		public ZPropertyInfo PacksQuantityInfo
		{
			get { return GetZPropertyInfo(Schema.PacksQuantity); }
		}

		public ZDecimal PacksQuantity
		{
			get
			{
				return packsQuantity;
			}
			set
			{
				packsQuantity = value;
				PacksQuantityInfo.RefreshBinding();
			}
		}

		ZDecimal packsQuantity;

		#endregion

		#region PacksUQ

		public ZPropertyInfo PacksUQInfo
		{
			get { return GetZPropertyInfo(Schema.PacksUQ); }
		}

		[MaxLength(3)]
		public ZString PacksUQ
		{
			get
			{
				return packsUQ;
			}
			set
			{
				CheckMaximumLength(PacksUQInfo, value);
				packsUQ = value;
				PacksUQInfo.RefreshBinding();
			}
		}

		ZString packsUQ;

		#endregion

		#region OrderedQuantity

		public ZPropertyInfo OrderedQuantityInfo
		{
			get { return GetZPropertyInfo(Schema.OrderedQuantity); }
		}

		public ZDecimal OrderedQuantity
		{
			get
			{
				return orderedQuantity;
			}
			set
			{
				orderedQuantity = value;
				OrderedQuantityInfo.RefreshBinding();
			}
		}

		ZDecimal orderedQuantity;

		#endregion

		#region UQ

		public ZPropertyInfo UQInfo
		{
			get { return GetZPropertyInfo(Schema.UQ); }
		}

		[MaxLength(3)]
		public ZString UQ
		{
			get
			{
				return uQ;
			}
			set
			{
				CheckMaximumLength(UQInfo, value);
				uQ = value;
				UQInfo.RefreshBinding();
			}
		}

		ZString uQ;

		#endregion

		#region ReservedQuantity

		public ZPropertyInfo ReservedQuantityInfo
		{
			get { return GetZPropertyInfo(Schema.ReservedQuantity); }
		}

		public ZDecimal ReservedQuantity
		{
			get
			{
				return reservedQuantity;
			}
			set
			{
				reservedQuantity = value;
				ReservedQuantityInfo.RefreshBinding();
			}
		}

		ZDecimal reservedQuantity;

		#endregion

		#region Pack Types

		public CodeDescriptionPairList PackTypes
		{
			get { return new RefPackTypeCollection(Factory).GetAsCodeDescriptionPairWithStandardUnits(); }
		}

		#endregion

		public virtual OrgSupplierPart SupplierPart
		{
			get { return Factory.Load<OrgSupplierPart>(ProductPK); }
		}

		#endregion
	}
}
