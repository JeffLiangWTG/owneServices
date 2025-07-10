using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.Tracking.Business
{
	public class TrackingWhsOrderLine : NonPersistentBusinessObjectWithLogsAndNotes, IWhsOrderLineWrapperStrategy, IWrappedBizOProvider
	{
		#region Schema

		[SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Follow same inheritance as the containing class")]
		public abstract class Schema : WhsOrderLine.Schema { }

		public abstract class WrapperSchema
		{
			public const string ProductDescription = "ProductDescription";
			public const string AllocatedQuantityAsString = "AllocatedQuantityAsString";
			public const string UnallocatedQuantity = "UnallocatedQuantity";
			public const string ReleaseDetails = "ReleaseDetails";
			public const string ProductCode = SchemaRoot + WhsDocketLine.Schema.ProductCode;
			public const string WE_PackQuantity = SchemaRoot + WhsDocketLine.Schema.WE_PackQuantity;
			public const string WE_F3_NKPackType = SchemaRoot + AutoWhsDocketLine.Schema.WE_F3_NKPackType;
			public const string WE_TransactionQuantity = SchemaRoot + AutoWhsDocketLine.Schema.WE_TransactionQuantity;
			public const string ProductUQ = SchemaRoot + WhsDocketLine.Schema.ProductUQ;
			public const string WE_OP = SchemaRoot + AutoWhsDocketLine.Schema.WE_OP;
			public const string WE_ShortfallQuantityCached = SchemaRoot + WhsPickableDocketLine.Schema.WE_ShortfallQuantityCached;
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

		public const string SchemaRoot = "WhsOrderLine.";
		public const string SchemaPath = "WhsOrderLine+";

		public static string GetSchemaPath(string root, bool isRoot = false)
		{
			return isRoot ? SchemaRoot : SchemaPath + root;
		}

		#endregion

		public TrackingWhsOrderLine(WhsOrderLine orderLine) : base(orderLine.Factory)
		{
			this.orderLine = orderLine;
		}

		#region Flyweight & Wrapped Object

		readonly WhsOrderLine orderLine;
		public WhsOrderLine WhsOrderLine
		{
			get { return orderLine; }
		}

		public static TrackingWhsOrderLine GetTrackingWhsOrderLine(WhsOrderLine orderLine)
		{
			Argument.NotNull(orderLine, "WhsOrderLine");
			var result = orderLine.Factory.GetCachedValue("TrackingWhsOrderLine|" + orderLine.PK, () => new TrackingWhsOrderLine(orderLine));

			if (result.IsDeleted)
			{
				result.ResetState();
			}

			return result;
		}

		public override void Delete()
		{
			base.Delete();

			if (!WhsOrderLine.IsDeleted)
			{
				WhsOrderLine.Delete();
			}
		}

		public override bool IsInDatabase
		{
			get { return WhsOrderLine.IsInDatabase; }
		}

		public override bool HasChanges
		{
			get { return base.HasChanges || WhsOrderLine.HasChanges; }
			set { base.HasChanges = WhsOrderLine.HasChanges = value; }
		}

		public ZShort WE_LineNo => WhsOrderLine.WE_LineNo;

		public new ZGuid PK { get { return WhsOrderLine.PK; } }

		#endregion

		#region Implementation of IWrappedBizOProvider

		public BusinessObject GetWrappedBizO()
		{
			return WhsOrderLine;
		}

		public string GetWrappedBindTo(string bindTo)
		{
			return GetSchemaPath(bindTo);
		}

		#endregion

		#region Order

		TrackingWhsOrder order;
		public TrackingWhsOrder Order
		{
			get { return order ?? (order = TrackingHelper.Get(WhsOrderLine.Order)); }
		}

		#endregion

		[MaxLength(AutoOrgSupplierPart.Schema.OP_DescMaxLength)]
		public ZString ProductDescription
		{
			get { return WhsOrderLine.SupplierPart != null ? WhsOrderLine.SupplierPart.OP_Desc : ZString.Empty; }
		}

		public ZPropertyInfo ProductDescriptionInfo
		{
			get { return GetZPropertyInfo(WrapperSchema.ProductDescription); }
		}

		public ZString AllocatedQuantityAsString
		{
			get { return WhsOrderLine.SupplierPart != null ? WhsOrderLine.WE_CrossDockQuantity.ToString(WhsOrderLine.SupplierPart.OP_CountDecimalPlaces) : string.Empty; }
		}

		public ZPropertyInfo AllocatedQuantityAsStringInfo
		{
			get { return GetZPropertyInfo(WrapperSchema.AllocatedQuantityAsString); }
		}

		public ZDecimal UnallocatedQuantity
		{
			get { return WhsOrderLine.WE_TransactionQuantity - WhsOrderLine.WE_CrossDockQuantity; }
		}

		public ZPropertyInfo UnallocatedQuantityInfo
		{
			get { return GetZPropertyInfo(WrapperSchema.UnallocatedQuantity); }
		}

		public TrackingWhsReleaseLineCollection ReleaseDetails
		{
			get { return ReleaseDetailsCollectionCore(); }
		}

		TrackingWhsReleaseLineCollection ReleaseDetailsCollectionCore()
		{
			var attributeManager = WhsOrderLine.Docket?.Client.PartAttributeManager;
			var decimalPlaces = (int)(WhsOrderLine.SupplierPart?.OP_CountDecimalPlaces ?? 3);

			if (WhsOrderLine.ReleaseLines.Count > 0)
			{
				var collection = new TrackingWhsReleaseLineCollection(WhsOrderLine.ReleaseLines, attributeManager, decimalPlaces);
				foreach (TrackingWhsReleaseLine releaseLine in collection)
				{
					SetOrderLineProperties(releaseLine);

					var unitsUQ = releaseLine.W1_UnitsUQ;
					var unitsQName = WhsOrderLine.Lookups.PackTypes.GetDescriptionFromCode(unitsUQ);
					releaseLine.UnitsQName = unitsQName.IsNullOrEmpty() ? unitsUQ : unitsQName;
					releaseLine.QtyOrdered = releaseLine.W1_Units;
				}

				return collection;
			}
			else
			{
				var releaseLine = new TrackingWhsReleaseLine(null, attributeManager, decimalPlaces);
				SetOrderLineProperties(releaseLine);
				releaseLine.QtyOrdered = WhsOrderLine.WE_TransactionQuantity.ToString(decimalPlaces);

				var productUQ = WhsOrderLine.ProductUQ;
				var unitsQName = WhsOrderLine.Lookups.PackTypes.GetDescriptionFromCode(productUQ);
				releaseLine.UnitsQName = unitsQName.IsNullOrEmpty() ? productUQ : unitsQName;
				releaseLine.W1_Units = WhsOrderLine.SumOfUnitsMet.ToString(decimalPlaces);
				if (attributeManager != null && attributeManager.IsExpiryDateUsedByOrganisation)
				{
					releaseLine.W1_ExpiryDate = WhsOrderLine.WE_ExpiryDate.ToShortDateString();
				}

				if (attributeManager != null && attributeManager.IsPackingDateUsedByOrganisation)
				{
					releaseLine.W1_PackingDate = WhsOrderLine.WE_PackingDate.ToShortDateString();
				}

				releaseLine.W1_PartAttrib1 = WhsOrderLine.WE_PartAttrib1;
				releaseLine.W1_PartAttrib2 = WhsOrderLine.WE_PartAttrib2;
				releaseLine.W1_PartAttrib3 = WhsOrderLine.WE_PartAttrib3;
				releaseLine.W1_SerialNumber = WhsOrderLine.WE_SerialNumber;

				var collection = new TrackingWhsReleaseLineCollection(Factory);
				collection.Add(releaseLine);

				return collection;
			}
		}

		void SetOrderLineProperties(TrackingWhsReleaseLine releaseLine)
		{
			releaseLine.ProductCode = WhsOrderLine.ProductCode;
			releaseLine.ProductDescription = ProductDescription;
			releaseLine.Packs = WhsOrderLine.WE_PackQuantity;

			var packType = WhsOrderLine.WE_F3_NKPackType;
			releaseLine.PacksUQ = WhsOrderLine.Lookups.PackTypes.GetDescriptionFromCode(packType) ?? packType;
		}

		#region Clone

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var line = WhsOrderLine.Clone();
			var trackingLine = TrackingHelper.Get(line);
			trackingLine.CopyValuesFrom(this);
			trackingLine.WhsOrderLine.WE_WD = WhsOrderLine.WE_WD;
			return trackingLine;
		}

		#endregion

		#region CriticalFieldInfos

		public IEnumerable<ZPropertyInfo> CriticalFieldInfos
		{
			get
			{
				// tested in TrackingWhsOrder
				return new[]
				{
					WhsOrderLine.WE_TransactionQuantityInfo,
					WhsOrderLine.WE_F3_NKPackTypeInfo,
					WhsOrderLine.WE_OPInfo,
				};
			}
		}

		#endregion

		#region Implementation of IWhsOrderLineWrapperStrategy

		void IWhsOrderLineWrapperStrategy.WE_TransactionQuantity_SetAfter(bool valueChanged)
		{
			if (valueChanged)
			{
				WhsOrderLine.ClearWE_ShortfallQuantityCached();
			}
		}

		bool IWhsOrderLineWrapperStrategy.ShouldUpdateWeightAndVolumeOfDocketFromDocketLine
		{
			get { return !WhsOrderLine.IsComponentLineOnSalesOrder; }
		}

		#endregion

		#region Overrides of NonPersistentBusinessObjectWithLogsAndNotes

		protected override BusinessObject LogsAndNotesTarget { get { return WhsOrderLine; } }

		#endregion
	}
}
