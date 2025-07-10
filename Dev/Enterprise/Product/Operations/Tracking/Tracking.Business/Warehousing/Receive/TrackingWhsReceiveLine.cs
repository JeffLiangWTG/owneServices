using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.Tracking.Business
{
	public class TrackingWhsReceiveLine : NonPersistentBusinessObjectWithLogsAndNotes, IWhsReceiveLineWrapperStrategy, IWrappedBizOProvider
	{
		#region Schema

		public abstract class Schema : WhsReceiveLine.Schema { }

		public abstract class WrapperSchema
		{
			public const string ProductDescription = "ProductDescription";
			public const string AllocatedQuantityAsString = "AllocatedQuantityAsString";
			public const string UnallocatedQuantity = "UnallocatedQuantity";
			public const string ReleaseDetails = "ReleaseDetails";
			public const string ProductCode = SchemaRoot + WhsDocketLine.Schema.ProductCode;
			public const string ProductDesc = SchemaRoot + WhsDocketLine.Schema.ProductDesc;
			public const string WE_PackQuantity = SchemaRoot + WhsDocketLine.Schema.WE_PackQuantity;
			public const string WE_F3_NKPackType = SchemaRoot + AutoWhsDocketLine.Schema.WE_F3_NKPackType;
			public const string WE_TransactionQuantity = SchemaRoot + AutoWhsDocketLine.Schema.WE_TransactionQuantity;
			public const string WE_ClientOrderedUnits = SchemaRoot + AutoWhsDocketLine.Schema.WE_ClientOrderedUnits;
			public const string ProductUQ = SchemaRoot + WhsDocketLine.Schema.ProductUQ;
			public const string WE_OP = SchemaRoot + AutoWhsDocketLine.Schema.WE_OP;
			public const string WE_PartAttrib1 = SchemaRoot + AutoWhsDocketLine.Schema.WE_PartAttrib1;
			public const string WE_PartAttrib2 = SchemaRoot + AutoWhsDocketLine.Schema.WE_PartAttrib2;
			public const string WE_PartAttrib3 = SchemaRoot + AutoWhsDocketLine.Schema.WE_PartAttrib3;
			public const string WE_SerialNumber = SchemaRoot + AutoWhsDocketLine.Schema.WE_SerialNumber;
			public const string WE_WD = SchemaRoot + AutoWhsDocketLine.Schema.WE_WD;
			public const string WE_ExpiryDate = SchemaRoot + AutoWhsDocketLine.Schema.WE_ExpiryDate;
			public const string WE_PackingDate = SchemaRoot + AutoWhsDocketLine.Schema.WE_PackingDate;
			public const string WE_LineNo = SchemaRoot + AutoWhsDocketLine.Schema.WE_LineNo;
			public const string WE_PK = SchemaRoot + "PK";
		}

		public const string SchemaRoot = "WhsReceiveLine.";
		public const string SchemaPath = "WhsReceiveLine+";

		public static string GetSchemaPath(string root, bool isRoot = false)
		{
			return isRoot ? SchemaRoot : SchemaPath + root;
		}

		#endregion

		TrackingWhsReceiveLine(WhsReceiveLine receiveLine) : base(receiveLine.Factory)
		{
			WhsReceiveLine = receiveLine;
		}

		#region Flyweight & Wrapped Object

		public WhsReceiveLine WhsReceiveLine { get; }

		public static TrackingWhsReceiveLine GetTrackingWhsReceiveLine(WhsReceiveLine receiveLine)
		{
			Argument.NotNull(receiveLine, "WhsReceiveLine");
			var result = receiveLine.Factory.GetCachedValue("TrackingWhsReceiveLine|" + receiveLine.PK, GetNewTrackingWhsReceiveLine);

			if (result.IsDeleted)
			{
				result.ResetState();
			}

			return result;

			TrackingWhsReceiveLine GetNewTrackingWhsReceiveLine()
			{
				var newLine = new TrackingWhsReceiveLine(receiveLine);
				newLine.RegisterEditableChildObject(receiveLine);

				return newLine;
			}
		}

		#endregion

		public TrackingWhsReceive Docket
		{
			get { return TrackingHelper.Get(WhsReceiveLine.Docket); }
		}

		[ChildEditable(true)]
		public TrackingWhsInventoryCollection Inventory
		{
			get { return (TrackingWhsInventoryCollection)WhsReceiveLine.Inventory; }
		}

		WhsInventoryViewCollection IWhsReceiveLineWrapperStrategy.GetNewWhsInventoryCollection()
		{
			return new TrackingWhsInventoryCollection(Factory, this);
		}

		WhsInventoryView IWhsReceiveLineWrapperStrategy.LoadInventoryView(ZQuery query)
		{
			return Factory.LoadTop1<TrackingWhsInventory>(query);
		}

		WhsInventoryView IWhsReceiveLineWrapperStrategy.GetNewInventoryView()
		{
			return Factory.NewWithPrimaryKey<TrackingWhsInventory>(WhsReceiveLine.PK.ToGuid());
		}

		#region Implementation of IWrappedBizOProvider

		public BusinessObject GetWrappedBizO()
		{
			return WhsReceiveLine;
		}

		public string GetWrappedBindTo(string bindTo)
		{
			return GetSchemaPath(bindTo);
		}

		#endregion

		public ZShort WE_LineNo => WhsReceiveLine.WE_LineNo;

		public override void Delete()
		{
			base.Delete();

			if (!WhsReceiveLine.IsDeleted)
			{
				WhsReceiveLine.Delete();
			}
		}

		public override bool IsInDatabase
		{
			get { return WhsReceiveLine.IsInDatabase; }
		}

		public override bool HasChanges
		{
			get { return base.HasChanges || WhsReceiveLine.HasChanges; }
			set { base.HasChanges = WhsReceiveLine.HasChanges = value; }
		}

		protected override BusinessObject LogsAndNotesTarget { get { return WhsReceiveLine; } }

		#region Cloning support

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var clone = (WhsReceiveLine)WhsReceiveLine.Clone();

			return new TrackingWhsReceiveLine(clone);
		}

		#endregion
	}
}
